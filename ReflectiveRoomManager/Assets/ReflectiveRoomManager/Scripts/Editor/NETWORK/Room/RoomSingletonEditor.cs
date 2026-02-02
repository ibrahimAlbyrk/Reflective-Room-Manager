using UnityEditor;
using UnityEngine.UIElements;
using REFLECTIVE.Editor.Utilities;
using REFLECTIVE.Runtime.Singleton;

namespace REFLECTIVE.Editor.NETWORK.Room
{
    [CustomEditor(typeof(RoomSingleton<>), true)]
    public class RoomSingletonEditor : ReflectiveEditorBase
    {
        protected override string GetTitle() => "ROOM SINGLETON";

        protected override void BuildInspectorUI(VisualElement root)
        {
            AddDefaultProperties(root);
        }
    }
}
