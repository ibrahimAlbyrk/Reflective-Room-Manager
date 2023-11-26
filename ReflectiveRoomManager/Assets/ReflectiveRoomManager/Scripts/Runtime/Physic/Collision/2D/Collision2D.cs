using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D2
{
    public abstract class Collision2D : CollisionBase<Collider2D, PhysicsScene2D>
    {
        protected override void CalculateCollision()
        {
        }

        protected override void GetPhysicScene()
        {
            m_physicsScene = gameObject.scene.GetPhysicsScene2D();
        }
    }
}