using UnityEngine;

public class Guard_Level1_Special_Shahrul : MonoBehaviour
{
    [Header("Guard Animation")]
    public Animator guardAnimator;

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float detectionRange = 7f;
    public float attackRange = 1f;
    public int damage = 15;
    public float attackCooldown = 1.2f;

    [Header("State")]
    public bool hasSurrendered = false;
    public bool isStatic = true;

    [Header("Cleanup Settings")]
    public float disappearDistance = 15f;

    [Header("Components")]
    public Collider2D guardCollider;
    public GameObject surrenderParticle;

    private Transform playerTarget;
    private float lastAttackTime;
    private GuardHealth_Shahrul myHealth;
    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private Camera mainCamera;

    void Start()
    {
        myHealth = GetComponent<GuardHealth_Shahrul>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (guardCollider == null) guardCollider = GetComponent<Collider2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTarget = p.transform;

        if (guardAnimator == null) guardAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (hasSurrendered)
        {
            StopMoving();
            CheckToDisappear();
            return;
        }

        if ((myHealth != null && myHealth.isDead) || isStatic)
        {
            StopMoving();
            return;
        }

        if (playerTarget == null) return;

        PlayerHealth_Shahrul ph = playerTarget.GetComponent<PlayerHealth_Shahrul>();
        if (ph != null && ph.currentHealth <= 0)
        {
            StopMoving();
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTarget.position);

        if (distance <= attackRange)
        {
            StopMoving();
            FaceTarget(playerTarget.position);

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

    void CheckToDisappear()
    {
        if (mainCamera == null) return;

        float distanceToCamera = Vector2.Distance(transform.position, mainCamera.transform.position);

        if (distanceToCamera > disappearDistance)
        {
            Debug.Log("Guard is out of camera range. Disappearing.");
            Destroy(gameObject);
        }
    }

    public void BeginSparring()
    {
        isStatic = false;
        Debug.Log("Guard is now sparring!");
    }

    void ChaseTarget()
    {
        float direction = Mathf.Sign(playerTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();

        if (guardAnimator != null) guardAnimator.SetBool("IsRunning", true);
    }

    void FaceTarget(Vector3 targetPos)
    {
        float direction = Mathf.Sign(targetPos.x - transform.position.x);
        if (direction > 0 && !isFacingRight) Flip();
        else if (direction < 0 && isFacingRight) Flip();
    }

    void StopMoving()
    {
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (guardAnimator != null) guardAnimator.SetBool("IsRunning", false);
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        if (guardAnimator != null) guardAnimator.SetTrigger("Attack");

        PlayerHealth_Shahrul ph = playerTarget.GetComponent<PlayerHealth_Shahrul>();
        if (ph != null) ph.TakeDamage(damage);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TriggerSurrender()
    {
        hasSurrendered = true;
        StopMoving();
        Debug.Log("Level 1 Guard Surrendered!");

        if (surrenderParticle != null)
            Instantiate(surrenderParticle, transform.position, Quaternion.identity);

        if (guardCollider != null)
            guardCollider.isTrigger = true;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        if (guardAnimator != null)
        {
            guardAnimator.SetBool("IsRunning", false);
            guardAnimator.SetBool("IsKneeling", true);
        }
    }
}