using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D2;
using REFLECTIVE.Runtime.Physic.Collision.Utilities;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CustomEditor(typeof(CollisionCircle))]
    public class CollisionCircleEditor : CollisionBaseEditor<CollisionCircle>
    {
        protected override void DrawInspector(CollisionCircle myTarget)
        {
            EditorCollisionDrawUtilities.DrawBaseInspector(myTarget);
            
            myTarget.Center = EditorGUILayout.Vector2Field("Center: ", myTarget.Center);
            myTarget.Radius = EditorGUILayout.FloatField("Radius: ", myTarget.Radius);
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

            var center = transform.position + myTarget.Center;
            
            var radius = CollisionTransformUtilities.GetRadius2D(transform, myTarget.Radius);
            
            Handles.DrawWireDisc(center, -transform.forward, radius);
        }

        protected override void DrawEditableHandles(CollisionCircle myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;
            
            Handles.color = myTarget.enabled ? EditorCollisionDrawUtilities.EnableColor : EditorCollisionDrawUtilities.DisableColor;
        
            EditorGUI.BeginChangeCheck();
        
            var boxDirections = new []
            {
                myTarget.transform.up,
                -myTarget.transform.up,
                myTarget.transform.right,
                -myTarget.transform.right,
            };
        
            var boxCenter = myTarget.transform.position + myTarget.Center;
            var tempSize = myTarget.Radius;
        
            foreach (var handleDirection in boxDirections)
            {
                var handlePosition = boxCenter + handleDirection * tempSize;
                var newHandlePosition = Handles.FreeMoveHandle(handlePosition, 0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);
                
                var directionChange = newHandlePosition - boxCenter;
                var changeOnHandleDirection = Vector3.Project(directionChange, handleDirection);
        
                var newSizeValue = changeOnHandleDirection.magnitude;
        
                tempSize = newSizeValue;
            }
        
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myTarget, "Edited Collider");
                myTarget.Radius = tempSize;
            }
        }
    }
}