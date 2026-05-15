using UnityEngine;

public class EnemyController_Ikmal : MonoBehaviour
{
    [Header("Enemy Animation")]
    public Animator enemyAnimator;

    [Header("Movement Stats")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float jumpCooldown = 1.0f;

    [Header("Combat Stats")]
    public float detectionRange = 6f;
    public float attackRange = 2.0f;
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

    private Transform currentTarget;
    private float lastAttackTime;
    private float lastJumpTime;
    private EnemyHealth_Ikmal myHealth;
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    private float debugTimer;

    void Start()
    {
        myHealth = GetComponent<EnemyHealth_Ikmal>();
        rb = GetComponent<Rigidbody2D>();
        if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
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
            if (Time.time > lastAttackTime + attackCooldown) Attack();
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
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        GameObject teja = GameObject.FindGameObjectWithTag("TunTeja");

        float distToPlayer = Mathf.Infinity;
        float distToTeja = Mathf.Infinity;

        if (p != null)
        {
            PlayerHealth_Ikmal ph = p.GetComponent<PlayerHealth_Ikmal>();
            if (ph != null && ph.currentHealth > 0)
                distToPlayer = Vector2.Distance(transform.position, p.transform.position);
        }

        if (teja != null)
        {
            // Try to get the health script
            TunTejaHealth_Ikmal th = teja.GetComponent<TunTejaHealth_Ikmal>();

            if (th != null && !th.isDead)
            {
                distToTeja = Vector2.Distance(transform.position, teja.transform.position);
            }
            else if (th == null)
            {
                // ERROR FOUND: We found an object with the tag, but it has no script.
                // We print the name of the object so you can find it in the Hierarchy.
                if (Time.time > debugTimer)
                {
                    Debug.LogError($"ERROR: I found an object named '{teja.name}' with the tag 'TunTeja', but it does NOT have the 'TunTejaHealth_Ikmal' script attached!");
                    debugTimer = Time.time + 3f;
                }
            }
        }

        if (distToTeja < distToPlayer && distToTeja < detectionRange)
        {
            currentTarget = teja.transform;
        }
        else if (distToPlayer < detectionRange)
        {
            currentTarget = p.transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    void ChaseTarget()
    {
        float direction = Mathf.Sign(currentTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();

        if (enemyAnimator != null) enemyAnimator.SetBool("IsRunning", true);

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
        Vector2 dir = isFacingRight ? Vector2.right : Vector2.left;
        bool wall = Physics2D.Raycast(wallCheck.position, dir, 0.5f, groundLayer);
        bool hazard = Physics2D.CircleCast(hazardCheck.position, 0.4f, dir, 1f, hazardLayer).collider != null;
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
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Attack");

        PlayerHealth_Ikmal ph = currentTarget.GetComponent<PlayerHealth_Ikmal>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            return;
        }

        TunTejaHealth_Ikmal th = currentTarget.GetComponent<TunTejaHealth_Ikmal>();
        if (th != null)
        {
            th.TakeDamage(damage);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}