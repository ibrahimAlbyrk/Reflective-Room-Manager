using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CollisionBox3D))]
    public class CollisionBox3DEditor : CollisionBaseEditor<CollisionBox3D>
    {
        protected override void DrawInspector(CollisionBox3D myTarget)
        {
            EditorCollisionDrawUtilities.DrawBaseInspector(myTarget);
        }
        
        protected override void DrawCollision(CollisionBox3D myTarget)
        {
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionDrawUtilities.EditColor
                    : EditorCollisionDrawUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionDrawUtilities.DisableColor;
            
            var center = myTarget.transform.TransformPoint(myTarget.Center);

            var size = Vector3.Scale(myTarget.transform.localScale, myTarget.Size);

            Handles.matrix = Matrix4x4.TRS(center, myTarget.transform.rotation, size);
            
            Handles.DrawWireCube(Vector3.zero, Vector3.one);
            
            Handles.matrix = Matrix4x4.identity;
        }

        protected override void DrawEditableHandles(CollisionBox3D myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;
            
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionDrawUtilities.EditColor
                    : EditorCollisionDrawUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionDrawUtilities.DisableColor;
            
            EditorGUI.BeginChangeCheck();
            
            var boxDirections = new []
            {
                Vector3.up,
                -Vector3.up,
                Vector3.right,
                -Vector3.right,
                Vector3.forward,
                -Vector3.forward
            };

            var transform = myTarget.transform;
            
            var boxCenter = myTarget.Center;
            var tempSize = Vector3.Scale(transform.localScale * .5f, myTarget.Size);

            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Handles.matrix = matrix;
            
            for (var i = 0; i < boxDirections.Length; i++)
            {
                var handleDirection = boxDirections[i];
                
                var handlePosition = boxCenter + Vector3.Scale(handleDirection, tempSize);
                
#pragma warning disable CS0618 // Type or member is obsolete
                var newHandlePosition = Handles.FreeMoveHandle(handlePosition,
                    Quaternion.identity, 0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);
#pragma warning restore CS0618 // Type or member is obsolete

                var newSizeValue = Vector3.Dot(newHandlePosition - boxCenter, handleDirection);

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
            
            Handles.matrix = Matrix4x4.identity;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myTarget, "Edited Collider");
                myTarget.Size = new Vector3(
                    tempSize.x / (myTarget.transform.localScale.x * .5f),
                    tempSize.y / (myTarget.transform.localScale.y * .5f),
                    tempSize.z / (myTarget.transform.localScale.z * .5f));
            }
        }
    }
}