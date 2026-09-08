using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ServerBallSpawner : MonoBehaviour
{
    [SerializeField] NetworkObject ballPrefab;
    [SerializeField] Vector3 spawnPosition = new (0f, 0f, 0f);
    //makes the instance of the ball a public object to be used by the GameManager
    public NetworkObject SpawnedBall => _spawnedBall;
    NetworkObject _spawnedBall;
    private bool hasBallSpawned;

    
    private void Awake()
    {
        hasBallSpawned = false;
    }
    
    private void Update()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.IsListening || !networkManager.IsServer) return;

        //only spawns the ball if it hasn't been already 
        if (!hasBallSpawned)
            SpawnBall();
    }

    private void SpawnBall()
    {   
        //intstantiates the ball and marks it as spawned 
        hasBallSpawned = true;
        _spawnedBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
        _spawnedBall.Spawn();
        
        Debug.Log(
            $"[ServerBall] Server spawned ball | ownerClientId={_spawnedBall.OwnerClientId} | " +
            $"networkObjectId={_spawnedBall.NetworkObjectId}");
    }

    private void DespawnBall()
    {
        if (_spawnedBall.IsSpawned)
        {
            Debug.Log($"[ServerBall] Server despawning ball | networkObjectId={_spawnedBall.NetworkObjectId}");
            _spawnedBall.Despawn();
        }
    }
}
