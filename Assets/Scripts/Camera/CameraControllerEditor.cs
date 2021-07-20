using UnityEditor;
using UnityEngine;

namespace CamExtensions.Controller
{
    [CustomEditor(typeof(CameraController))]
    [CanEditMultipleObjects]
    public class CameraControllerEditor : Editor
    {
        GUISkin skin;
        CameraController tpCamera;

        void OnSceneGUI()
        {
            if (Application.isPlaying)
                return;
            tpCamera = (CameraController)target;
        }

        void OnEnable()
        {
            tpCamera = (CameraController)target;
            tpCamera.indexLookPoint = 0;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical("Lite Camera Controller by Invector", "window");

            GUILayout.Space(30);

            EditorGUILayout.BeginVertical();

            base.OnInspectorGUI();

            GUILayout.Space(10);

            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            GUILayout.Space(2);
        }
    }
}