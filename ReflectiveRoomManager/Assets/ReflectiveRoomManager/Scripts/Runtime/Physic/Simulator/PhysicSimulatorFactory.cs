using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace REFLECTIVE.Runtime.Physic
{
    public static class PhysicSimulatorFactory
    {
        public static Simulator Create(GameObject gameObject, LocalPhysicsMode physicMode)
        {
            Simulator simulator = physicMode switch
            {
                LocalPhysicsMode.Physics3D => new Simulator3D(gameObject),
                LocalPhysicsMode.Physics2D => new Simulator2D(gameObject),
                LocalPhysicsMode.None => null,
                _ => throw new ArgumentException()
            };

            return simulator;
        }
    }
}