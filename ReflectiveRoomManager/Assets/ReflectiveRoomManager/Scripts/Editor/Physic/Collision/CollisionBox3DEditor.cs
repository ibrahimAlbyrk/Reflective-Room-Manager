using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CustomEditor(typeof(CollisionBox3D))]
    public class CollisionBox3DEditor : CollisionBaseEditor<CollisionBox3D>
    {
        protected override void DrawGUI(CollisionBox3D myTarget)
        {
            EditorCollisionUtilities.DrawEditColliderButton(ref myTarget.Editable);
            
            myTarget.Center = EditorGUILayout.Vector3Field("Center: ", myTarget.Center);
            myTarget.Size = EditorGUILayout.Vector3Field("Size: ", myTarget.Size);
        }
        
        protected override void DrawCollision(CollisionBox3D myTarget)
        {
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionUtilities.EditColor
                    : EditorCollisionUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionUtilities.DisableColor;
            
            var center = myTarget.transform.position + myTarget.Center;
            
            Handles.DrawWireCube(center, myTarget.Size);
        }

        protected override void DrawEditableHandles(CollisionBox3D myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;
            
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionUtilities.EditColor
                    : EditorCollisionUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionUtilities.DisableColor;
            
            EditorGUI.BeginChangeCheck();

            var boxDirections = new []
            {
                myTarget.transform.up,
                -myTarget.transform.up,
                myTarget.transform.right,
                -myTarget.transform.right,
                myTarget.transform.forward,
                -myTarget.transform.forward
            };

            var boxCenter = myTarget.transform.position + myTarget.Center;
            var tempSize = myTarget.Size;

            for (var i = 0; i < 6; i++)
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
                    case 4:
                    case 5:
                        tempSize.z = newSizeValue;
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