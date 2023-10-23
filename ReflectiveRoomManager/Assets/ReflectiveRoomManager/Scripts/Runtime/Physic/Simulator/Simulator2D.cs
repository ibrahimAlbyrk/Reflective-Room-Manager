using UnityEngine;

namespace REFLECTIVE.Runtime.Physic
{
    public class Simulator2D : Simulator
    {
        private PhysicsScene2D _physicsScene;
        
        public Simulator2D(GameObject gameObject) : base(gameObject)
        {
        }
        
        public override void OnAwake()
        {
            _physicsScene = m_gameObject.scene.GetPhysicsScene2D();
        }

        public override void FixedUpdate()
        {
            _physicsScene.Simulate(Time.fixedDeltaTime);
        }
    }
}