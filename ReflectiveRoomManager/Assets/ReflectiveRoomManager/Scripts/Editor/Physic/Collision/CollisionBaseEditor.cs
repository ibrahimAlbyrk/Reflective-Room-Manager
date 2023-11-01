using UnityEditor;
using REFLECTIVE.Runtime.Physic.Collision;

namespace REFLECTIVE.Editor.Physic.Collision
{
    [CanEditMultipleObjects]
    public abstract class CollisionBaseEditor<TargetType> : UnityEditor.Editor where TargetType : UnityEngine.Object
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            var _target = (TargetType)target;
            
            DrawGUI(_target);
            
            EditorUtility.SetDirty(target);
            
            if (!EditorGUI.EndChangeCheck()) return;
            
            Undo.RecordObject(_target, "Change Collider Value");
        }

        protected virtual void OnSceneGUI()
        {
            var myTarget = (TargetType)target;

            DrawCollision(myTarget);
            DrawEditableHandles(myTarget);
        }

        protected virtual void DrawGUI(TargetType myTarget)
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