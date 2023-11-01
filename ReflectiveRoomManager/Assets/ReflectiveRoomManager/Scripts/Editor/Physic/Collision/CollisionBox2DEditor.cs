using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D2;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CustomEditor(typeof(CollisionBox2D))]
    public class CollisionBox2DEditor : CollisionBaseEditor<CollisionBox2D>
    {
        protected override void DrawGUI(CollisionBox2D myTarget)
        {
            EditorCollisionUtilities.DrawEditColliderButton(ref myTarget.Editable);
            
            myTarget.Center = EditorGUILayout.Vector2Field("Center: ", myTarget.Center);
            myTarget.Size = EditorGUILayout.Vector2Field("Size: ", myTarget.Size);
        }

        protected override void DrawCollision(CollisionBox2D myTarget)
        {
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionUtilities.EditColor
                    : EditorCollisionUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionUtilities.DisableColor;

            var center = myTarget.transform.position + (Vector3)myTarget.Center;
            
            var topLeft = center + new Vector3(-myTarget.Size.x, myTarget.Size.y, 0) / 2;
            var topRight = center + new Vector3(myTarget.Size.x, myTarget.Size.y, 0) / 2;
            var bottomLeft = center + new Vector3(-myTarget.Size.x, -myTarget.Size.y, 0) / 2;
            var bottomRight = center + new Vector3(myTarget.Size.x, -myTarget.Size.y, 0) / 2;

            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(bottomLeft, topLeft);
        }

        protected override void DrawEditableHandles(CollisionBox2D myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;
            
            Handles.color = myTarget.enabled ? EditorCollisionUtilities.EnableColor : EditorCollisionUtilities.DisableColor;

            EditorGUI.BeginChangeCheck();

            var boxDirections = new []
            {
                myTarget.transform.up,
                -myTarget.transform.up,
                myTarget.transform.right,
                -myTarget.transform.right,
            };

            var boxCenter = myTarget.transform.position + (Vector3)myTarget.Center;
            var tempSize = myTarget.Size;

            for (var i = 0; i < 4; i++)
            {
                var handleDirection = boxDirections[i];
                var handlePosition = boxCenter + Vector3.Scale(handleDirection, tempSize) * 0.5f;
                var newHandlePosition = Handles.FreeMoveHandle(handlePosition, 0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);

                var newSizeValue = (newHandlePosition - boxCenter).magnitude * 2.0f;

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
                myTarget.Size = tempSize;
            }
        }
    }
}