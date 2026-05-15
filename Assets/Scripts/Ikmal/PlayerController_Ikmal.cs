using UnityEngine;


public class PlayerController_Ikmal : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Animator animator;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isFacingRight = true;
    private bool isGrounded;

    // 1. Add a reference to your Combat script
    private PlayerCombat_Ikmal combatScript;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();

        // 2. Find the combat script so we can check if we are dodging
        combatScript = GetComponent<PlayerCombat_Ikmal>();
    }

    void Update()
    {
        // 3. Stop inputs if dodging
        if (combatScript != null && combatScript.isDodging) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        if (groundCheck) isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

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

        if (animator)
        {
            animator.SetBool("IsRunning", Mathf.Abs(horizontalInput) > 0.1f);
            animator.SetBool("IsJumping", !isGrounded);
        }
    }

    void FixedUpdate()
    {
        // 4. CRITICAL FIX: Do not apply movement velocity if we are dodging.
        // Let PlayerCombat_Ikmal handle the velocity instead.
        if (combatScript != null && combatScript.isDodging) return;

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void FlipSprite()
    {
        if (horizontalInput > 0 && !isFacingRight || horizontalInput < 0 && isFacingRight)
        {
            isFacingRight = !isFacingRight; Vector3 s = transform.localScale; s.x *= -1; transform.localScale = s;
        }
    }

    void OnDisable() { if (animator) { animator.SetBool("IsRunning", false); animator.SetBool("IsJumping", false); } }
}