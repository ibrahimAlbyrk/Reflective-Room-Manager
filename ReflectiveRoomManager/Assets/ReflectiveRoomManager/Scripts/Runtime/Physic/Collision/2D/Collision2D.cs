using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public abstract class Collision2D : CollisionBase<Collider2D>
    {
        protected new PhysicsScene2D m_physicsScene;

        protected override Collider2D[] CalculateCollision()
        {
            return null;
        }

        protected override void GetPhysicScene()
        {
            m_physicsScene = gameObject.scene.GetPhysicsScene2D();
        }
    }
}