using UnityEngine;

namespace REFLECTIVE.Runtime.Physic.Collision.D3
{
    public abstract class Collision3D : CollisionBase<Collider>
    {
        protected override Collider[] CalculateCollision()
        {
            return null;
        }

        protected override void GetPhysicScene()
        {
            m_physicsScene = gameObject.scene.GetPhysicsScene();
        }
    }
}