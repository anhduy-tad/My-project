using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Máu Quái")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Mục tiêu & Tốc độ")]
    public Transform player;          // Để trống (None), Code sẽ tự tìm!
    public float moveSpeed = 3.5f;

    [Header("Tấn công & Phạm vi")]
    public float detectRange = 8f;
    public float attackRange = 1.2f;
    public float attackRate = 1.0f;
    public float attackDamage = 10f;
    private float nextAttackTime = 0f;

    [Header("Phần thưởng")]
    public int scoreReward = 2;

    private Vector3 originalScale;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        originalScale = transform.localScale;

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // Cấu hình Rigidbody2D tự động để tránh bị kẹt vật lý
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // Dùng Kinematic để di chuyển mượt
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Đảm bảo Z = 0
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        // Tự động tìm Player
        FindPlayer();
    }

    void Update()
    {
        // 1. Nếu chưa có Player -> Tự động tìm lại
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Tính khoảng cách 2D tới Player
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.position;
        float distanceToPlayer = Vector2.Distance(enemyPos, playerPos);

        // 2. Nếu Player nằm trong tầm phát hiện và chưa vào tầm đánh -> Đuổi theo
        if (distanceToPlayer <= detectRange && distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        // 3. Vào tới tầm đánh -> Đứng lại tấn công
        else if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
            }
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
        // Di chuyển vị trí bằng Vector2.MoveTowards
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Lật mặt Sprite theo hướng Player
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
        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(attackDamage, transform.position);
        }
    }

    // ==========================================
    // NẬN DAMAGE TỪ PLAYER
    // ==========================================
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"⚔️ [{gameObject.name}] Nhận {damageAmount} sát thương! Máu còn lại: {currentHealth}/{maxHealth}");

        // Hiệu ứng chớp đỏ nhẹ khi bị đánh
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
        Debug.Log($"☠️ [{gameObject.name}] Đã bị tiêu diệt!");

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(scoreReward);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn tầm phát hiện (Vàng) và tầm đánh (Đỏ) trong Scene
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}