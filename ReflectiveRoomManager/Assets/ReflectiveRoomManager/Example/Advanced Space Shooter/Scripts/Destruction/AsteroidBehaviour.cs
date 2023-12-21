using Mirror;
using UnityEngine;
using System.Collections;
using REFLECTIVE.Runtime.Physic.Collision.D3;

[RequireComponent(typeof(NetworkIdentity))]
public class AsteroidBehaviour : NetworkBehaviour
{
    [SerializeField] private Collision3D _collision3D;
    
    public float spaceshipPushDistance = 125f;
    public float pushSpeedMod = 1.05f;
    private bool running;

    public bool physics;
    
    private void OnColliderEntered(Component coll)
    {
        if (coll == null) return;
        
        try
        {
            if (!running && !physics)
            {
                StartCoroutine(Moving(coll.transform));
            }
        }
        catch
        {
            // ignored
        }
    }

    [ServerCallback]
    private void Start()
    {
        _collision3D.OnCollisionEnter += OnColliderEntered;
    }
    

    private IEnumerator Moving(Transform other)
    {
        running = true;
        var dir = (transform.position - other.position).normalized;
        
        var dest = transform.position + dir * spaceshipPushDistance;

        while (Vector3.Distance(transform.position, dest) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, dest, Time.deltaTime * pushSpeedMod);
            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(-dir, transform.up),
                    Time.deltaTime / 3.5f);
            yield return null;
        }

        running = false;
    }
}