using UnityEngine;
using UnityEditor;

namespace GogoGaga.OptimizedRopesAndCables
{
    [CustomEditor(typeof(Rope))]
    public class RopeEditor : Editor
    {
        private Rope component;
        private SerializedProperty startPoint;
        private SerializedProperty midPoint;
        private SerializedProperty endPoint;
        private SerializedProperty linePoints;
        private SerializedProperty ropeWidth;
        private SerializedProperty stiffness;
        private SerializedProperty damping;
        private SerializedProperty ropeLength;
        private SerializedProperty midPointPosition;
        private SerializedProperty midPointWeight;

        private void OnEnable()
        {
            component = (Rope)target;
            startPoint = serializedObject.FindProperty("startPoint");
            midPoint = serializedObject.FindProperty("midPoint");
            endPoint = serializedObject.FindProperty("endPoint");
            linePoints = serializedObject.FindProperty(nameof(Rope.linePoints));
            ropeWidth = serializedObject.FindProperty(nameof(Rope.ropeWidth));
            stiffness = serializedObject.FindProperty(nameof(Rope.stiffness));
            damping = serializedObject.FindProperty(nameof(Rope.damping));
            ropeLength = serializedObject.FindProperty(nameof(Rope.ropeLength));
            midPointPosition = serializedObject.FindProperty(nameof(Rope.midPointPosition));
            midPointWeight = serializedObject.FindProperty(nameof(Rope.midPointWeight));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Label();
            RopeTransforms();
            RopeProperties();
            RopeCurve();

            if (GUILayout.Button("Add Mesh"))
            {
                AddMeshComponents();
            }

            if (GUILayout.Button("Add Wind Effect"))
            {
                if (!component.GetComponent<RopeWindEffect>())
                {
                    component.gameObject.AddComponent<RopeWindEffect>();
                }
            }

            serializedObject.ApplyModifiedProperties(); // Apply changes
        }

        private void Label()
        {
            string centeredText = "OPTIMIZED ROPE AND CABLE";
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            float availableWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth;
            buttonStyle.fontSize = (int)Mathf.Min(availableWidth / (centeredText.Length * 0.5f), 48);

            if (GUILayout.Button(centeredText, buttonStyle))
            {
                Application.OpenURL("https://u3d.as/3iRX");
            }
        }

        private void RopeTransforms()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ROPE HANDLES", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            EditorGUILayout.PropertyField(startPoint, new GUIContent("Rope Start"));
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(midPoint, new GUIContent("Mid Point (Optional)", "This will move at the center hanging from the rope, like a necklace, for example"));
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(endPoint, new GUIContent("Rope End"));
            EditorGUILayout.Space(2);

            CreateTransforms();

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        private void CreateTransforms()
        {
            if (!component.StartPoint && !component.EndPoint)
            {
                if (GUILayout.Button("Create Points"))
                {
                    var newStartPoint = new GameObject("Start Point").transform;
                    
                    newStartPoint.parent = component.transform;
                    newStartPoint.localPosition = component.transform.forward * 2;
                    component.SetStartPoint(newStartPoint,true);

                    var newEndPoint = new GameObject("End Point").transform;
                    newEndPoint.parent = component.transform;
                    newEndPoint.localPosition = -component.transform.forward * 2;
                    component.SetEndPoint(newEndPoint,true);
                    
                    if (!component.MidPoint)
                    {
                        var newMidPoint = new GameObject("Mid Point").transform;
                        newMidPoint.parent = component.transform;
                        newMidPoint.localPosition = -component.transform.up * 2;
                        component.SetMidPoint(newMidPoint,true);
                    }

                    serializedObject.Update(); // Update serialized object to reflect changes
                }
            }
        }

        private void RopeProperties()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ROPE PROPERTIES", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            EditorGUILayout.PropertyField(linePoints, new GUIContent("Line Quality", "How many points should the rope have, 2 would be a triangle with straight lines, 100 would be a very flexible rope with many parts"));

            EditorGUILayout.PropertyField(ropeWidth, new GUIContent("Rope Width", "The Rope width set at start (changing this value during run time will produce no effect)"));
            component.ropeWidth = component.ropeWidth < 0.001f ? 0.001f : component.ropeWidth;

            EditorGUILayout.PropertyField(stiffness, new GUIContent("Rope Stiffness", "Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one"));
            component.stiffness = component.stiffness < 1f ? 1f : component.stiffness;

            EditorGUILayout.PropertyField(damping, new GUIContent("Rope Dampness", "0 is no damping, 50 is a lot"));
            component.damping = component.damping < 0.01f ? 0.01f : component.damping;

            EditorGUILayout.PropertyField(ropeLength, new GUIContent("Rope Length", "How long is the rope, it will hang more or less from starting point to end point depending on this value"));
            component.ropeLength = component.ropeLength < 0.01f ? 0.01f : component.ropeLength;

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        private void RopeCurve()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ROPE CURVE", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            EditorGUILayout.PropertyField(midPointPosition, new GUIContent("Midpoint", "Position of the midpoint along the line between start and end points"));
            EditorGUILayout.PropertyField(midPointWeight, new GUIContent("Midpoint Influence", "Adjust the middle control point weight for the Rational Bezier curve"));

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        private void AddMeshComponents()
        {
            if (!component.GetComponent<MeshRenderer>())
            {
                MeshRenderer meshRenderer = component.gameObject.AddComponent<MeshRenderer>();

                for (int i = 0; i < 10; i++)
                {
                    UnityEditorInternal.ComponentUtility.MoveComponentUp(meshRenderer);
                }
            }

            if (!component.GetComponent<MeshFilter>())
            {
                MeshFilter meshFilter = component.gameObject.AddComponent<MeshFilter>();

                for (int i = 0; i < 10; i++)
                {
                    UnityEditorInternal.ComponentUtility.MoveComponentUp(meshFilter);
                }
            }

            if (!component.GetComponent<RopeMesh>())
            {
                component.gameObject.AddComponent<RopeMesh>();
            }

            if (component.TryGetComponent(out LineRenderer lineRenderer))
            {
                lineRenderer.enabled = false;
            }
        }
    }
}
