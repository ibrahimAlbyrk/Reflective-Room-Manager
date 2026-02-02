using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace REFLECTIVE.Editor.Utilities
{
    public abstract class ReflectiveEditorBase : UnityEditor.Editor
    {
        private const string CommonUSSPath =
            "Assets/ReflectiveRoomManager/Scripts/Editor/Styles/ReflectiveCommon.uss";

        protected virtual string[] AdditionalStyleSheets => System.Array.Empty<string>();

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.AddToClassList("reflective-root");

            LoadStyleSheets(root);

            var title = GetTitle();

            if (!string.IsNullOrEmpty(title))
                root.Add(CreateTitle(title));

            BuildInspectorUI(root);

            var footer = new VisualElement();
            footer.AddToClassList("footer-spacer");
            root.Add(footer);

            return root;
        }

        protected abstract string GetTitle();

        protected abstract void BuildInspectorUI(VisualElement root);

        private void LoadStyleSheets(VisualElement root)
        {
            var commonSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(CommonUSSPath);
            if (commonSheet != null) root.styleSheets.Add(commonSheet);

            foreach (var path in AdditionalStyleSheets)
            {
                var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (sheet != null) root.styleSheets.Add(sheet);
            }
        }

        protected static Label CreateTitle(string text)
        {
            var label = new Label(text);
            label.AddToClassList("reflective-title");
            return label;
        }

        protected static Label CreateSectionHeader(string text)
        {
            var label = new Label(text);
            label.AddToClassList("section-header");
            return label;
        }

        protected static VisualElement CreateSeparator()
        {
            var sep = new VisualElement();
            sep.AddToClassList("separator");
            return sep;
        }

        protected static Label CreateHelpBox(string message, bool isWarning = false)
        {
            var box = new Label(message);
            box.AddToClassList("reflective-helpbox");
            box.AddToClassList(isWarning ? "reflective-helpbox--warning" : "reflective-helpbox--info");
            return box;
        }

        protected static Button CreateButton(string text, System.Action onClick)
        {
            var btn = new Button(onClick) { text = text };
            btn.AddToClassList("reflective-button");
            return btn;
        }

        protected static VisualElement CreateSectionBody()
        {
            var body = new VisualElement();
            body.AddToClassList("section-body");
            return body;
        }

        protected void AddDefaultProperties(VisualElement parent)
        {
            VisualElement currentBody = null;
            var hasGroups = false;

            var targetType = target.GetType();

            var iterator = serializedObject.GetIterator();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath == "m_Script")
                    continue;

                var headerText = GetHeaderAttribute(targetType, iterator.name);

                if (headerText != null)
                {
                    hasGroups = true;

                    var group = new VisualElement();
                    group.AddToClassList("property-group");

                    var header = new Label(headerText);
                    header.AddToClassList("property-group-header");
                    group.Add(header);

                    currentBody = new VisualElement();
                    currentBody.AddToClassList("property-group-body");
                    group.Add(currentBody);

                    parent.Add(group);
                }

                if (currentBody == null && !hasGroups)
                {
                    currentBody = new VisualElement();
                    currentBody.AddToClassList("properties-container");
                    parent.Add(currentBody);
                }

                if (currentBody == null) continue;

                var field = new PropertyField(iterator.Copy());
                field.Bind(serializedObject);
                currentBody.Add(field);
            }
        }

        private static string GetHeaderAttribute(System.Type type, string fieldName)
        {
            while (type != null && type != typeof(Object) && type != typeof(object))
            {
                var field = type.GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (field != null)
                {
                    var header = field.GetCustomAttribute<HeaderAttribute>();
                    return header?.header;
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
