using UnityEngine;


public class SpawnpointController : MonoBehaviour
{
    public static SpawnpointController instance;
    [SerializeField] private Transform[] spawnpoints;
    
    private void Awake()
    {
        if (!instance) instance = this;
    }

    public Transform GetRandomSpawnpoint()
    {
        return spawnpoints[Random.Range(0, spawnpoints.Length)];
    }
}
