using TMPro;
using UnityEngine;
using Unity.Netcode;

/*
 * GameManager owns the local match rules: scoring, win checks, and ball resets.
 * In the starter project, everything happens in one Unity player. This is
 * the script you will convert so the server owns shared game state and the
 * score is synchronized to every client.
 */

public class GameManager : NetworkBehaviour
{
    [SerializeField] ServerBallSpawner serverBallSpawner;
    private NetworkObject ball;
    [SerializeField] float startSpeed = 3f;
    [SerializeField] Vector3 startPosition = new(0f, 0.25f, 0f);
    [SerializeField] TextMeshProUGUI leftPlayerScoreText;
    [SerializeField] TextMeshProUGUI rightPlayerScoreText;
    public bool IsServerOwned; 

    const int ScoreToWin = 11;

    private NetworkVariable<int>_leftPlayerScoreServer = new NetworkVariable<int>
        (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<int>_rightPlayerScoreServer = new NetworkVariable<int>
        (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        IsServerOwned = IsSpawned && OwnerClientId == NetworkManager.ServerClientId;
        Debug.Log(
            $"{name} spawned | ownerClientId={OwnerClientId} | " +
            $"IsServerOwned={IsServerOwned}");
        
        _leftPlayerScoreServer.OnValueChanged += HandleLeftScoreChanged;
        _rightPlayerScoreServer.OnValueChanged += HandleRightScoreChanged;

        UpdateScore();
    }

    public override void OnNetworkDespawn()
    {
        _leftPlayerScoreServer.OnValueChanged -= HandleLeftScoreChanged;
        _rightPlayerScoreServer.OnValueChanged -= HandleRightScoreChanged;
        
        base.OnNetworkDespawn();
    }
    

    public void StartGame()
    {
        if (!IsServer) return;
        
        Debug.Log("game started");
        ball = serverBallSpawner.SpawnedBall;

        float direction = Random.value < 0.5f ? -1f : 1f;
        ResetBall(direction);
    }

    public void OnGoalScored(PaddleSide scoringSide)
    {
        if (!IsServer) return;
        
        // If the ball entered a goal area, increment the score, check for win, and reset the ball
        
        Debug.Log("paddle score");
        if (scoringSide == PaddleSide.Left)
        {
            _leftPlayerScoreServer.Value++;
            Debug.Log($"Left player scored: {_leftPlayerScoreServer.Value}");

            if (_leftPlayerScoreServer.Value == ScoreToWin)
                Debug.Log("Left player wins!");
            else
                ResetBall(1f);
        }
        else if (scoringSide == PaddleSide.Right)
        {
            _rightPlayerScoreServer.Value++;
            Debug.Log($"Right player scored: {_rightPlayerScoreServer.Value}");

            if (_rightPlayerScoreServer.Value == ScoreToWin)
                Debug.Log("Right player wins!");
            else
                ResetBall(-1f);
        }
    }

    void UpdateScore()
    {
        Debug.Log("score updated");
        rightPlayerScoreText.text = _rightPlayerScoreServer.Value.ToString();
        leftPlayerScoreText.text = _leftPlayerScoreServer.Value.ToString();
    }

    void ResetBall(float directionSign)
    {
        if(!IsServer) return;
        Debug.Log("ball reset");

        // Start the ball within 20 degrees off-center toward direction indicated by directionSign
        directionSign = Mathf.Sign(directionSign);
        Vector3 newVelocity = new Vector3(directionSign, 0f, 0f) * startSpeed;
        newVelocity = Quaternion.Euler(0f, Random.Range(-20f, 20f), 0f) * newVelocity;

        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.position = startPosition;
        ballRigidbody.linearVelocity = newVelocity;
        ballRigidbody.angularVelocity = Vector3.zero;
    }

    private void HandleLeftScoreChanged(int previousValue, int newValue)
    {
        UpdateScore();
    }

    private void HandleRightScoreChanged(int previousValue, int newValue)
    {
        UpdateScore();
    }

}