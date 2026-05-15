using UnityEngine;

public class TunTejaController_Ikmal : MonoBehaviour
{
    [Header("Movement Stats")]
    public float moveSpeed = 4f;
    public float jumpForce = 10f;
    public float stopDistance = 2f;
    public float jumpCooldown = 1f;

    [Header("Animation")]
    public Animator tejaAnimator;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;

    [Header("Hazard Detection")]
    public LayerMask hazardLayer; // Assign the layer where your Spikes are (e.g., "Default" or "Hazard")
    public float hazardCheckDistance = 2f; // Distance to see the spike
    public float hazardHeightOffset = 0.1f; // Adjustable height for the hazard ray (0 = feet level)

    // New Header for Pit Detection
    [Header("Pit Detection")]
    public Transform pitCheck; // Assign an empty object slightly in front of her feet
    public float pitCheckDistance = 1f; // How far down to look for ground

    private Transform player;
    private Rigidbody2D rb;
    private TunTejaHealth_Ikmal myHealth;
    private bool isFacingRight = true;
    private float lastJumpTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myHealth = GetComponent<TunTejaHealth_Ikmal>();
        if (tejaAnimator == null) tejaAnimator = GetComponent<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        Collider2D myCol = GetComponent<Collider2D>();
        Collider2D pCol = p != null ? p.GetComponent<Collider2D>() : null;
        if (myCol && pCol) Physics2D.IgnoreCollision(myCol, pCol);

        // Ignore collision with Guards
        GuardHealth_Ikmal[] guards = FindObjectsOfType<GuardHealth_Ikmal>();
        foreach (GuardHealth_Ikmal guard in guards)
        {
            Collider2D gCol = guard.GetComponent<Collider2D>();
            if (myCol && gCol) Physics2D.IgnoreCollision(myCol, gCol);
        }
    }

    void Update()
    {
        if (myHealth != null && myHealth.isDead)
        {
            StopMoving();
            return;
        }

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            if (direction > 0 && !isFacingRight) Flip();
            else if (direction < 0 && isFacingRight) Flip();

            if (tejaAnimator != null) tejaAnimator.SetBool("IsRunning", true);

            CheckJump(direction);
        }
        else
        {
            StopMoving();
        }
    }

    void CheckJump(float direction)
    {
        if (wallCheck == null || groundCheck == null) return;

        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // 1. Check for Wall (Existing logic)
        bool wallAhead = Physics2D.Raycast(wallCheck.position, Vector2.right * direction, 0.6f, groundLayer);

        // 2. Check for Hazard/Spike Forward (Existing logic - Adjustable Origin)
        Vector2 hazardOrigin = new Vector2(wallCheck.position.x, groundCheck.position.y + hazardHeightOffset);
        bool hazardAhead = Physics2D.Raycast(hazardOrigin, Vector2.right * direction, hazardCheckDistance, hazardLayer);

        // 3. Check for Pit AND Hazard BELOW (NEW LOGIC)
        Vector2 origin = pitCheck != null ? pitCheck.position : wallCheck.position + (Vector3)(Vector2.right * direction * 0.5f);

        // Check if there is ground
        RaycastHit2D groundHit = Physics2D.Raycast(origin, Vector2.down, pitCheckDistance, groundLayer);
        bool pitAhead = (groundHit.collider == null);

        // Check if there is a hazard directly below (e.g. spikes on floor)
        RaycastHit2D hazardBelowHit = Physics2D.Raycast(origin, Vector2.down, pitCheckDistance, hazardLayer);
        bool hazardBelow = (hazardBelowHit.collider != null);

        // Jump if: Wall OR Pit OR Hazard Forward OR Hazard Below
        if (isGrounded && (wallAhead || pitAhead || hazardAhead || hazardBelow))
        {
            if (Time.time > lastJumpTime + jumpCooldown)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                lastJumpTime = Time.time;
                if (tejaAnimator != null) tejaAnimator.SetTrigger("Jump");
            }
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (tejaAnimator != null) tejaAnimator.SetBool("IsRunning", false);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmos()
    {
        if (pitCheck != null)
        {
            // Red line for Pit Check (Ground)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pitCheck.position, pitCheck.position + Vector3.down * pitCheckDistance);

            // Purple line for Hazard Check (Below)
            Gizmos.color = new Color(1f, 0f, 1f, 0.5f); // Purple
            Gizmos.DrawLine(pitCheck.position, pitCheck.position + Vector3.down * pitCheckDistance);
        }

        if (wallCheck != null && groundCheck != null)
        {
            // Yellow line for Hazard Check (Forward)
            Gizmos.color = Color.yellow;
            float direction = isFacingRight ? 1 : -1;
            Vector3 hazardOrigin = new Vector3(wallCheck.position.x, groundCheck.position.y + hazardHeightOffset, 0);
            Gizmos.DrawLine(hazardOrigin, hazardOrigin + Vector3.right * direction * hazardCheckDistance);
        }
    }
}