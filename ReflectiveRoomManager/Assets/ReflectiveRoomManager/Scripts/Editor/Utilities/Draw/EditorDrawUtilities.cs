using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace REFLECTIVE.Editor.Utilities.Draw
{
    public static class EditorDrawUtilities
    {
        public static void DrawTwoShadedWireDisc(Vector3 position, Vector3 axis, Vector3 from, float degrees, float radius)
        {
            Handles.DrawWireArc(position, axis, from, degrees, radius);
            var color1 = Handles.color;
            var color2 = color1;
            color1.a *= .2f;
            Handles.color = color1;
            Handles.DrawWireArc(position, axis, from, degrees - 360f, radius);
            Handles.color = color2;
        }
        
        public static void DrawTwoShadedWireDisc(Vector3 position, Vector3 axis, float radius)
        {
            var color1 = Handles.color;
            var color2 = color1;
            color1.a *= .2f;
            Handles.color = color1;
            Handles.DrawWireDisc(position, axis, radius);
            Handles.color = color2;
        }
        
        public static void DrawToShadedSphere(Vector3 position, Quaternion rotation, float radius)
        {
            var vector3Array = new []
            {
                rotation * Vector3.right,
                rotation * Vector3.up,
                rotation * Vector3.forward,
                rotation * -Vector3.right,
                rotation * -Vector3.up,
                rotation * -Vector3.forward
            };
            
            Vector3 vector3;
            
            if (Camera.current.orthographic)
            {
                vector3 = Camera.current.transform.forward;
                
                Handles.DrawWireDisc(position, vector3, radius);
                for (var index = 0; index < 3; ++index)
                {
                    var normalized = Vector3.Cross(vector3Array[index], vector3).normalized;
                    DrawTwoShadedWireDisc(position, vector3Array[index], normalized, 180f, radius);
                }
            }
            else
            {
                var matrix4x4 = Matrix4x4.Inverse(Handles.matrix);
                vector3 = position - matrix4x4.MultiplyPoint(Camera.current.transform.position);
                var sqrMagnitude = vector3.sqrMagnitude;
                var num2 = radius * radius;
                var f1 = num2 * num2 / sqrMagnitude;
                var num3 = f1 / num2;
                if (num3 < 1.0)
                {
                    var num4 = Mathf.Sqrt(num2 - f1);
                    Handles.DrawWireDisc(position - num2 * vector3 / sqrMagnitude, vector3, num4);
                }
                
                for (var index = 0; index < 3; ++index)
                {
                    if (num3 < 1.0)
                    {
                        var a = Vector3.Angle(vector3, vector3Array[index]);
                        var num5 = Mathf.Tan((90f - Mathf.Min(a, 180f - a)) * (Mathf.PI / 180f));
                        var f2 = Mathf.Sqrt(f1 + num5 * num5 * f1) / radius;
                        if (f2 < 1.0)
                        {
                            var angle = Mathf.Asin(f2) * 57.29578f;
                            var normalized = Vector3.Cross(vector3Array[index], vector3).normalized;
                            var from = Quaternion.AngleAxis(angle, vector3Array[index]) * normalized;
                            DrawTwoShadedWireDisc(position, vector3Array[index], from,
                                (float)((90.0 - angle) * 2.0), radius);
                        }
                        else
                            DrawTwoShadedWireDisc(position, vector3Array[index], radius);
                    }
                    else
                        DrawTwoShadedWireDisc(position, vector3Array[index], radius);
                }
            }
        }
        
        private static IEnumerable<Vector3> GetDirsForAxis(Quaternion rotation, Vector3 dir1, Vector3 dir2)
        {
            return new[]
            {
                rotation * dir1,
                rotation * -dir1,
                rotation * dir2,
                rotation * -dir2
            };
        }
    }
}