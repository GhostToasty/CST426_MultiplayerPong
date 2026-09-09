using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.ShaderGraph.Internal;

public class PlayerClientBehavior : NetworkBehaviour
{
    [SerializeField] Paddle paddle;
    [SerializeField] PaddleSide paddleSide;
  
    
    public override void OnNetworkSpawn()
    {      
        base.OnNetworkSpawn();
        name = $"{NetworkObject.OwnerClientId}";
        Debug.Log(
            $"[Player] {name} spawned | ownerClientId={OwnerClientId} | " +
            $"isOwner={IsOwner} | isServer={IsServer} | isClient={IsClient} | isHost={IsHost}");
    
        //only lets the server spawn in the paddles
        if (!IsServer) return;
        SpawnPaddlePlacement();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Debug.Log($"[Player] {name} despawned");
    }

    // private void Update()
    // {
    //     if (!IsOwner) return;
    //     paddle.CheckMovePaddle();
    // } 


    private void SpawnPaddlePlacement()
    {
        //first checks how many players and then assigns the appropriate slot
        AssignSlotPosition(GetAvaliableSlot());
    }


    private int GetAvaliableSlot()
    {
        int slot = -1;

        //checks how many paddles are in the scene
        foreach (var player in FindObjectsByType<PlayerClientBehavior>(FindObjectsInactive.Exclude))
        {
            if (player.IsSpawned)
                slot++;
        }
        return slot;
    }


    private void AssignSlotPosition (int slot)
    {
        float x = 0;

        //assigns the first slot to the host and puts them on the left
        //second slot is assigned to the joining client and puts them on the right
        if (slot == 0)
            paddleSide = PaddleSide.Left;  
        else if (slot > 0)
            paddleSide = PaddleSide.Right;
        else
        {
            Debug.Log("invalid slot!");
            return;
        }

        //gets position set in the original paddle script 
        if (paddleSide == PaddleSide.Left)
            x = paddle.GetHostPosition();
        else
            x = paddle.GetClientPosition();
            
        //places the paddle in the newly assigned spot
        Vector3 assignedPosition = transform.position;
        assignedPosition.x = x;
        transform.position = assignedPosition;
    }

    //only the server is able to check for collisions with the paddle 
    void OnCollisionEnter(Collision other)
    {
        if(!IsServer) return;

        paddle.PaddleCollisionEnter(other);
    }

    //allows only the server to change to direction of the paddle based on the owners input 
    [Rpc(target:SendTo.Server)]
    private void UpdatePaddleMovementRpc(float direction)
    {
        if (!IsServer) return;
        
        paddle.MovePaddle(direction);
    }

    
    //checks for keyboard input only for the owner
    //the server cannot check this because only the owner as access to their current keyboard 
    private void Update()
    {
        if (!IsOwner) return;

        UpdatePaddleMovementRpc(paddle.CheckMovementDirection());
    }

}
