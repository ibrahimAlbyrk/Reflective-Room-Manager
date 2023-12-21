using Mirror;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using Examples.SpaceShooter.Utilities;

public class DestructionScript : NetworkBehaviour
{
    public GameObject Explosion;
    public Transform particleParent;

    [SyncVar] public float HP;

    public float ExplosionScale = 10f;

    private bool dead;

    [ServerCallback]
    private void Update()
    {
        if (!dead && HP <= 0f)
        {
            StartCoroutine(Death());
            dead = true;
        }
    }

    //TODO: Run If HP is Zero
    [ServerCallback]
    private IEnumerator Death()
    {
        RPC_CloseVisualAndInteraction();

        if (Explosion != null)
        {
            RPC_SpawnFireworks(transform.position, transform.rotation, 2);

            yield return new WaitForSeconds(2.5f);

            NetworkServer.Destroy(gameObject);
        }
        else
            NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RPC_CloseVisualAndInteraction()
    {
        if(TryGetComponent(out Renderer visual))
        {
            visual.enabled = false;
        }
        
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>() != null)
            {
                child.GetComponent<Renderer>().enabled = false;
            }
        }

        if (particleParent != null)
        {
            foreach (Transform child in particleParent)
            {
                child.gameObject.SetActive(false);
            }
        }

        GetComponent<Collider>().enabled = false;
    }

    [ClientRpc]
    private async void RPC_SpawnFireworks(Vector3 pos, Quaternion rot, int destroyCountdown)
    {
        if (Explosion == null) return;
        
        var firework = Instantiate(Explosion, pos, rot);
        firework.transform.localScale *= ExplosionScale;

        firework.GetComponent<ParticleSystem>().Play();

        await Task.Delay(destroyCountdown * 1000);

        if (firework == null) return;

        firework.Destroy();
    }
}