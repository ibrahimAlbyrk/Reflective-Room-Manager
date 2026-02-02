using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using REFLECTIVE.Editor.Utilities;
using REFLECTIVE.Runtime.Physic.Collision;
using UnityEditor.UIElements;

namespace REFLECTIVE.Editor.Physic.Collision
{
    [CanEditMultipleObjects]
    public abstract class CollisionBaseEditor<TargetType> : ReflectiveEditorBase where TargetType : Object
    {
        protected override string GetTitle() => null;

        protected override void BuildInspectorUI(VisualElement root)
        {
            var typedTarget = (TargetType)target;

            BuildCollisionInspector(root, typedTarget);

            AddDefaultProperties(root);

            root.TrackSerializedObjectValue(serializedObject, _ =>
            {
                EditorUtility.SetDirty(target);
            });
        }

        protected virtual void OnSceneGUI()
        {
            var myTarget = (TargetType)target;

            DrawCollision(myTarget);
            DrawEditableHandles(myTarget);
        }

        protected virtual void BuildCollisionInspector(VisualElement root, TargetType myTarget)
        {
        }

        protected virtual void DrawCollision(TargetType myTarget)
        {
        }

        protected virtual void DrawEditableHandles(TargetType myTarget)
        {
        }

        private void OnEnable()
        {
            var typedTarget = (TargetType)target;

            if (typedTarget is not IEditableForEditor editable) return;

            editable.SetEditable(false);
        }
    }
}
