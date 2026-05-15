using UnityEngine;

public class BanditBossController_Ikmal : MonoBehaviour
{
    // Defines the two types of behavior (Same as Normal Enemy)
    public enum BossBehaviorType { Normal_Idle, Patroller }

    [Header("Behavior Settings")]
    public BossBehaviorType behaviorType = BossBehaviorType.Normal_Idle;

    [Header("Patrol Settings (Timer Based)")]
    public float patrolSpeed = 2f;
    public float patrolDuration = 3f; // How long to walk in one direction

    [Header("Boss Animation")]
    public Animator bossAnimator;

    [Header("Movement Stats")]
    public float chaseSpeed = 4f;
    public float jumpForce = 8f;
    public float jumpCooldown = 1.0f;

    [Header("Combat Stats")]
    public float detectionRange = 8f; // Boss usually has slightly larger range
    public float attackRange = 1.5f;
    public int damage = 30; // Kept at 30 as requested
    public float attackCooldown = 2f;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform hazardCheck; // Optional
    [Range(0.1f, 1f)]
    public float hazardCheckRadius = 0.4f;

    public LayerMask groundLayer;
    public LayerMask hazardLayer;

    private Transform playerTarget;
    private float lastAttackTime;
    private float lastJumpTime;
    private BanditBossHealth_Ikmal myHealth; // Refers to Boss Health
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    // Patrol Variables
    private float patrolTimer;
    private int patrolDirection = -1; // -1 = Left, 1 = Right

    void Start()
    {
        myHealth = GetComponent<BanditBossHealth_Ikmal>();
        rb = GetComponent<Rigidbody2D>();
        if (bossAnimator == null) bossAnimator = GetComponent<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        // Safety check: Ensure the boss didn't target himself if accidentally tagged "Player"
        if (p != null && p != this.gameObject)
        {
            playerTarget = p.transform;
        }

        patrolTimer = patrolDuration;
    }

    void Update()
    {
        // 1. If dead, stop everything
        if (myHealth != null && myHealth.isDead)
        {
            StopMoving();
            return;
        }

        // 2. Logic Decision Tree
        if (playerTarget == null)
        {
            PerformPassiveBehavior();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= attackRange)
        {
            // Range: Attack
            StopMoving();
            if (Time.time > lastAttackTime + attackCooldown) Attack();
        }
        else if (distanceToPlayer <= detectionRange)
        {
            // Range: Chase (Overrides patrol)
            ChaseTarget();
        }
        else
        {
            // Range: Out of sight -> Passive
            PerformPassiveBehavior();
        }
    }

    void PerformPassiveBehavior()
    {
        if (behaviorType == BossBehaviorType.Normal_Idle)
        {
            StopMoving();
        }
        else if (behaviorType == BossBehaviorType.Patroller)
        {
            PatrolLogic();
        }
    }

    void PatrolLogic()
    {
        patrolTimer -= Time.deltaTime;

        if (patrolTimer <= 0)
        {
            patrolDirection *= -1; // Switch direction (Left <-> Right)
            patrolTimer = patrolDuration; // Reset timer
        }

        // Move based on direction
        rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, rb.linearVelocity.y);

        // Face the correct way
        if (patrolDirection > 0 && !isFacingRight) Flip();
        else if (patrolDirection < 0 && isFacingRight) Flip();

        // Animation
        if (bossAnimator != null) bossAnimator.SetBool("IsRunning", true);

        // Optional: Simple obstacle avoidance (Jump if hitting wall)
        CheckJump();
    }

    void ChaseTarget()
    {
        float direction = Mathf.Sign(playerTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();

        if (bossAnimator != null) bossAnimator.SetBool("IsRunning", true);

        CheckJump();
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (bossAnimator != null) bossAnimator.SetBool("IsRunning", false);
    }

    void CheckJump()
    {
        if (wallCheck == null || groundCheck == null) return;

        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;

        // Raycast forward to see wall
        bool wall = Physics2D.Raycast(wallCheck.position, dir, 0.5f, groundLayer);
        bool hazard = false;
        if (hazardCheck != null)
            hazard = Physics2D.CircleCast(hazardCheck.position, hazardCheckRadius, dir, 1f, hazardLayer).collider != null;

        // Jump logic
        if ((wall || hazard) && Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer))
        {
            if (Time.time > lastJumpTime + jumpCooldown)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                lastJumpTime = Time.time;
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        if (bossAnimator != null) bossAnimator.SetTrigger("Attack");

        // Simple damage (No knockback, No Dodge)
        PlayerHealth_Ikmal ph = playerTarget.GetComponent<PlayerHealth_Ikmal>();
        if (ph != null) ph.TakeDamage(damage);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // DEBUG: Visualize ranges in Scene View
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}