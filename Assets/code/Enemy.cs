using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Máu Quái")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Mục tiêu & Tốc độ")]
    public Transform player;
    public float moveSpeed = 3.5f;

    [Header("Tấn công & Phạm vi")]
    public float detectRange = 8f;
    public float attackRange = 1.2f;
    public float attackDamage = 10f;

    [Header("Cấu hình Cooldown Tấn Công")]
    public float attackCooldown = 2.0f;     // Thời gian hồi chiêu giữa 2 lần đánh (giây)
    private float cooldownTimer = 0f;       // Bộ đếm thời gian
    private bool isAttacking = false;

    [Header("Phần thưởng & Vật phẩm rơi")]
    public int scoreReward = 2;
    public GameObject healItemPrefab;
    [Range(0f, 1f)] public float dropChance = 0.5f;

    private Vector3 originalScale;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        FindPlayer();
    }

    void Update()
    {
        if (isDead) return;

        // Giảm thời gian Cooldown theo thời gian thực
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (player == null)
        {
            FindPlayer();
            if (anim != null) anim.SetBool("IsRunning", false);
            return;
        }

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.position;
        float distanceToPlayer = Vector2.Distance(enemyPos, playerPos);

        // 1. Nếu trong tầm phát hiện và chưa vào tầm đánh
        if (distanceToPlayer <= detectRange && distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
            if (anim != null) anim.SetBool("IsRunning", true);
        }
        // 2. Đã vào tầm đánh
        else if (distanceToPlayer <= attackRange)
        {
            if (anim != null) anim.SetBool("IsRunning", false);

            // Chỉ tấn công khi đã HẾT Cooldown
            if (cooldownTimer <= 0f)
            {
                AttackPlayer();
                cooldownTimer = attackCooldown; // Đặt lại bộ đếm cooldown
            }
        }
        else
        {
            if (anim != null) anim.SetBool("IsRunning", false);
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        float absX = Mathf.Abs(originalScale.x);
        float absY = Mathf.Abs(originalScale.y);
        float absZ = Mathf.Abs(originalScale.z);

        if (player.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-absX, absY, absZ);
        }
        else if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(absX, absY, absZ);
        }
    }

    void AttackPlayer()
    {
        if (anim != null) anim.SetTrigger("Attack");

        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"⚔️ [{gameObject.name}] Nhận {damageAmount} sát thương! Máu còn lại: {currentHealth}/{maxHealth}");

        if (anim != null) anim.SetTrigger("Hurt");

        if (sr != null)
        {
            Invoke(nameof(ResetColor), 0.15f);
            sr.color = Color.red;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ResetColor()
    {
        if (sr != null) sr.color = Color.white;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetTrigger("Die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(scoreReward);
        }

        if (healItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 0.8f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}