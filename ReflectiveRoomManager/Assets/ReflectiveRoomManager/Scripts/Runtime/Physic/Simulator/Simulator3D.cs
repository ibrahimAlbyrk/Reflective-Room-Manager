using UnityEngine;

namespace REFLECTIVE.Runtime.Physic
{
    public class Simulator3D : Simulator
    {
        private PhysicsScene _physicsScene;
        
        public Simulator3D(GameObject gameObject) : base(gameObject)
        {
        }
        
        public override void OnAwake()
        {
            _physicsScene = m_gameObject.scene.GetPhysicsScene();
        }

        public override void FixedUpdate()
        {
            _physicsScene.Simulate(Time.fixedDeltaTime);
        }
    }
}