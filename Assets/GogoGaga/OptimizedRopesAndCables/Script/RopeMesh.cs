using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GogoGaga.OptimizedRopesAndCables
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(Rope))]
    public class RopeMesh : MonoBehaviour
    {
        [Range(3, 25)] public int OverallDivision = 6;
        [Range(0.01f, 10)] public float ropeWidth = 0.3f;
        [Range(3, 20)] public int radialDivision = 8;
        [Tooltip("For now only base color is applied")]
        public Material material;
        [Tooltip("Tiling density per meter of the rope")]
        public float tilingPerMeter = 1.0f;

        private Rope rope;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh ropeMesh;
        private bool isStartOrEndPointMissing;

        // Use fields to store lists
        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();
        private List<Vector2> uvs = new List<Vector2>();

        private void OnValidate()
        {
            InitializeComponents();
            if (rope.IsPrefab)
                return;
            
            SubscribeToRopeEvents();
            if (meshRenderer && material)
            {
                meshRenderer.material = material;
            }
            // We are using delay call to generate mesh to avoid errors in the editor
            #if UNITY_EDITOR
            EditorApplication.delayCall += DelayedGenerateMesh;
            #endif
        }

        private void Awake()
        {
            InitializeComponents();
            SubscribeToRopeEvents();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                #if UNITY_EDITOR
                EditorApplication.delayCall += DelayedGenerateMesh;
                #endif
            }
            SubscribeToRopeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromRopeEvents();
            #if UNITY_EDITOR
            EditorApplication.delayCall -= DelayedGenerateMesh;
            #endif
        }

        private void InitializeComponents()
        {
            if (!rope)
                rope = GetComponent<Rope>();
            if (!meshFilter)
                meshFilter = GetComponent<MeshFilter>();
            if (!meshRenderer)
                meshRenderer = GetComponent<MeshRenderer>();

            CheckEndPoints();
        }

        private void CheckEndPoints()
        {
            // Check if start and end points are assigned
            if (gameObject.scene.rootCount == 0)
            {
                isStartOrEndPointMissing = false;
                return;
            }
            
            if (rope.StartPoint == null || rope.EndPoint == null)
            {
                isStartOrEndPointMissing = true;
                Debug.LogError("StartPoint or EndPoint is not assigned.", gameObject);
            }
            else
            {
                isStartOrEndPointMissing = false;
            }
        }

        private void SubscribeToRopeEvents()
        {
            UnsubscribeFromRopeEvents();
            if (rope != null)
            {
                rope.OnPointsChanged += GenerateMesh;
            }
        }

        private void UnsubscribeFromRopeEvents()
        {
            if (rope != null)
            {
                rope.OnPointsChanged -= GenerateMesh;
            }
        }

        public void CreateRopeMesh(Vector3[] points, float radius, int segmentsPerWire)
        {
            // Validate input
            if (points == null || points.Length < 2)
            {
                Debug.LogError("Need at least two points to create a rope mesh.", gameObject);
                return;
            }

            if (ropeMesh == null)
            {
                ropeMesh = new Mesh { name = "RopeMesh" };
                meshFilter.mesh = ropeMesh;
            }
            else
            {
                ropeMesh.Clear();
            }

            Vector3 gameObjectPosition = transform.position;

            // Clear the lists before using them
            vertices.Clear();
            triangles.Clear();
            uvs.Clear();

            float currentLength = 0f;

            // Generate vertices and UVs for each segment along the points
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 direction = i < points.Length - 1 ? points[i + 1] - points[i] : points[i] - points[i - 1];
                Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

                // Create vertices around a circle at this point
                for (int j = 0; j <= segmentsPerWire; j++)
                {
                    float angle = j * Mathf.PI * 2f / segmentsPerWire;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                    vertices.Add(transform.InverseTransformPoint(points[i] + rotation * offset));

                    float u = (float)j / segmentsPerWire;
                    float v = currentLength * tilingPerMeter;
                    uvs.Add(new Vector2(u, v));
                }

                if (i < points.Length - 1)
                {
                    currentLength += Vector3.Distance(points[i], points[i + 1]);
                }
            }

            // Generate triangles for each segment
            for (int i = 0; i < points.Length - 1; i++)
            {
                for (int j = 0; j < segmentsPerWire; j++)
                {
                    int current = i * (segmentsPerWire + 1) + j;
                    int next = current + 1;
                    int nextSegment = current + segmentsPerWire + 1;
                    int nextSegmentNext = nextSegment + 1;

                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(nextSegment);

                    triangles.Add(next);
                    triangles.Add(nextSegmentNext);
                    triangles.Add(nextSegment);
                }
            }

            // Generate vertices, triangles, and UVs for the start cap
            int startCapCenterIndex = vertices.Count;
            vertices.Add(transform.InverseTransformPoint(points[0]));
            uvs.Add(new Vector2(0.5f, 0)); // Center of the cap
            Quaternion startRotation = Quaternion.LookRotation(points[1] - points[0]);
            for (int j = 0; j <= segmentsPerWire; j++)
            {
                float angle = j * Mathf.PI * 2f / segmentsPerWire;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                vertices.Add(transform.InverseTransformPoint(points[0] + startRotation * offset));

                if (j < segmentsPerWire)
                {
                    triangles.Add(startCapCenterIndex);
                    triangles.Add(startCapCenterIndex + j + 1);
                    triangles.Add(startCapCenterIndex + j + 2);
                }

                uvs.Add(new Vector2((Mathf.Cos(angle) + 1) / 2, (Mathf.Sin(angle) + 1) / 2));
            }

            // Generate vertices, triangles, and UVs for the end cap
            int endCapCenterIndex = vertices.Count;
            vertices.Add(transform.InverseTransformPoint(points[points.Length - 1]));
            uvs.Add(new Vector2(0.5f, currentLength * tilingPerMeter)); // Center of the cap
            Quaternion endRotation = Quaternion.LookRotation(points[points.Length - 1] - points[points.Length - 2]);
            for (int j = 0; j <= segmentsPerWire; j++)
            {
                float angle = j * Mathf.PI * 2f / segmentsPerWire;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                vertices.Add(transform.InverseTransformPoint(points[points.Length - 1] + endRotation * offset));

                if (j < segmentsPerWire)
                {
                    triangles.Add(endCapCenterIndex);
                    triangles.Add(endCapCenterIndex + j + 1);
                    triangles.Add(endCapCenterIndex + j + 2);
                }

                uvs.Add(new Vector2((Mathf.Cos(angle) + 1) / 2, (Mathf.Sin(angle) + 1) / 2));
            }

            ropeMesh.vertices = vertices.ToArray();
            ropeMesh.triangles = triangles.ToArray();
            ropeMesh.uv = uvs.ToArray();
            ropeMesh.RecalculateNormals();
        }

        void GenerateMesh()
        {
            if (this == null || rope == null || meshFilter == null)
            {
                return;
            }

            if (isStartOrEndPointMissing)
            {
                if (meshFilter.sharedMesh != null)
                {
                    meshFilter.sharedMesh.Clear();
                }
                return;
            }

            Vector3[] points = new Vector3[OverallDivision + 1];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = rope.GetPointAt(i / (float)OverallDivision);
            }
            CreateRopeMesh(points, ropeWidth, radialDivision);
        }

        void Update()
        {
            if (rope.IsPrefab)
                return;

            if (Application.isPlaying)
            {
                GenerateMesh();
            }
        }

        private void DelayedGenerateMesh()
        {
            if (this != null)
            {
                GenerateMesh();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromRopeEvents();
            #if UNITY_EDITOR
            EditorApplication.delayCall -= DelayedGenerateMesh;
            #endif

            if (meshRenderer != null)
                Destroy(meshRenderer);
            if (meshFilter != null)
                Destroy(meshFilter);
        }
    }
}
