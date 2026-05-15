using UnityEngine;

public class PlayerController_Shahrul : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("References")]
    public Animator animator;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr; // Reference to SpriteRenderer
    private float horizontalInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // PLAY JUMP SOUND
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayJumpSound();
            }
        }

        if (animator != null)
        {
            bool isRunning = Mathf.Abs(horizontalInput) > 0.1f;
            animator.SetBool("IsRunning", isRunning);
            animator.SetBool("IsJumping", !isGrounded);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    // Use LateUpdate to override Animation properties
    void LateUpdate()
    {
        if (sr == null) return;

        // If moving LEFT (Negative), face LEFT
        if (horizontalInput < 0)
        {
            // If your sprite naturally faces RIGHT, flipX = true makes it face LEFT.
            // If your sprite naturally faces LEFT, flipX = false makes it face LEFT.
            // Adjust this boolean based on your specific sprite sheet.
            // Assuming standard sprite sheet facing Right:
            sr.flipX = true;
        }
        // If moving RIGHT (Positive), face RIGHT
        else if (horizontalInput > 0)
        {
            sr.flipX = false;
        }
    }

    void OnDisable()
    {
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
        }
    }
}