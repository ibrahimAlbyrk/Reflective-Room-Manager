using UnityEditor;
using REFLECTIVE.Runtime.Singleton;

namespace REFLECTIVE.Editor.NETWORK.Room
{
    using Utilities;
    
    [CustomEditor(typeof(RoomSingleton<>), true)]
    public class RoomSingletonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            CustomEditorUtilities.DrawReflectionTitle("ROOM SINGLETON");
            
            CustomEditorUtilities.DrawDefaultInspector(serializedObject);
        }
    }
}