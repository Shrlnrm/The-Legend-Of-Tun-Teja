using UnityEngine;

public class PlayerCombat_Shahrul : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint;

    [Header("Combat Settings")]
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public float attackRate = 2f;

    [Header("Defense Settings")]
    public bool isBlocking = false;
    public bool isDodging = false;
    public float dodgeSpeed = 15f;
    public float dodgeDuration = 0.5f;
    public float dodgeCooldown = 1f;
    public KeyCode dodgeKey = KeyCode.LeftShift;

    // --- New Stamina Logic Variables ---
    public int consecutiveDodges = 0;
    public float lastDodgeEndTime;
    public float dodgeResetTime = 2f;
    public float blockDuration = 0f;
    public float blockStaminaDrainStart = 3f;
    public float blockStaminaDrainRate = 10f;
    public float dodgeStaminaCost = 20f;

    private float lastDodgeTime;
    private float dodgeStartTime;
    private Rigidbody2D rb;
    private Vector2 dodgeDirection;
    private SpriteRenderer sr;

    float nextAttackTime = 0f;
    private PlayerStamina_Shahrul stamina;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (animator == null) animator = GetComponent<Animator>();
        stamina = GetComponent<PlayerStamina_Shahrul>();
    }

    void Update()
    {
        // --- DODGE LOGIC ---
        if (Time.time > lastDodgeEndTime + dodgeResetTime) consecutiveDodges = 0;

        if (Input.GetKeyDown(dodgeKey) && Time.time > lastDodgeTime + dodgeCooldown && !isDodging && !isBlocking)
        {
            if (consecutiveDodges >= 2)
            {
                if (stamina != null && stamina.currentStamina < dodgeStaminaCost)
                {
                    Debug.Log("Not enough stamina to dodge!");
                    return;
                }

                if (stamina != null) stamina.DrainStamina(dodgeStaminaCost);
            }
            StartDodge();
        }

        if (isDodging)
        {
            if (Time.time > dodgeStartTime + dodgeDuration) EndDodge();
            return;
        }

        // --- BLOCK LOGIC ---
        bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f;
        if (Input.GetMouseButton(1) && !isMoving)
        {
            isBlocking = true;
            if (animator != null) animator.SetBool("IsBlocking", true);
            blockDuration += Time.deltaTime;

            if (blockDuration > blockStaminaDrainStart)
            {
                if (stamina != null)
                {
                    stamina.DrainStamina(blockStaminaDrainRate * Time.deltaTime);
                    if (stamina.currentStamina <= 0)
                    {
                        isBlocking = false;
                        if (animator != null) animator.SetBool("IsBlocking", false);
                        blockDuration = 0f;
                    }
                }
            }
        }
        else
        {
            isBlocking = false;
            if (animator != null) animator.SetBool("IsBlocking", false);
            blockDuration = 0f;
        }

        // --- ATTACK LOGIC ---
        if (Time.time >= nextAttackTime && !isBlocking && !isDodging)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDodging) rb.linearVelocity = dodgeDirection * dodgeSpeed;
    }

    void StartDodge()
    {
        isDodging = true;
        lastDodgeTime = Time.time;
        dodgeStartTime = Time.time;
        consecutiveDodges++;

        // FIX: Use SpriteRenderer.flipX to determine facing direction
        // flipX false = Facing Right (1) -> Dodge Left (-1)
        // flipX true  = Facing Left (-1) -> Dodge Right (1)

        float facingDir = 1f; // Default Right
        if (sr != null && sr.flipX)
        {
            facingDir = -1f; // Facing Left
        }

        // Dodge Backward means OPPOSITE to facing direction
        dodgeDirection = new Vector2(-facingDir, 0);

        // PLAY DODGE SOUND
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayDodgeSound();
        }

        if (animator != null)
        {
            animator.SetTrigger("Dodge");
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
        }

        Debug.Log($"Dodge Started. Facing: {facingDir}, Moving: {dodgeDirection}");
    }

    void EndDodge()
    {
        isDodging = false;
        lastDodgeEndTime = Time.time;
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    void Attack()
    {
        if (stamina != null) stamina.OnAttack();

        if (animator != null) animator.SetTrigger("Attack");

        int currentDamage = attackDamage;

        if (stamina != null && !stamina.hasUnlimitedStamina)
        {
            float staminaRatio = stamina.currentStamina / stamina.maxStamina;
            currentDamage = Mathf.RoundToInt(attackDamage * staminaRatio);
        }
        else if (stamina != null && stamina.hasUnlimitedStamina)
        {
            currentDamage = attackDamage;
        }

        if (attackPoint == null) return;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.gameObject == gameObject) continue;

            EnemyHealth_Shahrul eHealth = enemy.GetComponent<EnemyHealth_Shahrul>();
            if (eHealth == null) eHealth = enemy.GetComponentInParent<EnemyHealth_Shahrul>();

            if (eHealth != null)
            {
                eHealth.TakeDamage(currentDamage);
                continue;
            }

            GuardHealth_Shahrul gHealth = enemy.GetComponent<GuardHealth_Shahrul>();
            if (gHealth == null) gHealth = enemy.GetComponentInParent<GuardHealth_Shahrul>();

            if (gHealth != null) gHealth.TakeDamage(currentDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}