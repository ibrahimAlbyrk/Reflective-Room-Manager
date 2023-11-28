using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Singleton;

namespace ReflectiveRoomManager.Scripts.Editor.NETWORK.Room
{
    [CustomEditor(typeof(RoomSingleton<>), true)]
    public class RoomSingletonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var myStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal =
                {
                    textColor = Color.black,
                    background = MakeTex(1, 1, Color.white)
                },
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12
            };

            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("Room Singleton", myStyle);
            
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
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
        }
        
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
        
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
}