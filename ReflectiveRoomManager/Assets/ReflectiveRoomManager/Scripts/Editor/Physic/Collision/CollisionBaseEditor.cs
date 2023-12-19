using UnityEditor;
using REFLECTIVE.Runtime.Physic.Collision;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Editor.Utilities;
    
    [CanEditMultipleObjects]
    public abstract class CollisionBaseEditor<TargetType> : UnityEditor.Editor where TargetType : UnityEngine.Object
    {
        public override void OnInspectorGUI()
        {
            var _target = (TargetType)target;
            
            DrawInspector(_target);
            
            CustomEditorUtilities.DrawDefaultInspector(serializedObject);
            
            Undo.RecordObject(_target, "Change Collider Value");
            
            EditorUtility.SetDirty(target);
        }

        protected virtual void OnSceneGUI()
        {
            var myTarget = (TargetType)target;

            DrawCollision(myTarget);
            DrawEditableHandles(myTarget);
        }

        protected virtual void DrawInspector(TargetType myTarget)
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
            var _target = (TargetType)target;

            if (_target is not IEditableForEditor editable) return;
            
            editable.SetEditable(false);
        }
    }
}