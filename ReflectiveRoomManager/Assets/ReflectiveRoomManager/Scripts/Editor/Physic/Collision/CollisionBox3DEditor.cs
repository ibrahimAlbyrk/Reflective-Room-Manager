using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace REFLECTIVE.Editor.Physic.Collision
{
    [CustomEditor(typeof(CollisionBox3D))]
    public class CollisionBox3DEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var myTarget = (CollisionBox3D)target;

            if (!myTarget.Editable) return;
            
            Handles.color = Color.green;

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

            var boxCenter = myTarget.transform.position + myTarget.Offset;
            var tempSize = myTarget.Size;

            for (var i = 0; i < 6; i++)
            {
                var handleDirection = boxDirections[i];
                var handlePosition = boxCenter + Vector3.Scale(handleDirection, tempSize) * 0.5f;
                var newHandlePosition = Handles.FreeMoveHandle(handlePosition, 0.05f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);

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
                Undo.RecordObject(myTarget, "Changed Offset Or Size");
                myTarget.Size = tempSize;
            }
        }
    }
}