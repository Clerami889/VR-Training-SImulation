using UnityEngine;

public class MugSpawner : MonoBehaviour
{
    public GameObject mugPrefab;
    public Transform spawnPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject SpawnEmptyMug()
    {
        return Instantiate(mugPrefab, spawnPoint.position, spawnPoint.rotation);
    }

}
