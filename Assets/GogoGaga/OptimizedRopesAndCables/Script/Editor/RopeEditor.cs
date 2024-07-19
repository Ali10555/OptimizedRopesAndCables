using UnityEngine;
using UnityEditor;

namespace GogoGaga.OptimizedRopesAndCables
{

    [CustomEditor(typeof(Rope))]
    public class RopeEditor : Editor
    {
        Rope component;
        LineRenderer lineRenderer;
        public override void OnInspectorGUI()
        {
            component = (Rope)target;
            if(component == null)
                return;

            Undo.RecordObject(component, "Waypoint Indicator: " + component.name);

            serializedObject.Update();

            Label();
            RopeTransforms();
            RopeProperties();
            RopeCurve();

            if (GUILayout.Button("Add Mesh"))
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

                if(component.TryGetComponent(out LineRenderer lineRenderer))
                {
                    lineRenderer.enabled = false;
                }
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

        void Label()
        {
            string centeredText = "OPTIMIZED ROPE AND CABLE";
            Vector3 minSize = new Vector2(300, 100); // Set a minimum window size
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14, // Adjust font size as needed
                fontStyle = FontStyle.Bold // Optional  
            };


            float availableWidth = EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth;

            // Adjust font size to fit available width (consider character count)
            buttonStyle.fontSize = (int)Mathf.Min(availableWidth / (centeredText.Length * 0.5f), 48);  // Adjust factor based on font

            if (GUILayout.Button(centeredText, buttonStyle))
            {
                Application.OpenURL("https://u3d.as/3iRX");
            }
        }

        void RopeTransforms()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("ROPE HANDLES",EditorStyles.boldLabel) ;
            
            EditorGUILayout.Space(2);


            //Start Point...
            SerializedProperty startPoint = serializedObject.FindProperty(
                nameof(component.startPoint)
                );

            startPoint.objectReferenceValue = EditorGUILayout.ObjectField(
                        "Rope Start",
                        startPoint.objectReferenceValue,
                        typeof(Transform), true);


            EditorGUILayout.Space(2);


            //Mid Point...
            SerializedProperty midPoint = serializedObject.FindProperty(
               nameof(component.midPoint)
               );

            midPoint.objectReferenceValue = EditorGUILayout.ObjectField(
                        new GUIContent("Mid Point (Optional)", "This will move at the center hanging from the rope, like a necklace, for example"),
                        midPoint.objectReferenceValue,
                        typeof(Transform), true);

            EditorGUILayout.Space(2);
            

            //End Point...
            SerializedProperty endPoint = serializedObject.FindProperty(
                nameof(component.endPoint)
                );

            endPoint.objectReferenceValue = EditorGUILayout.ObjectField(
                        "Rope End",
                        endPoint.objectReferenceValue,
                        typeof(Transform), true);

            EditorGUILayout.Space(2);

            CreateTransforms();

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        void CreateTransforms()
        {
            if(!component.startPoint && !component.endPoint)
            {
                if(GUILayout.Button("Create Points"))
                {
                    component.startPoint = new GameObject("Start Point").transform;
                    component.startPoint.parent = component.transform;
                    component.startPoint.localPosition = component.transform.forward * 2;

                    component.endPoint = new GameObject("End Point").transform;
                    component.endPoint.parent = component.transform;
                    component.endPoint.localPosition = -component.transform.forward * 2;

                    if (!component.midPoint)
                    {
                        component.midPoint = new GameObject("Mid Point").transform;
                        component.midPoint.parent = component.transform;
                        component.midPoint.localPosition = -component.transform.up * 2;
                    }
                }
            }
        }

        void RopeProperties()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ROPE PROPERTIES", EditorStyles.boldLabel);

            EditorGUILayout.Space(2);


            EditorGUILayout.LabelField(new GUIContent("Line Quality", "How many points should the rope have, 2 would be a triangle with straight lines, 100 would be a very flexible rope with many parts"));
            component.linePoints = EditorGUILayout.IntSlider(
                component.linePoints,
                2, 100
                );

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Rope Widht", "The Rope width set at start (changing this value during run time will produce no effect)"));
            component.ropeWidth = EditorGUILayout.Slider(
                component.ropeWidth,
                0.01f, 10
                );

            if (EditorGUI.EndChangeCheck())
            {
                LineRenderer l= component.GetComponent<LineRenderer>();
                l.startWidth = component.ropeWidth;
                l.endWidth = component.ropeWidth;
            }


            EditorGUILayout.LabelField(new GUIContent("Rope Stiffness", "Value highly dependent on use case, a metal cable would have high stiffness, a rubber rope would have a low one"));
            component.stiffness = EditorGUILayout.Slider(
                component.stiffness,
                0, 1000
                );


            EditorGUILayout.LabelField(new GUIContent("Rope Dampness", "How long is the rope, it will hang more or less from starting point to end point depending on this value"));
            component.damping = EditorGUILayout.Slider(
                component.damping,
                0.01f, 100
                );


            EditorGUILayout.LabelField(new GUIContent("Rope Lenght", "How long is the rope, it will hang more or less from starting point to end point depending on this value"));
            component.ropeLength = EditorGUILayout.Slider(
                component.ropeLength,
                0.1f, 1000
                );



            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        void RopeCurve()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ROPE CURVE", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);


            EditorGUILayout.LabelField(new GUIContent("Midpoint", "Position of the midpoint along the line between start and end points"));
            component.midPointPosition = EditorGUILayout.Slider(
                component.midPointPosition,
                0.15f, 0.85f
                );


            EditorGUILayout.LabelField(new GUIContent("Midpoint Influence", "Adjust the middle control point weight for the Rational Bezier curve"));
            component.midPointWeight = EditorGUILayout.Slider(
                component.midPointWeight,
                1f, 20f
                );


            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        void Template()
        {
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("ROPE Properties", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);


            



            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }
    }
}