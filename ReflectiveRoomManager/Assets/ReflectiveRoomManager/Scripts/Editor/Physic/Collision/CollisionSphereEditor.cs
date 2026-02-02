using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using REFLECTIVE.Runtime.Physic.Collision.D3;
using REFLECTIVE.Runtime.Physic.Collision.Utilities;

namespace REFLECTIVE.Editor.Physic.Collision
{
    using Utilities;
    using Editor.Utilities.Draw;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(CollisionSphere))]
    public class CollisionSphereEditor : CollisionBaseEditor<CollisionSphere>
    {
        protected override void BuildCollisionInspector(VisualElement root, CollisionSphere myTarget)
        {
            root.Add(EditorCollisionDrawUtilities.CreateEditColliderRow(myTarget));
        }

        protected override void DrawCollision(CollisionSphere myTarget)
        {
            if (myTarget.enabled)
            {
                Handles.color = myTarget.Editable
                    ? EditorCollisionDrawUtilities.EditColor
                    : EditorCollisionDrawUtilities.EnableColor;
            }
            else Handles.color = EditorCollisionDrawUtilities.DisableColor;

            var transform = myTarget.transform;

            var rotation = transform.rotation;

            var radius = CollisionTransformUtilities.GetRadius(transform, myTarget.Radius);

            var center = myTarget.Center;

            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Handles.matrix = matrix;

            EditorDrawUtilities.DrawToShadedSphere(center, rotation, radius);

            Handles.matrix = Matrix4x4.identity;
        }

        protected override void DrawEditableHandles(CollisionSphere myTarget)
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

            var transform = myTarget.transform;

            var rotation = transform.rotation;

            var radius = CollisionTransformUtilities.GetRadius(transform, myTarget.Radius);

            var center = myTarget.Center;

            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Handles.matrix = matrix;

            var tempSize = Handles.RadiusHandle(rotation, center, radius, true);

            Handles.matrix = Matrix4x4.identity;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myTarget, "Edited Collider");

                var transformRadius = CollisionTransformUtilities.GetTransformRadius(transform);

                myTarget.Radius = tempSize / transformRadius;
            }
        }
    }
}
