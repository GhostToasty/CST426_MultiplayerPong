using UnityEngine;
using Unity.Netcode;

public class BallBehavior : NetworkBehaviour
{
    public bool IsServerOwned;
    // private Rigidbody rb;
    
    public override void OnNetworkSpawn()
    {
        name = "ServerBall";
        IsServerOwned = IsSpawned && OwnerClientId == NetworkManager.ServerClientId;
        
        Debug.Log(
            $"{name} spawned | ownerClientId={OwnerClientId} | " +
            $"IsServerOwned={IsServerOwned}");

        // SetRigidbody();
    }

    public override void OnNetworkDespawn()
    {
        Debug.Log($"{name} despawned");
    }

    private void LateUpdate()
    {
        if (!IsServer) return;
    }

    // private void SetRigidbody()
    // {
    //     rb = GetComponent<Rigidbody>();

    //     if (IsServer)
    //         rb.interpolation = RigidbodyInterpolation.Interpolate;
    // }

}
