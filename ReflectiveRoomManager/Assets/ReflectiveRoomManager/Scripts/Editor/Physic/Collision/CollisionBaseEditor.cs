using UnityEditor;
using UnityEngine;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Editor.Utilities;
    
    using Runtime.Physic.Collision;
    
    [CanEditMultipleObjects]
    public abstract class CollisionBaseEditor<TargetType> : UnityEditor.Editor where TargetType : Object
    {
        public override void OnInspectorGUI()
        {
            var _target = (TargetType)target;
            
            DrawInspector(_target);
            
            CustomEditorUtilities.DrawDefaultInspector(serializedObject);
            
            if (GUI.changed)
            {
                Undo.RecordObject(_target, "Changed Collider");
            
                EditorUtility.SetDirty(target);   
            }
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