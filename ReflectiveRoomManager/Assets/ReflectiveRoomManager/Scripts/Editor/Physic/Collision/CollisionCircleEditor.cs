using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D2;
using REFLECTIVE.Runtime.Physic.Collision.Utilities;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CollisionCircle))]
    public class CollisionCircleEditor : CollisionBaseEditor<CollisionCircle>
    {
        protected override void DrawInspector(CollisionCircle myTarget)
        {
            EditorCollisionDrawUtilities.DrawBaseInspector(myTarget);
        }

        protected override void DrawCollision(CollisionCircle myTarget)
        {
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionDrawUtilities.EditColor
                    : EditorCollisionDrawUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionDrawUtilities.DisableColor;

            var transform = myTarget.transform;

            var center = myTarget.Center;
            
            var radius = CollisionTransformUtilities.GetRadius2D(transform, myTarget.Radius);
            
            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Handles.matrix = matrix;
            
            Handles.DrawWireDisc(center, -transform.forward, radius);

            Handles.matrix = Matrix4x4.identity;
        }

        protected override void DrawEditableHandles(CollisionCircle myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;
            
            Handles.color = myTarget.enabled ? EditorCollisionDrawUtilities.EnableColor : EditorCollisionDrawUtilities.DisableColor;
        
            EditorGUI.BeginChangeCheck();

            var transform = myTarget.transform;
            
            var radius = CollisionTransformUtilities.GetRadius2D(transform, myTarget.Radius);
            
            var boxDirections = new []
            {
                Vector3.up,
                -Vector3.up,
                Vector3.right,
                -Vector3.right,
            };
        
            var boxCenter = myTarget.Center;
            var tempSize = radius;
            
            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Handles.matrix = matrix;
        
            foreach (var handleDirection in boxDirections)
            {
                var handlePosition = boxCenter + handleDirection * tempSize;
                
                var newHandlePosition = Handles.FreeMoveHandle(handlePosition,
                    0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);
                
                var directionChange = newHandlePosition - boxCenter;
                var changeOnHandleDirection = Vector3.Project(directionChange, handleDirection);
        
                var newSizeValue = changeOnHandleDirection.magnitude;
        
                tempSize = newSizeValue;
            }

            Handles.matrix = Matrix4x4.identity;
        
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myTarget, "Edited Collider");
                
                var transformRadius = CollisionTransformUtilities.GetTransformRadius2D(transform);
                
                myTarget.Radius = tempSize / transformRadius;
            }
        }
    }
}