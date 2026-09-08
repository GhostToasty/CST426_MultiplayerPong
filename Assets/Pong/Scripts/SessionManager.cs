using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/*
 * SessionManager is intentionally light in the starter project.
 * Local Pong starts immediately. The GameManager, NetworkManager, and button
 * references mark where you will start a Host or Client session.
 */

public class SessionManager : NetworkBehaviour
{
    [Header("Multiplayer")]
    [SerializeField] GameManager gameManager;
    
    // This does not already exist in the scene, you need to add it and reference it
    [SerializeField] NetworkManager networkManager;

    [Header("Multiplayer UI")]
    [SerializeField] Button startHostButton;
    [SerializeField] Button startClientButton;
    [SerializeField] Canvas sessionUI;
    [SerializeField] bool gameStart;

    void Awake()
    {
        // Hide Host/Client until you are ready to wire the session.
        sessionUI.gameObject.SetActive(true);
        gameStart = false;
        
        startHostButton.onClick.AddListener(() => StartHost());
        startClientButton.onClick.AddListener(() => StartClient());
    }


    private void Update()
    {
        if (!gameStart)
            CheckReadyPlayers();
    }

    public void StartHost()
    {
        NetworkManager!.StartHost();
        sessionUI.gameObject.SetActive(false);
    }

    public void StartClient()
    {
        NetworkManager!.StartClient();
        sessionUI.gameObject.SetActive(false);
    }

    public void CheckReadyPlayers()
    {
        if (!IsServer) return;
        
        Debug.Log("check players");
        int playerCount = 0;

        //checks how many paddles are in the scene
        foreach (var player in FindObjectsByType<PlayerClientBehavior>(FindObjectsInactive.Exclude))
        {
            if (player.IsSpawned)
                playerCount++;
        }
        
        if (playerCount == 2)
        {
            gameManager.StartGame();
            gameStart = true;
        }
            
    }

}