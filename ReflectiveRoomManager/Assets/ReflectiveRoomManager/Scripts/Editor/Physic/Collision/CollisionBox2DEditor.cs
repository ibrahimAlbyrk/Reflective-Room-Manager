using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D2;

namespace REFLECTIVE.Editor.Physic.Collision
{
    [CustomEditor(typeof(CollisionBox2D))]
    public class CollisionBox2DEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var myTarget = (CollisionBox2D)target;
            
            if (!myTarget.Editable) return;
            
            Handles.color = Color.green;

            EditorGUI.BeginChangeCheck();

            var boxDirections = new []
            {
                myTarget.transform.up,
                -myTarget.transform.up,
                myTarget.transform.right,
                -myTarget.transform.right,
            };

            var boxCenter = myTarget.transform.position + (Vector3)myTarget.Offset;
            var tempSize = myTarget.Size;

            for (var i = 0; i < 4; i++)
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