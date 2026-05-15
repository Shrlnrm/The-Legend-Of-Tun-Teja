using UnityEngine;

public class GuardController_Ikmal : MonoBehaviour
{
    [Header("Guard Animation")]
    public Animator guardAnimator;

    [Header("Settings")]
    public bool isTutorial = false;
    public float moveSpeed = 3f;
    public float detectionRange = 7f;
    public float attackRange = 1f;
    public int damage = 15;
    public float attackCooldown = 1.2f;

    [Header("State")]
    public bool hasSurrendered = false;

    private Transform currentTarget;
    private float lastAttackTime;
    private GuardHealth_Ikmal myHealth;

    void Start()
    {
        myHealth = GetComponent<GuardHealth_Ikmal>();
    }

    void Update()
    {
        if ((myHealth != null && myHealth.isDead) || hasSurrendered) return;

        FindTarget();
        if (currentTarget == null)
        {
            if (guardAnimator != null) guardAnimator.SetBool("IsRunning", false);
            return;
        }

        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            if (Time.time > lastAttackTime + attackCooldown) Attack();
        }
        else if (distance <= detectionRange)
        {
            ChaseTarget();
        }
    }

    void FindTarget()
    {
        if (currentTarget != null) return;
        if (isTutorial)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) currentTarget = p.transform;
        }
        else
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            float closestDist = Mathf.Infinity;
            foreach (GameObject enemy in enemies)
            {
                float d = Vector2.Distance(transform.position, enemy.transform.position);
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
    }

    void ChaseTarget()
    {
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
        float dir = Mathf.Sign(currentTarget.position.x - transform.position.x);
        transform.localScale = new Vector3(dir > 0 ? -1 : 1, 1, 1);
        if (guardAnimator != null) guardAnimator.SetBool("IsRunning", true);
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        if (guardAnimator != null) guardAnimator.SetTrigger("Attack");

        if (isTutorial)
        {
            PlayerHealth_Ikmal ph = currentTarget.GetComponent<PlayerHealth_Ikmal>();
            if (ph != null) ph.TakeDamage(damage);
        }
        else
        {
            EnemyHealth_Ikmal eh = currentTarget.GetComponent<EnemyHealth_Ikmal>();
            if (eh != null) eh.TakeDamage(damage);
        }
    }
}