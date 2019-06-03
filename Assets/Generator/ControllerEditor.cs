using UnityEngine;
using UnityEditor;
using System;

namespace Assets.Generator
{
    [CustomEditor(typeof(Controller))]
    public class ControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Controller controller = (Controller)target;
            if (GUILayout.Button("Generate"))
            {
                controller.Generate();
            }

            if (GUILayout.Button("Generate (New Seed)"))
            {
                controller.Seed = Guid.NewGuid().GetHashCode();
                controller.Generate();
            }
        }
    }
}
