using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CustomEditor(typeof(CollisionCapsule3D))]
    public class CollisionCapsule3DEditor : CollisionBaseEditor<CollisionCapsule3D>
    {
        private GUIContent editButtonContent;

        protected override void DrawInspector(CollisionCapsule3D myTarget)
        {
            EditorCollisionUtilities.DrawEditColliderButton(ref myTarget.Editable);

            myTarget.Center = EditorGUILayout.Vector3Field("Center: ", myTarget.Center);
            myTarget.Radius = EditorGUILayout.FloatField("Radius: ", myTarget.Radius);
            myTarget.Height = EditorGUILayout.FloatField("Height: ", myTarget.Height);
        }

        protected override void DrawCollision(CollisionCapsule3D myTarget)
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
                -transform.right,
                transform.forward,
                -transform.forward
            };

            var pos = transform.position + myTarget.Center;

            var endPoint1 = pos + transform.up * (myTarget.Height / 2 - myTarget.Radius);
            var endPoint2 = pos + -transform.up * (myTarget.Height / 2 - myTarget.Radius);

            // Draws the vertical lines of the capsule
            foreach (var dir in dirs)
            {
                Handles.DrawLine(endPoint1 + dir * myTarget.Radius, endPoint2 + dir * myTarget.Radius);
            }

            // Draws the upper part of the capsule
            Handles.DrawWireDisc(endPoint1, transform.up, myTarget.Radius);
            Handles.DrawWireArc(endPoint1, transform.right, -transform.forward, 180, myTarget.Radius);
            Handles.DrawWireArc(endPoint1, transform.forward, transform.right, 180, myTarget.Radius);

            // Draws the bottom part of the capsule
            Handles.DrawWireDisc(endPoint2, transform.up, myTarget.Radius);
            Handles.DrawWireArc(endPoint2, transform.right, -transform.forward, -180, myTarget.Radius);
            Handles.DrawWireArc(endPoint2, transform.forward, transform.right, -180, myTarget.Radius);
        }

        protected override void DrawEditableHandles(CollisionCapsule3D myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;

            Handles.color = EditorCollisionUtilities.EditColor;

            DrawHandlesForDirection(myTarget, myTarget.transform.up);
            DrawHandlesForDirection(myTarget, -myTarget.transform.up);
            DrawHandlesForDirection(myTarget, myTarget.transform.right);
            DrawHandlesForDirection(myTarget, -myTarget.transform.right);
            DrawHandlesForDirection(myTarget, myTarget.transform.forward);
            DrawHandlesForDirection(myTarget, -myTarget.transform.forward);
        }

        private void DrawHandlesForDirection(CollisionCapsule3D myTarget, Vector3 direction)
        {
            var boxCenter = myTarget.transform.position + myTarget.Center;

            var viewTransform = SceneView.lastActiveSceneView.camera.transform;

            var sizeMultiplier = direction == myTarget.transform.up || direction == -myTarget.transform.up ? myTarget.Height * .5f : myTarget.Radius;

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
                myTarget.Height = newSizeValue;
            }
            else
            {
                myTarget.Radius = newSizeValue * 0.5f;
            }
                
            if (myTarget.Height < myTarget.Radius * 2) myTarget.Height = myTarget.Radius * 2;
        }
    }
}