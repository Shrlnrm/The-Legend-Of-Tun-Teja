using UnityEngine;

public class EnemyController_Irfan : MonoBehaviour
{
    [Header("Enemy Animation")]
    public Animator enemyAnimator;

    [Header("Movement Stats")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float jumpCooldown = 1.0f;

    [Header("Combat Stats")]
    public float detectionRange = 6f;
    public float attackRange = 1f;
    public int damage = 10;
    public float attackCooldown = 1.5f;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform hazardCheck;
    [Range(0.1f, 1f)]
    public float hazardCheckRadius = 0.4f;

    public LayerMask groundLayer;
    public LayerMask hazardLayer;

    private Transform playerTarget;
    private float lastAttackTime;
    private float lastJumpTime;
    private EnemyHealth_Irfan myHealth;
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    void Start()
    {
        myHealth = GetComponent<EnemyHealth_Irfan>();
        rb = GetComponent<Rigidbody2D>();

        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTarget = p.transform;
    }

    void Update()
    {
        if (myHealth != null && myHealth.isDead)
        {
            StopMoving();
            return;
        }

        if (playerTarget == null) return;

        float distance = Vector2.Distance(transform.position, playerTarget.position);

        if (distance <= attackRange)
        {
            StopMoving();
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

    void ChaseTarget()
    {
        float direction = Mathf.Sign(playerTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();

        if (enemyAnimator != null)
        {
            if (Mathf.Abs(rb.linearVelocity.y) < 0.1f) enemyAnimator.SetBool("IsRunning", true);
            else enemyAnimator.SetBool("IsRunning", false);
        }

        CheckJump();
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (enemyAnimator != null) enemyAnimator.SetBool("IsRunning", false);
    }

    void CheckJump()
    {
        if (wallCheck == null || groundCheck == null || hazardCheck == null) return;

        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        bool wallDetected = Physics2D.Raycast(wallCheck.position, direction, 0.5f, groundLayer);
        RaycastHit2D hazardHit = Physics2D.CircleCast(hazardCheck.position, hazardCheckRadius, direction, 1f, hazardLayer);
        bool hazardDetected = hazardHit.collider != null;
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if ((wallDetected || hazardDetected) && isGrounded)
        {
            if (Time.time > lastJumpTime + jumpCooldown)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                if (enemyAnimator != null) enemyAnimator.SetTrigger("Jump");
                lastJumpTime = Time.time;
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack");

        PlayerHealth_Irfan ph = playerTarget.GetComponent<PlayerHealth_Irfan>();
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