using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D2;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CustomEditor(typeof(CollisionCapsule2D))]
    public class CollisionCapsule2DEditor : CollisionBaseEditor<CollisionCapsule2D>
    {
        private GUIContent editButtonContent;

        protected override void DrawGUI(CollisionCapsule2D myTarget)
        {
            EditorCollisionUtilities.DrawEditColliderButton(ref myTarget.Editable);

            myTarget.Center = EditorGUILayout.Vector2Field("Center: ", myTarget.Center);
            myTarget._size = EditorGUILayout.Vector2Field("Size: ", myTarget.Size);
        }

        protected override void DrawCollision(CollisionCapsule2D myTarget)
        {
            var transform = myTarget.transform;
            
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionUtilities.EditColor
                    : EditorCollisionUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionUtilities.DisableColor;

            var dirs = new[]
            {
                transform.right,
                -transform.right
            };

            var pos = transform.position + (Vector3)myTarget.Center;

            var endPoint1 = pos + transform.up * (myTarget._size.y / 2 - myTarget._size.x);
            var endPoint2 = pos + -transform.up * (myTarget._size.y / 2 - myTarget._size.x);
            
            // Draws the vertical lines of the capsule
            foreach (var dir in dirs)
            {
                Handles.DrawLine(endPoint1 + dir * myTarget._size.x, endPoint2 + dir * myTarget._size.x);
            }

            // Draws the upper part of the capsule
            Handles.DrawWireArc(endPoint1, transform.forward, transform.right, 180, myTarget._size.x);

            // Draws the bottom part of the capsule
            Handles.DrawWireArc(endPoint2, transform.forward, transform.right, -180, myTarget._size.x);
        }

        protected override void DrawEditableHandles(CollisionCapsule2D myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;

            Handles.color = EditorCollisionUtilities.EditColor;

            DrawHandlesForDirection(myTarget, myTarget.transform.up);
            DrawHandlesForDirection(myTarget, -myTarget.transform.up);
            DrawHandlesForDirection(myTarget, myTarget.transform.right);
            DrawHandlesForDirection(myTarget, -myTarget.transform.right);
        }

        private void DrawHandlesForDirection(CollisionCapsule2D myTarget, Vector3 direction)
        {
            var boxCenter = myTarget.transform.position + (Vector3)myTarget.Center;

            var viewTransform = SceneView.lastActiveSceneView.camera.transform;

            var sizeMultiplier = direction == myTarget.transform.up || direction == -myTarget.transform.up ? myTarget._size.y * .5f : myTarget._size.x;

            var tempSize = sizeMultiplier * Vector3.one;
            var handlePosition = boxCenter + Vector3.Scale(direction, tempSize);
            var pointToCamera = (handlePosition - viewTransform.position).normalized;

            Handles.color = (Vector3.Dot(pointToCamera, direction) < 0) ?
                EditorCollisionUtilities.EnableColor : EditorCollisionUtilities.DisableColor;

            EditorGUI.BeginChangeCheck();

            var newHandlePosition = Handles.FreeMoveHandle(handlePosition,
                0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);

            if (!EditorGUI.EndChangeCheck()) return;
            
            Undo.RecordObject(myTarget, "Edited Collider");
            
            var newSizeValue = (newHandlePosition - boxCenter).magnitude * 2.0f;

            if (direction == myTarget.transform.up || direction == -myTarget.transform.up)
            {
                myTarget._size.y = newSizeValue;
            }
            else
            {
                myTarget._size.x = newSizeValue * 0.5f;
            }
                
            if (myTarget._size.y < myTarget._size.x * 2) myTarget._size.y = myTarget._size.x * 2;
        }
    }
}