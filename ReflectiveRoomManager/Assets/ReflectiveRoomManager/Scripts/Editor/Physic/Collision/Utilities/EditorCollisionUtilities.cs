using UnityEditor;
using UnityEngine;
using REFLECTIVE.Runtime.Physic.Collision;

namespace REFLECTIVE.Editor.Physic.Collision.Utilities
{
    public static class EditorCollisionDrawUtilities
    {
        public static readonly Color EditColor = new(0.48f, 1f, 0.22f);
        public static readonly Color EnableColor = new(0.48f, 1f, 0.22f, .5f);
        public static readonly Color DisableColor = new(0.48f, 1f, 0.22f, .2f);

        public static void DrawEditColliderButton(ref bool editable)
        {
            GUILayout.Space(10);

            var buttonStyle = new GUIStyle(EditorStyles.miniButtonLeft)
            {
                alignment = TextAnchor.MiddleCenter
            };

            var editIcon = EditorGUIUtility.IconContent("EditCollider").image as Texture2D;
            var editButtonContent = new GUIContent(editIcon);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Edit Collider: ");

            if (GUILayout.Button(editButtonContent, buttonStyle, GUILayout.Width(30), GUILayout.Height(30)))
            {
                editable = !editable;
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        public static void DrawGarbageField<TCollision, PScene>(CollisionBase<TCollision, PScene> collision)
            where TCollision : Component
        {
            if (EditorApplication.isPlaying)
                GUI.enabled = false;

            collision.GarbageColliderSize =
                EditorGUILayout.IntField("Garbage Collider Size: ", collision.GarbageColliderSize);

            GUI.enabled = true;

            EditorGUILayout.Space(10);
        }

        public static void DrawBaseInspector<TCollision, PScene>(CollisionBase<TCollision, PScene> collision)
            where TCollision : Component
        {
            DrawEditColliderButton(ref collision.Editable);
        }
    }
}