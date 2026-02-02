using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using REFLECTIVE.Runtime.Physic.Collision;

namespace REFLECTIVE.Editor.Physic.Collision.Utilities
{
    public static class EditorCollisionDrawUtilities
    {
        public static readonly Color EditColor = new(0.48f, 1f, 0.22f);
        public static readonly Color EnableColor = new(0.48f, 1f, 0.22f, .5f);
        public static readonly Color DisableColor = new(0.48f, 1f, 0.22f, .2f);

        public static VisualElement CreateEditColliderRow<TCollision, PScene>(
            CollisionBase<TCollision, PScene> collision) where TCollision : Component
        {
            var row = new VisualElement();
            row.AddToClassList("edit-collider-row");

            row.Add(new Label("Edit Collider:"));

            var button = new Button();
            button.AddToClassList("edit-collider-btn");

            var editIcon = EditorGUIUtility.IconContent("EditCollider").image;
            if (editIcon != null)
            {
                var icon = new Image { image = editIcon };
                icon.style.width = 16;
                icon.style.height = 16;
                button.Add(icon);
            }

            if (collision.Editable)
                button.AddToClassList("edit-collider-btn--active");

            button.clicked += () =>
            {
                collision.Editable = !collision.Editable;

                if (collision.Editable)
                    button.AddToClassList("edit-collider-btn--active");
                else
                    button.RemoveFromClassList("edit-collider-btn--active");
            };

            row.Add(button);

            return row;
        }
    }
}
