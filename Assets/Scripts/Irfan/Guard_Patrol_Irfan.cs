using UnityEngine;

public class Guard_Patrol_Irfan : MonoBehaviour
{
    [Header("Guard Animation")]
    public Animator guardAnimator;

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float detectionRange = 7f;
    public float attackRange = 1f;
    public int damage = 25; // Normal damage
    public float attackCooldown = 1.2f;
    public float jumpForce = 8f; // Jump force for obstacles
    public float jumpCooldown = 1.0f; // Added jump cooldown

    [Header("Patrol Settings")]
    public float patrolDistance = 5f; // Distance to patrol from start point
    public float patrolIdleTime = 2f; // Time to wait at patrol points

    [Header("Components")]
    public Collider2D guardCollider;
    public Transform groundCheck; // Assign a child object at feet
    public Transform wallCheck;   // Assign a child object at eye level
    public Transform hazardCheck; // Added hazard check transform
    [Range(0.1f, 1f)]
    public float hazardCheckRadius = 0.4f; // Added radius for hazard check

    public LayerMask groundLayer;
    public LayerMask hazardLayer; // Added hazard layer

    private Transform playerTarget;
    private Vector3 startPosition;
    private float lastAttackTime;
    private float lastJumpTime; // Added jump timer
    private float patrolTimer;
    private bool movingRight = true;
    private GuardHealth_Irfan myHealth; // **Updated to Irfan**
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    void Start()
    {
        myHealth = GetComponent<GuardHealth_Irfan>(); // **Updated to Irfan**
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        if (guardCollider == null) guardCollider = GetComponent<Collider2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTarget = p.transform;

        if (guardAnimator == null) guardAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (myHealth != null && myHealth.isDead)
        {
            StopMoving();
            return;
        }

        // Check if player is alive
        bool playerAlive = false;
        if (playerTarget != null)
        {
            PlayerHealth_Irfan ph = playerTarget.GetComponent<PlayerHealth_Irfan>(); // **Updated to Irfan**
            if (ph != null && ph.currentHealth > 0) playerAlive = true;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (playerAlive && distanceToPlayer <= detectionRange)
        {
            // Chase Behavior
            if (distanceToPlayer <= attackRange)
            {
                StopMoving();
                FaceTarget(playerTarget.position);
                if (Time.time > lastAttackTime + attackCooldown) Attack();
            }
            else
            {
                ChaseTarget();
            }
        }
        else
        {
            // Patrol Behavior
            Patrol();
        }
    }

    void Patrol()
    {
        // Simple patrol logic: move back and forth relative to start position
        float distFromStart = transform.position.x - startPosition.x;

        if (movingRight)
        {
            if (distFromStart > patrolDistance)
            {
                movingRight = false; // Turn back
            }
        }
        else
        {
            if (distFromStart < -patrolDistance)
            {
                movingRight = true; // Turn back
            }
        }

        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();

        if (guardAnimator != null) guardAnimator.SetBool("IsRunning", true);

        CheckJump(direction);
    }

    void ChaseTarget()
    {
        float direction = Mathf.Sign(playerTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();

        if (guardAnimator != null) guardAnimator.SetBool("IsRunning", true);

        CheckJump(direction);
    }

    void CheckJump(float direction)
    {
        if (wallCheck == null || groundCheck == null || hazardCheck == null) return;

        // 1. Check for Walls
        bool wallDetected = Physics2D.Raycast(wallCheck.position, Vector2.right * direction, 0.5f, groundLayer);

        // 2. Check for Hazards (Spikes)
        RaycastHit2D hazardHit = Physics2D.CircleCast(hazardCheck.position, hazardCheckRadius, Vector2.right * direction, 1f, hazardLayer);
        bool hazardDetected = hazardHit.collider != null;

        // 3. Check if grounded
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if ((wallDetected || hazardDetected) && isGrounded)
        {
            if (Time.time > lastJumpTime + jumpCooldown)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                if (guardAnimator != null) guardAnimator.SetTrigger("Jump");
                lastJumpTime = Time.time;
            }
        }
    }

    void FaceTarget(Vector3 targetPos)
    {
        float direction = Mathf.Sign(targetPos.x - transform.position.x);
        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (guardAnimator != null) guardAnimator.SetBool("IsRunning", false);
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        if (guardAnimator != null) guardAnimator.SetTrigger("Attack");

        PlayerHealth_Irfan ph = playerTarget.GetComponent<PlayerHealth_Irfan>(); // **Updated to Irfan**
        if (ph != null) ph.TakeDamage(damage);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}