using UnityEngine;

public class PlayerDodge_Irfan : MonoBehaviour
{
    [Header("Dodge Settings")]
    public float dodgeSpeed = 15f;
    public float dodgeDuration = 0.5f; // Increased to match animation
    public float dodgeCooldown = 1f;
    public KeyCode dodgeKey = KeyCode.LeftShift;

    [Header("References")]
    public Animator animator;

    private Rigidbody2D rb;
    private PlayerController_Irfan controller;
    private float lastDodgeTime;
    private bool isDodging;
    private Vector2 dodgeDirection;
    private float dodgeStartTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController_Irfan>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(dodgeKey) && CanDodge())
        {
            StartDodge();
        }

        // REMOVED: Don't auto-end dodge by time
        // Let animation event or state transition handle it
    }

    void FixedUpdate()
    {
        if (isDodging)
        {
            // Calculate movement based on animation progress
            float elapsed = Time.time - dodgeStartTime;
            float progress = Mathf.Clamp01(elapsed / dodgeDuration);

            // Optional: Add curve for movement (fast start, slow end)
            float speedMultiplier = Mathf.Lerp(1f, 0.5f, progress);

            rb.linearVelocity = dodgeDirection * dodgeSpeed * speedMultiplier;
        }
    }

    bool CanDodge()
    {
        return !isDodging &&
               Time.time - lastDodgeTime > dodgeCooldown &&
               controller != null &&
               Mathf.Abs(controller.GetHorizontalInput()) > 0.1f;
    }

    void StartDodge()
    {
        isDodging = true;
        dodgeStartTime = Time.time;
        lastDodgeTime = Time.time;

        // Determine direction based on current input
        float horizontal = controller.GetHorizontalInput();
        dodgeDirection = new Vector2(Mathf.Sign(horizontal), 0);

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("Dodge");

            // Reset other animations
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
        }

        Debug.Log("Dodge started");
    }

    // CALL THIS METHOD FROM ANIMATION EVENT
    public void EndDodge()
    {
        isDodging = false;
        Debug.Log("Dodge ended");
    }

    public bool IsDodging()
    {
        return isDodging;
    }
}