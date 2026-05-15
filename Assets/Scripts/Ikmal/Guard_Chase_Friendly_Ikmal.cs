using UnityEngine;

public class Guard_Chase_Friendly_Ikmal : MonoBehaviour
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
    private GuardHealth_Ikmal myHealth;
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    // ADDED: To remember start position
    private Vector3 spawnPosition;

    void Start()
    {
        myHealth = GetComponent<GuardHealth_Ikmal>();
        rb = GetComponent<Rigidbody2D>();

        // Remember where we started so we can go back
        spawnPosition = transform.position;

        if (guardCollider == null) guardCollider = GetComponent<Collider2D>();

        if (guardAnimator == null) guardAnimator = GetComponent<Animator>();

        // 1. Ignore collision with Player so they can walk through each other
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && guardCollider != null)
        {
            // Get all colliders on player (in case they have multiple)
            Collider2D[] playerColliders = player.GetComponents<Collider2D>();
            foreach (Collider2D pCol in playerColliders)
            {
                Physics2D.IgnoreCollision(guardCollider, pCol, true);
            }
        }

        // 2. ADDED: Ignore collision with other Guards so they can pass through each other
        if (guardCollider != null)
        {
            // Find all objects that have the GuardHealth_Ikmal script (identifying them as guards)
            GuardHealth_Ikmal[] allGuards = FindObjectsOfType<GuardHealth_Ikmal>();
            foreach (GuardHealth_Ikmal guard in allGuards)
            {
                // Skip self
                if (guard.gameObject == this.gameObject) continue;

                Collider2D[] otherGuardColliders = guard.GetComponents<Collider2D>();
                foreach (Collider2D otherCol in otherGuardColliders)
                {
                    Physics2D.IgnoreCollision(guardCollider, otherCol, true);
                }
            }
        }
    }

    void Update()
    {
        if (myHealth != null && myHealth.isDead)
        {
            StopMoving();
            return;
        }

        // 1. Check if current target is still valid (Alive)
        if (currentTarget != null)
        {
            if (currentTarget.gameObject == null) // Destroyed?
            {
                currentTarget = null;
            }
            else
            {
                EnemyHealth_Ikmal eh = currentTarget.GetComponent<EnemyHealth_Ikmal>();
                if (eh == null || eh.isDead) // Dead?
                {
                    currentTarget = null;
                }
            }
        }

        FindTarget();

        if (currentTarget == null)
        {
            // CHANGED: Instead of stopping immediately, go home
            ReturnToSpawn();
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
            // If target is out of detection range, stop chasing and go home
            currentTarget = null;
            ReturnToSpawn();
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
                EnemyHealth_Ikmal eh = enemy.GetComponent<EnemyHealth_Ikmal>();
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

    // ADDED: Logic to return to start position
    void ReturnToSpawn()
    {
        float distance = Vector2.Distance(transform.position, spawnPosition);

        // If we are far from home (threshold 0.5f), walk back
        if (distance > 0.5f)
        {
            float direction = Mathf.Sign(spawnPosition.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            if (direction > 0 && !isFacingRight) Flip();
            else if (direction < 0 && isFacingRight) Flip();

            if (guardAnimator != null) guardAnimator.SetBool("IsRunning", true);

            CheckJump(direction);
        }
        else
        {
            // We are home
            StopMoving();
        }
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
        EnemyHealth_Ikmal eh = currentTarget.GetComponent<EnemyHealth_Ikmal>();
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