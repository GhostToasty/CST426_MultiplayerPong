using UnityEngine;
using Unity.Netcode;

public class BallBehavior : NetworkBehaviour
{
    public bool IsServerOwned;
    
    public override void OnNetworkSpawn()
    {
        //ensures that the ball is owned by the server after it's instantiated 
        name = "ServerBall";
        IsServerOwned = IsSpawned && OwnerClientId == NetworkManager.ServerClientId;
        
        Debug.Log(
            $"{name} spawned | ownerClientId={OwnerClientId} | " +
            $"IsServerOwned={IsServerOwned}");
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"{name} despawned");
    }

    private void LateUpdate()
    {
        if (!IsServer) return;
    }

}
