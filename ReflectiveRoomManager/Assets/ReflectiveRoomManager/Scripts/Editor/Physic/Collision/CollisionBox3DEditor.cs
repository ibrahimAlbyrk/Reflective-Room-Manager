using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CustomEditor(typeof(CollisionBox3D))]
    public class CollisionBox3DEditor : CollisionBaseEditor<CollisionBox3D>
    {
        protected override void DrawInspector(CollisionBox3D myTarget)
        {
            EditorCollisionDrawUtilities.DrawBaseInspector(myTarget);
            
            myTarget.Center = EditorGUILayout.Vector3Field("Center: ", myTarget.Center);
            myTarget.Size = EditorGUILayout.Vector3Field("Size: ", myTarget.Size);
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
            
            var center = myTarget.transform.position + myTarget.Center;

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
                myTarget.transform.up,
                -myTarget.transform.up,
                myTarget.transform.right,
                -myTarget.transform.right,
                myTarget.transform.forward,
                -myTarget.transform.forward
            };

            var boxCenter = myTarget.transform.position + myTarget.Center;
            var tempSize = Vector3.Scale(myTarget.transform.localScale, myTarget.Size);

            for (var i = 0; i < boxDirections.Length; i++)
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
                myTarget.Size = new Vector3(
                    tempSize.x / myTarget.transform.localScale.x,
                    tempSize.y / myTarget.transform.localScale.y,
                    tempSize.z / myTarget.transform.localScale.z);
            }
        }
    }
}