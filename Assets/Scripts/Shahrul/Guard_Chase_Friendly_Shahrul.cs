using UnityEngine;

public class Guard_Chase_Friendly_Shahrul : MonoBehaviour
{
    [Header("Guard Animation")]
    public Animator guardAnimator;

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f; // Larger range for chase type
    public float attackRange = 1f;
    public int damage = 25; // Normal damage
    public float attackCooldown = 1.2f;
    public float jumpForce = 8f;
    public float jumpCooldown = 1.0f;

    [Header("Components")]
    public Collider2D guardCollider;
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform hazardCheck;
    [Range(0.1f, 1f)]
    public float hazardCheckRadius = 0.4f;

    public LayerMask groundLayer;
    public LayerMask hazardLayer;

    private Transform currentTarget;
    private float lastAttackTime;
    private float lastJumpTime;
    private GuardHealth_Shahrul myHealth;
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    void Start()
    {
        myHealth = GetComponent<GuardHealth_Shahrul>();
        rb = GetComponent<Rigidbody2D>();

        if (guardCollider == null) guardCollider = GetComponent<Collider2D>();

        if (guardAnimator == null) guardAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (myHealth != null && myHealth.isDead)
        {
            StopMoving();
            return;
        }

        FindTarget();

        if (currentTarget == null)
        {
            StopMoving();
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        if (distance <= attackRange)
        {
            StopMoving();
            FaceTarget(currentTarget.position);

            if (Time.time > lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
        else if (distance <= detectionRange)
        {
            ChaseTarget();
        }
        else
        {
            StopMoving();
        }
    }

    void FindTarget()
    {
        // Only target Enemies (Bandits), NEVER Player
        if (currentTarget != null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float d = Vector2.Distance(transform.position, enemy.transform.position);

            // Check range and if enemy is alive
            if (d < closestDist && d < detectionRange)
            {
                EnemyHealth_Shahrul eh = enemy.GetComponent<EnemyHealth_Shahrul>();
                if (eh != null && !eh.isDead)
                {
                    closestDist = d;
                    currentTarget = enemy.transform;
                }
            }
        }
    }

    void ChaseTarget()
    {
        float direction = Mathf.Sign(currentTarget.position.x - transform.position.x);
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

        // Attack Enemy (Bandit) Only
        EnemyHealth_Shahrul eh = currentTarget.GetComponent<EnemyHealth_Shahrul>();
        if (eh != null) eh.TakeDamage(damage);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}