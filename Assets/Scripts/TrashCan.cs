using UnityEngine;
using System.Collections;

public class TrashCan : MonoBehaviour
{
    public Transform respawnPoint; // assign a spawn location for reset
    public GameObject mugPrefab; // prefab to spawn
    public float respawnDelay = 0.5f;
    public GameObject mug; // optional reference to the current mug in scene


    private void OnTriggerEnter(Collider other)
    {
        // If a specific mug reference is assigned, only act on that object.
        if (mug != null)
        {
            if (other.gameObject == mug)
            {
                StartCoroutine(DestroyAndRespawn(other.gameObject));
            }
            return;
        }
    }

    public IEnumerator DestroyAndRespawn(GameObject toDestroy, System.Action<GameObject> onRespawned = null)
    {
        if (toDestroy == null)
        {
            Debug.LogWarning("TrashCan: toDestroy is null!");
            yield break;
        }

        // Disable collisions/visuals immediately so player sees it removed
        Collider col = toDestroy.GetComponent<Collider>();
        if (col) col.enabled = false;
        Renderer[] rends = toDestroy.GetComponentsInChildren<Renderer>();
        foreach (var r in rends) r.enabled = false;



        // Wait a short moment so any smash animations or sound can finish
        yield return new WaitForSeconds(respawnDelay);

        // Destroy the old mug instance
        Destroy(toDestroy);

        // Spawn new mug from prefab at respawnPoint
        // Spawn new mug from prefab at respawnPoint
        if (mugPrefab != null && respawnPoint != null)
        {
            GameObject newMug = Instantiate(mugPrefab, respawnPoint.position, respawnPoint.rotation);

            // ✅ Parent it under the drink component (or whatever container you need)
            newMug.transform.SetParent(respawnPoint.parent, worldPositionStays: true);

            mug = newMug;

            Debug.Log("Respawned new mug at " + newMug.transform.position);
        }
        else
        {
            Debug.LogWarning("TrashCan: mugPrefab or respawnPoint not assigned.");
        }
    }
}