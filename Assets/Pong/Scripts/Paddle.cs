using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Paddle owns local paddle movement and the ball bounce when the ball hits it.
 * In this starter, both paddles live in the scene and read local keyboard input.
 * In the multiplayer solution, this same responsibility splits by authority:
 * the owning client reads input, while the server handles shared ball physics.
 *
 * LeftX / RightX are magic world positions, not a real placement system.
 * Move the scene around and these numbers are silently wrong. We keep them so
 * the lesson stays on ownership instead of spawn-point wiring. The same
 * shortcut can stay in the multiplayer version: a Netcode-spawned prefab
 * cannot drag in the scene's goals.
 *
 * Doing this properly would mean storing the positions on GameManager,
 * searching the scene for the goals (easy to get wrong), or flipping the
 * prefab X if the table is centered at 0.
 */

public class Paddle : MonoBehaviour
{
    [SerializeField] PaddleSide paddleSide;
    [SerializeField] float minTravelZ;
    [SerializeField] float maxTravelZ;
    [SerializeField] float speed;
    [SerializeField] float collisionBallSpeedUp = 1.5f;
    
    // Local two-player needs separate keys per paddle. InputSystem_Actions
    // already has a Player/Paddle axis (W/S) for the one-owner step.
    [SerializeField] Key moveUpKey = Key.W;
    [SerializeField] Key moveDownKey = Key.S;

    // Demo shortcut — see the class header note on LeftX / RightX.
    const float LeftX = -7.5f;
    const float RightX = 7.5f;

    //sends the set positions to the PlayerClient Behavior to help spawn in paddles 
    public float GetHostPosition()
    {
        return LeftX;
    }

    public float GetClientPosition()
    {
        return RightX;
    }

    
    //checks if the paddle is moved, only done by the owner
    public float CheckMovementDirection()
    {
        float direction = 0f;
        if (Keyboard.current[moveUpKey].isPressed) direction += 1f;
        if (Keyboard.current[moveDownKey].isPressed) direction -= 1f;

        return direction;
    }
    
    //moves the paddle based on the given direction, only done by ther server 
    public void MovePaddle(float direction)
    {
        Vector3 newPosition = transform.position + new Vector3(0f, 0f, direction) * speed * Time.deltaTime;
        newPosition.z = Mathf.Clamp(newPosition.z, minTravelZ, maxTravelZ);

        transform.position = newPosition;
    }

    //collisions are only detected by the server 
    public void PaddleCollisionEnter(Collision other)
    {   
        Debug.Log("collission happened");
        
        // Get world-space bounds
        var paddleBounds = GetComponent<BoxCollider>().bounds;

        float paddleCenterZ = paddleBounds.center.z;
        float paddleHalfHeight = paddleBounds.extents.z;
        float hitZ = other.GetContact(0).point.z;

        // Get a parameterized value roughly in the -1 to 1 range for where the ball hits
        float normalizedHit = (hitZ - paddleCenterZ) / paddleHalfHeight;

        // Cap it so that it stay within range (happens when hitting the corner of the paddle)
        float bounceDirection = Mathf.Clamp(normalizedHit, -1f, 1f);

        // Ideally we would use linearVelocity here.  Unfortunately, it is 0-length during the collision
        Vector3 currentVelocity = other.relativeVelocity;

        // The flipped sign will change the velocity direction appropriately for both paddles
        float newSign = -Mathf.Sign(currentVelocity.x);

        // Change the velocity between -60 to 60 degrees based on where it hit the paddle
        float newSpeed = currentVelocity.magnitude * collisionBallSpeedUp;
        float newAngle = 60f * bounceDirection * Mathf.Deg2Rad;

        // Calculate new velocity vector - using trig and scaled by new speed
        Vector3 newVelocity = new Vector3(newSign * Mathf.Cos(newAngle), 0f, Mathf.Sin(newAngle)) * newSpeed;
        other.rigidbody.linearVelocity = newVelocity;
    }
}