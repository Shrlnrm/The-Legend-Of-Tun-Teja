using UnityEngine;

public class PlayerCombat_Irfan : MonoBehaviour
{
    public Animator animator;
    public Transform attackPoint;

    [Header("Combat Settings")]
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public float attackRate = 2f;
    public LayerMask enemyLayers;

    [Header("Defense Settings")]
    public bool isBlocking = false;
    public bool isDodging = false;
    public float dodgeSpeed = 15f;
    public float dodgeDuration = 0.5f;
    public float dodgeCooldown = 1f;
    public KeyCode dodgeKey = KeyCode.LeftShift;

    [Header("Taming Sari Settings")]
    public bool hasTamingSari = false;
    public int tamingSariDamage = 50;
    public GameObject tamingSariVisualEffect;

    // --- Stamina Variables ---
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

    private float nextAttackTime = 0f;
    private PlayerStamina_Irfan stamina;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        stamina = GetComponent<PlayerStamina_Irfan>();
    }

    void Update()
    {
        if (Time.time > lastDodgeEndTime + dodgeResetTime) consecutiveDodges = 0;

        if (Input.GetKeyDown(dodgeKey) && Time.time > lastDodgeTime + dodgeCooldown && !isDodging && !isBlocking)
        {
            if (consecutiveDodges >= 2)
            {
                if (stamina != null && stamina.currentStamina < dodgeStaminaCost) return;
                if (stamina != null) stamina.SendMessage("DrainStamina", dodgeStaminaCost);
            }
            StartDodge();
        }

        if (isDodging)
        {
            if (Time.time > dodgeStartTime + dodgeDuration) EndDodge();
            return;
        }

        bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f;

        if (Input.GetMouseButton(1) && !isMoving)
        {
            isBlocking = true;
            if (animator != null) animator.SetBool("IsBlocking", true);
            blockDuration += Time.deltaTime;

            if (blockDuration > blockStaminaDrainStart && stamina != null)
            {
                stamina.SendMessage("DrainStamina", blockStaminaDrainRate * Time.deltaTime);
                if (stamina.currentStamina <= 0)
                {
                    isBlocking = false;
                    if (animator != null) animator.SetBool("IsBlocking", false);
                    blockDuration = 0f;
                }
            }
        }
        else
        {
            isBlocking = false;
            if (animator != null) animator.SetBool("IsBlocking", false);
            blockDuration = 0f;
        }

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

        float facingDirection = transform.localScale.x > 0 ? 1 : -1;
        dodgeDirection = new Vector2(-facingDirection, 0);

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
    }

    void EndDodge()
    {
        isDodging = false;
        lastDodgeEndTime = Time.time;
        rb.linearVelocity = Vector2.zero;
    }

    void Attack()
    {
        if (stamina != null) stamina.OnAttack();

        if (animator != null)
        {
            animator.SetBool("HasTamingSari", hasTamingSari);
            animator.SetTrigger("Attack");
        }

        int currentBaseDamage = hasTamingSari ? tamingSariDamage : attackDamage;
        int finalDamage = currentBaseDamage;

        if (stamina != null && !stamina.hasUnlimitedStamina)
        {
            float staminaRatio = stamina.currentStamina / stamina.maxStamina;
            finalDamage = Mathf.RoundToInt(currentBaseDamage * staminaRatio);
            if (finalDamage < 1) finalDamage = 1;
        }

        if (attackPoint == null) return;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth_Irfan eHealth = enemy.GetComponent<EnemyHealth_Irfan>();
            if (eHealth != null) { eHealth.TakeDamage(finalDamage); continue; }

            GuardHealth_Irfan gHealth = enemy.GetComponent<GuardHealth_Irfan>();
            if (gHealth != null) { gHealth.TakeDamage(finalDamage); }
        }
    }

    public void EquipTamingSari()
    {
        hasTamingSari = true;
        if (animator != null) animator.SetBool("HasTamingSari", true);
        if (tamingSariVisualEffect != null) Instantiate(tamingSariVisualEffect, transform.position, Quaternion.identity, transform);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}