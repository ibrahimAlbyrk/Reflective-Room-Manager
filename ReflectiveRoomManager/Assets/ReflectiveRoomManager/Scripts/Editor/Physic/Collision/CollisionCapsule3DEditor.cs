﻿using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using REFLECTIVE.Runtime.Physic.Collision.D3;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CollisionCapsule3D))]
    public class CollisionCapsule3DEditor : CollisionBaseEditor<CollisionCapsule3D>
    {
        private GUIContent editButtonContent;

        private Action<CollisionCapsule3D> _drawFunc;
        private Action<CollisionCapsule3D> _editFunc;

        private readonly string[] dirTypeOptions =
        {
            "X-Axis",
            "Y-Axis",
            "Z-Axis"
        };
        
        protected override void DrawInspector(CollisionCapsule3D myTarget)
        {
            EditorCollisionDrawUtilities.DrawBaseInspector(myTarget);
            
            myTarget.DirType = EditorGUILayout.Popup("Direction: ", myTarget.DirType, dirTypeOptions);

            myTarget.Dirs = myTarget.DirType switch
            {
                0 => new[] { Vector3.left, Vector3.right },
                1 => new[] { Vector3.down, Vector3.up },
                2 => new[] { Vector3.back, Vector3.forward },
                _ => myTarget.Dirs
            };

            _drawFunc = myTarget.DirType switch
            {
                0 => DrawXAxisCollision,
                1 => DrawYAxisCollision,
                2 => DrawZAxisCollision,
                _ => null
            };

            _editFunc = myTarget.DirType switch
            {
                
                0 => DrawXAxisEdit,
                1 => DrawYAxisEdit,
                2 => DrawZAxisEdit,
                _ => null
            };
        }

        protected override void DrawCollision(CollisionCapsule3D myTarget)
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

        protected override void DrawEditableHandles(CollisionCapsule3D myTarget)
        {
            if (!myTarget.Editable || !myTarget.enabled) return;

            Handles.color = EditorCollisionDrawUtilities.EditColor;
            
            _editFunc?.Invoke(myTarget);
        }

        private static void DrawAxisEdit(CollisionCapsule3D myTarget, Vector3 up)
        {
            var matrix = Matrix4x4.TRS(myTarget.transform.position, myTarget.transform.rotation, Vector3.one);

            Handles.matrix = matrix;
            
            DrawHandlesForDirection(myTarget, Vector3.up, up);
            DrawHandlesForDirection(myTarget, -Vector3.up, up);
            DrawHandlesForDirection(myTarget, Vector3.right, up);
            DrawHandlesForDirection(myTarget, -Vector3.right, up);
            DrawHandlesForDirection(myTarget, Vector3.forward, up);
            DrawHandlesForDirection(myTarget, -Vector3.forward, up);

            Handles.matrix = Matrix4x4.identity;
        }
        
        private static void DrawXAxisEdit(CollisionCapsule3D myTarget)
        {
            DrawAxisEdit(myTarget, Vector3.right);
        }
        
        private static void DrawYAxisEdit(CollisionCapsule3D myTarget)
        {
            DrawAxisEdit(myTarget, Vector3.up);
        }
        
        private static void DrawZAxisEdit(CollisionCapsule3D myTarget)
        {
            DrawAxisEdit(myTarget, Vector3.forward);
        }
        private static void DrawHandlesForDirection(CollisionCapsule3D myTarget, Vector3 direction, Vector3 up)
        {
            var transform = myTarget.transform;
            
            var boxCenter = myTarget.Center;
            
            var size = GetSizeForAxis(transform, myTarget.Radius, myTarget.Height);

            var viewTransform = SceneView.lastActiveSceneView.camera.transform;
            
            var sizeMultiplier = direction == up || direction == -up ? size.y * .5f : size.x;
            
            var tempSize = sizeMultiplier * Vector3.one;
            var handlePosition = boxCenter + Vector3.Scale(direction, tempSize);
            var pointToCamera = (handlePosition - viewTransform.position).normalized;

            Handles.color = (Vector3.Dot(pointToCamera, direction) < 0) ?
                EditorCollisionDrawUtilities.EnableColor : EditorCollisionDrawUtilities.DisableColor;

            EditorGUI.BeginChangeCheck();

#pragma warning disable CS0618 // Type or member is obsolete
            var newHandlePosition = Handles.FreeMoveHandle(handlePosition,
                Quaternion.identity, 0.03f * HandleUtility.GetHandleSize(handlePosition), Vector3.zero, Handles.DotHandleCap);
#pragma warning restore CS0618 // Type or member is obsolete

            if (!EditorGUI.EndChangeCheck()) return;
            
            Undo.RecordObject(myTarget, "Edited Collider");
            
            var newSizeValue = Vector3.Dot(newHandlePosition - boxCenter, direction) * 2.0f;

            var scale = transform.localScale;
            
            if (direction == up || direction == -up)
            {
                myTarget.Height = newSizeValue;
                myTarget.Height /= scale.y;
            }
            else
            {
                myTarget.Radius = newSizeValue * 0.5f;
                myTarget.Radius /= Mathf.Max(scale.x, scale.z);
            }
                
            if (myTarget.Height < myTarget.Radius * 2) myTarget.Height = myTarget.Radius * 2;
        }

        private static void DrawAxisCollision(CollisionCapsule3D myTarget, Vector3 direction, Vector3 up,
            Vector3 forward)
        {
            var transform = myTarget.transform;
            
            var dirs = GetDirsForAxis(up, forward);
            
            var size = GetSizeForAxis(myTarget.transform, myTarget.Radius, myTarget.Height);
            
            var (point0, point1) = GetPointsForAxis(myTarget, direction, size);
            
            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Handles.matrix = matrix;
            
            // Draws the vertical lines of the capsule
            foreach (var dir in dirs)
            {
                var pos1 = point0 + dir * size.x;
                var pos2 = point1 + dir * size.x;
                
                Handles.DrawLine(pos1, pos2);
            }
            
            // Draw front and back part of the capsule
            Handles.DrawWireDisc(point0, direction, size.x);
            Handles.DrawWireArc(point0, -up, forward, 180, size.x);
            Handles.DrawWireArc(point0, forward, up, 180, size.x);

            // Draw back and upper part of the capsule
            Handles.DrawWireDisc(point1, direction, size.x);
            Handles.DrawWireArc(point1, -up, -forward, 180, size.x);
            Handles.DrawWireArc(point1, -forward, -up, -180, size.x);
            
            Handles.matrix = Matrix4x4.identity;
        }
        
        private static void DrawXAxisCollision(CollisionCapsule3D myTarget)
        {
            DrawAxisCollision(myTarget, Vector3.right, Vector3.forward, Vector3.up);
        }
        
        private static void DrawYAxisCollision(CollisionCapsule3D myTarget)
        {
            DrawAxisCollision(myTarget, Vector3.up, Vector3.right, Vector3.forward);
        }
        
        private static void DrawZAxisCollision(CollisionCapsule3D myTarget)
        {
            DrawAxisCollision(myTarget, Vector3.forward, Vector3.up, Vector3.right);
        }

        private static IEnumerable<Vector3> GetDirsForAxis(Vector3 dir1, Vector3 dir2)
        {
            return new[]
            {
                dir1,
                -dir1,
                dir2,
                -dir2
            };
        }

        private static (Vector3, Vector3) GetPointsForAxis(CollisionCapsule3D myTarget, Vector3 dir,Vector2 size)
        {
            var pos = myTarget.Center;

            var point0 = pos + dir * (size.y / 2 - size.x);
            var point1 = pos + -dir * (size.y / 2 - size.x);

            return (point0, point1);
        }

        private static Vector2 GetSizeForAxis(Transform transform, float xSize, float ySize)
        {
            var calculatedHeight = ySize * transform.localScale.y;
            var calculatedRadius = xSize * Mathf.Max(transform.localScale.x, transform.localScale.z);
            
            var size = new Vector2(calculatedRadius, calculatedHeight);
            
            size.y = Mathf.Max(size.y, 0);
            size.x = Mathf.Abs(size.x);
            
            if (size.y < size.x * 2) size.y = size.x * 2;
            if (size.x  > size.y / 2) size.x  = size.y / 2;

            return size;
        }
    }
}