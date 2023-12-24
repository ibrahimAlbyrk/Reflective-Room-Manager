using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D2;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CollisionBox2D))]
    public class CollisionBox2DEditor : CollisionBaseEditor<CollisionBox2D>
    {
        protected override void DrawInspector(CollisionBox2D myTarget)
        {
            EditorCollisionDrawUtilities.DrawBaseInspector(myTarget);
        }
        
        protected override void DrawCollision(CollisionBox2D myTarget)
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
            
            var size = Vector3.Scale(transform.localScale, myTarget.Size);
            
            var topLeft = center + new Vector3(-size.x, size.y, 0) / 2;
            var topRight = center + new Vector3(size.x, size.y, 0) / 2;
            var bottomLeft = center + new Vector3(-size.x, -size.y, 0) / 2;
            var bottomRight = center + new Vector3(size.x, -size.y, 0) / 2;
                        
            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            
            Handles.matrix = matrix;
            
            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(bottomLeft, topLeft);
            
            Handles.matrix = Matrix4x4.identity;
        }

        protected override void DrawEditableHandles(CollisionBox2D myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;
            
            Handles.color = myTarget.enabled ? EditorCollisionDrawUtilities.EnableColor : EditorCollisionDrawUtilities.DisableColor;

            EditorGUI.BeginChangeCheck();

            var boxDirections = new []
            {
                Vector3.up,
                -Vector3.up,
                Vector3.right,
                -Vector3.right,
            };

            var transform = myTarget.transform;
            
            var boxCenter = myTarget.Center;
            var tempSize = Vector2.Scale(transform.localScale, myTarget.Size) * .5f;
            
            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            
            Handles.matrix = matrix;

            for (var i = 0; i < 4; i++)
            {
                var handleDirection = boxDirections[i];
                var handlePosition = boxCenter + Vector3.Scale(handleDirection, tempSize);
                var newHandlePosition = Handles.FreeMoveHandle(handlePosition, 0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);

                var newSizeValue = Vector2.Dot(newHandlePosition - boxCenter, handleDirection);

                switch (i)
                {
                    case 0:
                    case 1:
                        tempSize.y = newSizeValue;
                        break;
                    case 2:
                    case 3:
                        tempSize.x = newSizeValue;
                        break;
                }
            }

            Handles.matrix = Matrix4x4.identity;
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myTarget, "Edited Collider");
                myTarget.Size = tempSize / transform.localScale * 2;
            }
        }
    }
}