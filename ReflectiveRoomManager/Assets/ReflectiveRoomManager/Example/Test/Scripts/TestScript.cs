using UnityEngine;
using Collision2D = REFLECTIVE.Runtime.Physic.Collision.D2.Collision2D;

namespace Example.Test
{
    public class TestScript : MonoBehaviour
    {
        [SerializeField]
        private Collision2D _collision2D;

        private void Start()
        {
            _collision2D.OnCollisionEnter += Enter;
            _collision2D.OnCollisionExit += Exit;
        }

        private static void Enter(Collider2D obj)
        {
            if (obj == null) return;
            
            Debug.Log("enter");
        }
        
        private static void Exit(Collider2D obj)
        {
            Debug.Log("exit");
        }
    }
}
