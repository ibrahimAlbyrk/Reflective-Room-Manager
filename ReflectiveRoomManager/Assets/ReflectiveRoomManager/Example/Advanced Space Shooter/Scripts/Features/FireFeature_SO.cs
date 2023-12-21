using UnityEngine;

namespace Examples.SpaceShooter.Features
{
    using Spaceship;
    
    [CreateAssetMenu(menuName = "Features/Fire Feature", order = 0)]
    public class FireFeature_SO : Feature_SO
    {
        [Header("Fire Settings")]
        [SerializeField] public Vector2 IncreaseBulletCountRange = new (2, 4);
        [SerializeField] public Vector2 IncreaseTargetDistanceRange = new (50, 300);
        [SerializeField] public Vector2 IncreaseSpeedRange = new (200, 500);

        private float _bulletCount;
        private float _targetDistance;
        private float _speed;

        public override void OnStart(SpaceshipController ownedController)
        {
            OwnedController = ownedController;
            
            if(_bulletCount != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.BulletCount += _bulletCount;
            
            if(_targetDistance != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.TargetDistance += _targetDistance;
            
            if(_speed != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.BulletSpeed += _speed;
        }

        public override void OnUpdate()
        {
        }

        public override void OnEnd()
        {
            if(_bulletCount != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.BulletCount -= _bulletCount;
            
            if(_targetDistance != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.TargetDistance -= _targetDistance;
            
            if(_speed != 0)
                OwnedController.Shooter.m_shooting.bulletSettings.BulletSpeed -= _speed;
        }

        public void SetBulletSettings(float bulletCount, float bulletDistance, float bulletSpeed)
        {
            _bulletCount = bulletCount;
            _targetDistance = bulletDistance;
            _speed = bulletSpeed;
        }
    }
}