using UnityEditor;
using UnityEngine;

namespace REFLECTIVE.Editor.Utilities
{
    public static class CustomEditorUtilities
    {
        private static Texture2D _titleBackground;

        public static void DrawReflectionTitle(string title = "Reflection")
        {
            if (_titleBackground == null)
                _titleBackground = MakeTex(1, 1, Color.white);

            var titleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal =
                {
                    textColor = Color.black,
                    background = _titleBackground
                },
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12
            };
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(title, titleStyle);
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
        }
        
        public static void DrawDefaultInspector(SerializedObject serializedObject)
        {
            serializedObject.Update();

            var iterator = serializedObject.GetIterator();
            
            var enterChildren = true;
            
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                    continue;

                EditorGUILayout.PropertyField(iterator, true);
            }
            
            serializedObject.ApplyModifiedProperties();
            
            serializedObject.Update();
        }
        
        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];

            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
        
            var result = new Texture2D(width, height);
            result.hideFlags = HideFlags.HideAndDontSave;

            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}