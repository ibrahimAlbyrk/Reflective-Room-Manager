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
            
            var topLeft = transform.TransformPoint(center + new Vector3(-size.x, size.y, 0) / 2);
            var topRight = transform.TransformPoint(center + new Vector3(size.x, size.y, 0) / 2);
            var bottomLeft = transform.TransformPoint(center + new Vector3(-size.x, -size.y, 0) / 2);
            var bottomRight = transform.TransformPoint(center + new Vector3(size.x, -size.y, 0) / 2);

            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(bottomLeft, topLeft);
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
            var tempSize = Vector3.Scale(myTarget.transform.localScale, myTarget.Size);

            for (var i = 0; i < 4; i++)
            {
                var handleDirection = boxDirections[i];
                var handlePosition = transform.TransformPoint(boxCenter + Vector3.Scale(handleDirection, tempSize) * 0.5f);
                var newHandlePosition = Handles.FreeMoveHandle(handlePosition, 0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);

                var newSizeValue = (newHandlePosition - transform.TransformPoint(boxCenter)).magnitude * 2.0f;

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

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myTarget, "Edited Collider");
                myTarget.Size = tempSize / (Vector2)myTarget.transform.localScale;
            }
        }
    }
}