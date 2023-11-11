using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using REFLECTIVE.Runtime.Physic.Collision.D2;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CustomEditor(typeof(CollisionCapsule2D))]
    public class CollisionCapsule2DEditor : CollisionBaseEditor<CollisionCapsule2D>
    {
        private GUIContent editButtonContent;
        
        private Action<CollisionCapsule2D> _drawFunc;
        private Action<CollisionCapsule2D> _editFunc;
        
        private readonly string[] dirTypeOptions =
        {
            "X-Axis",
            "Y-Axis",
        };

        protected override void DrawInspector(CollisionCapsule2D myTarget)
        {
            EditorCollisionDrawUtilities.DrawBaseInspector(myTarget);
            
            myTarget.Center = EditorGUILayout.Vector2Field("Center: ", myTarget.Center);

            myTarget.Radius = EditorGUILayout.FloatField("Radius: ", myTarget.Radius);
            myTarget.Height = EditorGUILayout.FloatField("Height: ", myTarget.Height);
            
            myTarget.DirType = EditorGUILayout.Popup("Direction: ", myTarget.DirType, dirTypeOptions);
            
            myTarget.Dir = myTarget.DirType switch
            {
                0 => CapsuleDirection2D.Horizontal,
                1 => CapsuleDirection2D.Vertical,
                _ => myTarget.Dir
            };
            
            _drawFunc = myTarget.DirType switch
            {
                0 => DrawXAxisCollision,
                1 => DrawYAxisCollision,
                _ => null
            };
            
            _editFunc = myTarget.DirType switch
            {
                0 => DrawXAxisEditableHandles,
                1 => DrawYAxisEditableHandles,
                _ => null
            };
        }

        protected override void DrawCollision(CollisionCapsule2D myTarget)
        {
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionDrawUtilities.EditColor
                    : EditorCollisionDrawUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionDrawUtilities.DisableColor;
            
            _drawFunc?.Invoke(myTarget);
        }

        protected override void DrawEditableHandles(CollisionCapsule2D myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;

            Handles.color = EditorCollisionDrawUtilities.EditColor;
            
            _editFunc?.Invoke(myTarget);
        }

        private void DrawXAxisEditableHandles(CollisionCapsule2D myTarget)
        {
            var size = GetSizeForXAxis(myTarget);

            DrawHandlesForDirectionX(myTarget, myTarget.transform.up, size);
            DrawHandlesForDirectionX(myTarget, -myTarget.transform.up, size);
            DrawHandlesForDirectionX(myTarget, myTarget.transform.right, size);
            DrawHandlesForDirectionX(myTarget, -myTarget.transform.right, size);
        }
        
        private void DrawYAxisEditableHandles(CollisionCapsule2D myTarget)
        {
            var size = GetSizeForYAxis(myTarget);
            
            DrawHandlesForDirectionY(myTarget, myTarget.transform.up, size);
            DrawHandlesForDirectionY(myTarget, -myTarget.transform.up, size);
            DrawHandlesForDirectionY(myTarget, myTarget.transform.right, size);
            DrawHandlesForDirectionY(myTarget, -myTarget.transform.right, size);
        }
        
        private void DrawHandlesForDirectionY(CollisionCapsule2D myTarget, Vector3 drawDirection, Vector2 size)
        {
            var boxCenter = myTarget.transform.position + myTarget.Center;

            var sizeMultiplier = drawDirection == myTarget.transform.up || drawDirection == -myTarget.transform.up ? size.y * .5f : size.x;

            var tempSize = sizeMultiplier * Vector3.one;
            var handlePosition = boxCenter + Vector3.Scale(drawDirection, tempSize);

            EditorGUI.BeginChangeCheck();

            var newHandlePosition = Handles.FreeMoveHandle(handlePosition,
                0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);
            
            if (!EditorGUI.EndChangeCheck()) return;
            
            var newSizeValue = ((newHandlePosition) - boxCenter).magnitude * 2.0f;
            
            if (drawDirection == myTarget.transform.up || drawDirection == -myTarget.transform.up)
            {
                myTarget.Height = newSizeValue;
                myTarget.Height /= myTarget.transform.localScale.y;
            }
            else
            {
                myTarget.Radius = newSizeValue * 0.5f;
                myTarget.Radius /= myTarget.transform.localScale.x;
            }

            if (myTarget.Height < myTarget.Radius * 2) myTarget.Height = myTarget.Radius * 2;
            
            
            Undo.RecordObject(myTarget, "Edited Collider");
        }
        
        //TODO Scale issues
        private void DrawHandlesForDirectionX(CollisionCapsule2D myTarget, Vector3 drawDirection, Vector2 size)
        {
            var boxCenter = myTarget.transform.position + myTarget.Center;

            var sizeMultiplier = (drawDirection == myTarget.transform.right || drawDirection == -myTarget.transform.right) 
                ? size.y * .5f 
                : size.x;

            var tempSize = sizeMultiplier * Vector3.one;
            var handlePosition = boxCenter +  Vector3.Scale(drawDirection, tempSize);

            EditorGUI.BeginChangeCheck();

            var newHandlePosition = Handles.FreeMoveHandle(handlePosition,
                0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);

            if (!EditorGUI.EndChangeCheck()) return;
            
            var newSizeValue = (newHandlePosition - boxCenter).magnitude * 2.0f;

            var scale = myTarget.transform.localScale;
            
            if (drawDirection == myTarget.transform.right || drawDirection == -myTarget.transform.right)
            {
                myTarget.Height = newSizeValue;
                myTarget.Height /= scale.x;
            }
            else
            {
                myTarget.Radius = newSizeValue * 0.5f;
                myTarget.Radius /= scale.y;
            }

            if (myTarget.Height < myTarget.Radius * 2) myTarget.Height = myTarget.Radius * 2;

            Undo.RecordObject(myTarget, "Edited Collider");
        }
        
        private void DrawYAxisCollision(CollisionCapsule2D myTarget)
        {
            var transform = myTarget.transform;

            var dirs = GetDirsForAxis(transform.right);

            var size = GetSizeForYAxis(myTarget);
            
            var (endPoint1, endPoint2) = GetPointsForAxis(myTarget, transform.up, size);
            
            foreach (var dir in dirs)
            {
                Handles.DrawLine(endPoint1 + dir * size.x, endPoint2 + dir * size.x);
            }

            Handles.DrawWireArc(endPoint1, transform.forward, transform.right, 180, size.x);

            Handles.DrawWireArc(endPoint2, transform.forward, transform.right, -180, size.x);
        }

        private void DrawXAxisCollision(CollisionCapsule2D myTarget)
        {
            var transform = myTarget.transform;

            var dirs = GetDirsForAxis(transform.up);

            var size = GetSizeForXAxis(myTarget);
            
            var (endPoint1, endPoint2) = GetPointsForAxis(myTarget, transform.right, size);
            
            foreach (var dir in dirs)
            {
                Handles.DrawLine(endPoint1 + dir * size.x, endPoint2 + dir * size.x);
            }

            Handles.DrawWireArc(endPoint1, transform.forward, transform.up, -180, size.x);
            Handles.DrawWireArc(endPoint2, transform.forward, transform.up, 180, size.x);
        }
        
        private static (Vector3, Vector3) GetPointsForAxis(CollisionCapsule2D myTarget, Vector3 dir, Vector2 size)
        {
            var pos = myTarget.transform.position + myTarget.Center;

            var point0 = pos + dir * (size.y / 2 - size.x);
            var point1 = pos + -dir * (size.y / 2 - size.x);

            return (point0, point1);
        }

        private static IEnumerable<Vector3> GetDirsForAxis(Vector3 dir1)
        {
            return new[]
            {
                dir1,
                -dir1,
            };
        }

        private Vector2 GetSizeForYAxis(CollisionCapsule2D myTarget)
        {
            var size = new Vector2(myTarget.Radius * myTarget.transform.localScale.x, myTarget.Height * myTarget.transform.localScale.y);
            
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Max(size.y, 0);
            
            if (size.y < size.x * 2) size.y = size.x * 2;
            if (size.x  > size.y / 2) size.x  = size.y / 2;

            return size;
        }

        private Vector2 GetSizeForXAxis(CollisionCapsule2D myTarget)
        {
            var size = new Vector2(myTarget.Radius * myTarget.transform.localScale.y, myTarget.Height * myTarget.transform.localScale.x);
            
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Max(size.y, 0);

            if (size.y < size.x * 2) size.y = size.x * 2;
            if (size.x  > size.y / 2) size.x  = size.y / 2;

            return size;
        }
    }
}