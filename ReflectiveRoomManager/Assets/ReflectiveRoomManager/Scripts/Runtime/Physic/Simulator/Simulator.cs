using UnityEngine;

namespace REFLECTIVE.Runtime.Physic
{
    public abstract class Simulator
    {
        protected readonly GameObject m_gameObject;

        protected Simulator(GameObject gameObject)
        {
            m_gameObject = gameObject;
        }
        
        public abstract void OnAwake();

        public abstract void FixedUpdate();
    }
}