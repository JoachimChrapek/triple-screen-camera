using UnityEditor;

namespace FazApp.TripleScreenCamera.Editor
{
    [CustomEditor(typeof(TripleScreenCameraController))]
    public class TripleScreenCameraControllerEditor : UnityEditor.Editor
    {
        private TripleScreenCameraController targetController;
        
        private const float HeaderSpace = 15.0f;

        public override void OnInspectorGUI()
        {
            if (targetController == null)
            {
                targetController = target as TripleScreenCameraController;
                targetController.UpdateAfterSettingsChange();
            }
            
            DrawCameraSettingsSection();
            DrawRuntimeSection();
            DrawComponentsSection();
            DrawDebugSection();
        
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCameraSettingsSection()
        {
            DrawHeader("Camera settings", false);
            EditorGUILayout.HelpBox("All distance units are in meters", MessageType.Info);
        
            EditorGUI.BeginChangeCheck();
        
            DrawProperty("screenSetup");
            DrawProperty("aspectRatio");
            DrawProperty("displayWidth");
            DrawProperty("distanceFromCenterDisplay");

            if (targetController.ScreenSetup == ScreenSetupType.TripleScreen)
            {
                DrawProperty("lateralDisplaysAngle");
                DrawProperty("lateralDisplaysMargin");
            }
        
            DrawProperty("nearClippingPlane");
            DrawProperty("farClippingPlane");
        
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                targetController.UpdateAfterSettingsChange();
            }
        }

        private void DrawRuntimeSection()
        {
            DrawHeader("Runtime");
        
            EditorGUILayout.HelpBox("Updates cameras settings automatically after changing settings properties. Can cause performance issues. It is recommended to use it only for testing", MessageType.Info);
            DrawProperty("autoUpdate");
        }

        private void DrawComponentsSection()
        {
            DrawHeader("Components");
        
            DrawProperty("centerCameraFrustumController");
            DrawProperty("leftCameraFrustumController");
            DrawProperty("rightCameraFrustumController");
        }

        private void DrawDebugSection()
        {
            DrawHeader("Debug");
        
            DrawProperty("drawDebugDisplayGizmos");
            DrawProperty("drawDebugCameraFrustumGizmos");
        }
    
        private void DrawHeader(string text, bool withSpace = true)
        {
            if (withSpace)
            {
                EditorGUILayout.Space(HeaderSpace);
            }
        
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }
    
        private void DrawProperty(string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                EditorGUILayout.LabelField($"Couldn't find property: {property}");
                return;
            }

            EditorGUILayout.PropertyField(property);
        }
    }
}