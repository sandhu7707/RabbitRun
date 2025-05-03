using UnityEngine;

public class HoppingCharacter : MonoBehaviour
{
    public float hopForce = 10f;
    public float hopDistance = 2f;
    public LayerMask groundLayer; // Layer for ground detection
    public float rayCastDistance = 0.1f;  // Distance for ground check
    public float gravity = -9.81f;
    public float maxFallSpeed = -10f;

    private CharacterController characterController;
    private Vector3 velocity;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Ground check using raycast
        bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.01f, Vector3.down, rayCastDistance, groundLayer);

        if(characterController.isGrounded || isGrounded) // Check if character is grounded
        {
            velocity.y = 0;
        }

        // Apply gravity
        if (velocity.y > maxFallSpeed)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // Apply move using character controller
        characterController.Move(velocity * Time.deltaTime);
    }


    // Called when the character enters a trigger collider
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Jumpable")) {
            // Apply a jump or hop based on the trigger
            velocity.y = Mathf.Sqrt(hopForce * -2f * gravity);
        }
    }
}