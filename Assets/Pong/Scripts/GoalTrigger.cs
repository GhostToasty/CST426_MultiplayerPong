using UnityEngine;
using Unity.Netcode;

/*
 * GoalTrigger is a scoring zone at one end of the table.
 * In the starter, it directly tells the local GameManager who scored.
 * You will make this server-only so two clients cannot score twice.
 */

public class GoalTrigger : NetworkBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] PaddleSide scoringSide;

    public override void OnNetworkSpawn()
    {
        name = "GoalTrigger";
        base.OnNetworkSpawn();
        Debug.Log(
            $"{name} spawned | ownerClientId={OwnerClientId} | " +
            $"isOwner={IsOwner} | isServer={IsServer} | isClient={IsClient} | isHost={IsHost}");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Debug.Log($"{name} despawned");
    }


    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        
        if (other.gameObject.CompareTag("Ball"))
            gameManager.OnGoalScored(scoringSide);
    }
}