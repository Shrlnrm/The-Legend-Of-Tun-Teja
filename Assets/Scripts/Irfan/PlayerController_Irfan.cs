using UnityEngine;

public class PlayerController_Irfan : MonoBehaviour
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
    private PlayerDodge_Irfan dodgeScript;
    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dodgeScript = GetComponent<PlayerDodge_Irfan>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Don't process input if dodging
        if (dodgeScript != null && dodgeScript.IsDodging())
            return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // PLAY JUMP SOUND
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayJumpSound();
            }
        }

        FlipSprite();

        if (animator != null)
        {
            animator.SetBool("IsRunning", Mathf.Abs(horizontalInput) > 0.1f);
            animator.SetBool("IsJumping", !isGrounded);
        }
    }

    void FixedUpdate()
    {
        // Don't move if dodging (dodge script handles movement)
        if (dodgeScript != null && dodgeScript.IsDodging())
            return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void FlipSprite()
    {
        if (dodgeScript != null && dodgeScript.IsDodging())
            return; // Don't flip during dodge

        if (horizontalInput > 0 && !isFacingRight || horizontalInput < 0 && isFacingRight)
        {
            isFacingRight = !isFacingRight;
            Vector3 s = transform.localScale; s.x *= -1; transform.localScale = s;
        }
    }

    void OnDisable()
    {
        if (animator != null) { animator.SetBool("IsRunning", false); animator.SetBool("IsJumping", false); }
    }

    // Helper method for dodge script
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetHorizontalInput()
    {
        return horizontalInput;
    }
}