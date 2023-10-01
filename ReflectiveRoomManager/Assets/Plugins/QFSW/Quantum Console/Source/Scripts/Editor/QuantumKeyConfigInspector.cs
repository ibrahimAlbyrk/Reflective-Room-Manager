using UnityEditor;
using UnityEngine;

namespace QFSW.QC.Editor
{
    [CustomEditor(typeof(QuantumKeyConfig))]
    public class QuantumKeyConfigInspector : QCInspectorBase
    {
        private QuantumKeyConfig _keyConfigInstance;

        private SerializedProperty _submitCommandKeyProperty;
        private SerializedProperty _hideConsoleKeyProperty;
        private SerializedProperty _showConsoleKeyProperty;
        private SerializedProperty _toggleConsoleVisibilityKeyProperty;

        private SerializedProperty _zoomInKeyProperty;
        private SerializedProperty _zoomOutKeyProperty;
        private SerializedProperty _dragConsoleKeyProperty;

        private SerializedProperty _suggestNextCommandKeyProperty;
        private SerializedProperty _suggestPreviousCommandKeyProperty;

        private SerializedProperty _nextCommandKeyProperty;
        private SerializedProperty _previousCommandKeyProperty;

        private SerializedProperty _cancelActionsKeyProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            _keyConfigInstance = (QuantumKeyConfig)target;

            _submitCommandKeyProperty = serializedObject.FindProperty("SubmitCommandKey");
            _hideConsoleKeyProperty = serializedObject.FindProperty("HideConsoleKey");
            _showConsoleKeyProperty = serializedObject.FindProperty("ShowConsoleKey");
            _toggleConsoleVisibilityKeyProperty = serializedObject.FindProperty("ToggleConsoleVisibilityKey");

            _zoomInKeyProperty = serializedObject.FindProperty("ZoomInKey");
            _zoomOutKeyProperty = serializedObject.FindProperty("ZoomOutKey");
            _dragConsoleKeyProperty = serializedObject.FindProperty("DragConsoleKey");

            _suggestNextCommandKeyProperty = serializedObject.FindProperty("SuggestNextCommandKey");
            _suggestPreviousCommandKeyProperty = serializedObject.FindProperty("SuggestPreviousCommandKey");

            _nextCommandKeyProperty = serializedObject.FindProperty("NextCommandKey");
            _previousCommandKeyProperty = serializedObject.FindProperty("PreviousCommandKey");

            _cancelActionsKeyProperty = serializedObject.FindProperty("CancelActionsKey");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorHelpers.DrawHeader(Banner);

            EditorGUILayout.LabelField(new GUIContent("General"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_submitCommandKeyProperty, new GUIContent("Submit Command", "The key to submit and invoke the current console input."));
            EditorGUILayout.PropertyField(_showConsoleKeyProperty, new GUIContent("Show Console", "The key used to show and activate the console."));
            EditorGUILayout.PropertyField(_hideConsoleKeyProperty, new GUIContent("Hide Console", "The key used to hide and deactivate the console."));
            EditorGUILayout.PropertyField(_toggleConsoleVisibilityKeyProperty, new GUIContent("Toggle Console", "The key used to toggle the active and visibility state of the console."));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("UI"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_zoomInKeyProperty, new GUIContent("Zoom In", "Zooms in the console scaling."));
            EditorGUILayout.PropertyField(_zoomOutKeyProperty, new GUIContent("Zoom Out", "Zooms out the console scaling."));
            EditorGUILayout.PropertyField(_dragConsoleKeyProperty, new GUIContent("Drag Console", "Drags the console window with the cursor."));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Command Suggestions"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_suggestNextCommandKeyProperty, new GUIContent("Show Suggested Commands", "The key to show and suggest commands based on the current console input."));
            EditorGUILayout.PropertyField(_suggestPreviousCommandKeyProperty, new GUIContent("Show Previous Suggestion", "The key to show the previous suggestion."));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Command History"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_nextCommandKeyProperty, new GUIContent("Select Next Command", "The key to be used to select the next command from the console history."));
            EditorGUILayout.PropertyField(_previousCommandKeyProperty, new GUIContent("Select Previous Command", "The key to be used to select the previous command from the console history."));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Actions"), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_cancelActionsKeyProperty, new GUIContent("Cancel Actions", "Cancels any actions currently executing."));

            serializedObject.ApplyModifiedProperties();
        }
    }
}