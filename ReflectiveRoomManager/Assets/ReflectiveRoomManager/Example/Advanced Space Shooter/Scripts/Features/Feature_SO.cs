using UnityEngine;

namespace Examples.SpaceShooter.Features
{
    using Spaceship;
    
    public abstract class Feature_SO : ScriptableObject
    {
        [Header("Feature Settings")]
        public string Name = "New Feature";
        public float Duration = 5f;

        public Sprite Icon;

        protected bool _isInitied;
        
        public SpaceshipController OwnedController { get; set; }

        public abstract void OnStart(SpaceshipController ownedController);

        public virtual void OnInit() => _isInitied = true;
        
        public abstract void OnUpdate();

        public abstract void OnEnd();
    }
}