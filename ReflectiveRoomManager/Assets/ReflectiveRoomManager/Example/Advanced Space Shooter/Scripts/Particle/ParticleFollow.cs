using UnityEngine;

namespace Examples.SpaceShooter.Particle
{
    using Spaceship;
    
    public sealed class ParticleFollow : MonoBehaviour
    {
        private Transform m_cachedTransform;
        private ParticleSystemRenderer m_particleRenderer;
        [SerializeField] private Vector2 m_particleScaleRange;
        [SerializeField] private float m_positionSmooth;
        [SerializeField] private float m_rotationSmooth = 50f;
        [SerializeField] private SpaceshipController m_target;

        private void Awake()
        {
            m_cachedTransform = transform;
            m_particleRenderer = GetComponent<ParticleSystemRenderer>();
        }

        private void Update()
        {
            if (!m_target.IsInitialized) return;

            m_cachedTransform.position = Vector3.Lerp(m_cachedTransform.position,
                m_target.CachedTransform.position, Time.deltaTime * m_positionSmooth);

            m_cachedTransform.rotation = Quaternion.Slerp(m_cachedTransform.rotation,
                m_target.CachedTransform.rotation, Time.deltaTime * m_rotationSmooth);

            m_particleRenderer.lengthScale =
                Mathf.Lerp(m_particleScaleRange.x, m_particleScaleRange.y, Input.GetAxis("Stop") == 0f
                    ? m_target.SpeedFactor
                    : m_target.m_spaceship.AccelerationCurve.Evaluate(0f));
        }
    }

}