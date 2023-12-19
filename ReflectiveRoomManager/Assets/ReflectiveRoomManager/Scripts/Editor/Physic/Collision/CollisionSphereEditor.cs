using UnityEditor;
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
        protected override void DrawInspector(CollisionSphere myTarget)
        {
            EditorCollisionDrawUtilities.DrawBaseInspector(myTarget);
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
            
            var center = transform.TransformPoint(myTarget.Center);
            
            EditorDrawUtilities.DrawToShadedSphere(center, rotation, radius);
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

            var center = transform.TransformPoint(myTarget.Center);

            var tempSize = Handles.RadiusHandle(rotation, center, radius, true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(myTarget, "Edited Collider");
                myTarget.Radius = tempSize;
            }
        }
    }
}