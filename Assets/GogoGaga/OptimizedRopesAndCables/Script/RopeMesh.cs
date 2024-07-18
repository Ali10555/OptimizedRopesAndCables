using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GogoGaga.OptimizedRopesAndCables
{
    [RequireComponent(typeof (MeshFilter)), RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(Rope))]
    public class RopeMesh : MonoBehaviour
    {
        [Range(0,1)]
        public float Weight = 0;
        public int OverallDivision = 3;
        public float ropeWidth = 0.3f;
        public int radialDivision = 8;
        public Material material;
        Rope rope;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh ropeMesh;

        private void OnValidate()
        {
            if (!rope)
                rope = GetComponent<Rope>();
            if (!meshFilter)
                meshFilter = GetComponent<MeshFilter>();
            if (!meshRenderer)
                meshRenderer = GetComponent<MeshRenderer>();

            meshRenderer.material = material;
        }


        private void Awake()
        {
            if (!rope)
                rope = GetComponent<Rope>();
            if (!meshFilter)
                meshFilter = GetComponent<MeshFilter>();
            if (!meshRenderer)
                meshRenderer = GetComponent<MeshRenderer>();
        }


        public void CreateRopeMesh(Vector3[] points, float radius, int segmentsPerWire)
        {
            // Validate input
            if (points == null || points.Length < 2)
            {
                Debug.LogError("Need at least two points to create a rope mesh.");
                return;
            }

            // Create a list to hold vertices and triangles
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            // Generate vertices for each segment along the points
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 direction = Vector3.forward;
                if (i < points.Length - 1)
                    direction = points[i + 1] - points[i];
                else
                    direction = points[i] - points[i - 1];

                Quaternion rotation = Quaternion.LookRotation(direction);

                // Create vertices around a circle at this point
                for (int j = 0; j <= segmentsPerWire; j++)
                {
                    float angle = j * Mathf.PI * 2f / segmentsPerWire;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                    vertices.Add(points[i] + rotation * offset);
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

            // Generate vertices and triangles for the start cap
            int startCapCenterIndex = vertices.Count;
            vertices.Add(points[0]);
            Quaternion startRotation = Quaternion.LookRotation(points[1] - points[0]);
            for (int j = 0; j <= segmentsPerWire; j++)
            {
                float angle = j * Mathf.PI * 2f / segmentsPerWire;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                vertices.Add(points[0] + startRotation * offset);

                if (j < segmentsPerWire)
                {
                    triangles.Add(startCapCenterIndex);
                    triangles.Add(startCapCenterIndex + j + 2);
                    triangles.Add(startCapCenterIndex + j + 1);
                }
            }

            // Generate vertices and triangles for the end cap
            int endCapCenterIndex = vertices.Count;
            vertices.Add(points[points.Length - 1]);
            Quaternion endRotation = Quaternion.LookRotation(points[points.Length - 1] - points[points.Length - 2]);
            for (int j = 0; j <= segmentsPerWire; j++)
            {
                float angle = j * Mathf.PI * 2f / segmentsPerWire;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                vertices.Add(points[points.Length - 1] + endRotation * offset);

                if (j < segmentsPerWire)
                {
                    triangles.Add(endCapCenterIndex);
                    triangles.Add(endCapCenterIndex + j + 1);
                    triangles.Add(endCapCenterIndex + j + 2);
                }
            }

            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();

            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            // Assign the mesh to the MeshFilter
            meshFilter.mesh = mesh;
        }

        void GenerateMesh()
        {
            Vector3[] points = new Vector3[OverallDivision+ 1];
            for (int i = 0; i < points.Length; i++)
                points[i] = rope.GetPointAt(i / (float)OverallDivision);
            CreateRopeMesh(points, ropeWidth, radialDivision);
        }

        void Update()
        {
            GenerateMesh();
        }

        private void OnDrawGizmos()
        {
            
        }
    }
}