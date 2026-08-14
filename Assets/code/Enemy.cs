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

    [Header("Phần thưởng & Vật phẩm rơi")]
    public int scoreReward = 2;
    public GameObject healItemPrefab; // Kéo Prefab bình máu vào đây
    [Range(0f, 1f)] public float dropChance = 0.5f; // Tỉ lệ rớt item (0.5 = 50%)

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
        // Nếu quái đã chết -> Dừng toàn bộ logic
        if (isDead) return;

        // 1. Nếu chưa có Player -> Tự động tìm lại
        if (player == null)
        {
            FindPlayer();
            if (anim != null) anim.SetBool("IsRunning", false);
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
            if (anim != null) anim.SetBool("IsRunning", true);
        }
        // 3. Vào tới tầm đánh -> Đứng lại tấn công
        else if (distanceToPlayer <= attackRange)
        {
            if (anim != null) anim.SetBool("IsRunning", false);

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
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
        // Chạy Animation Attack
        if (anim != null) anim.SetTrigger("Attack");

        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            // Không truyền vị trí -> Player không bị đẩy lùi
            playerScript.TakeDamage(attackDamage);
        }
    }

    // ==========================================
    // NHẬN DAMAGE TỪ PLAYER
    // ==========================================
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"⚔️ [{gameObject.name}] Nhận {damageAmount} sát thương! Máu còn lại: {currentHealth}/{maxHealth}");

        // Chạy Animation Hurt
        if (anim != null) anim.SetTrigger("Hurt");

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

    // ==========================================
    // XỬ LÝ CHẾT & RỚT ITEM
    // ==========================================
    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"☠️ [{gameObject.name}] Đã bị tiêu diệt!");

        // 1. Chạy Animation Die
        if (anim != null) anim.SetTrigger("Die");

        // 2. Tắt Collider để Player không bị vướng
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 3. Cộng điểm
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(scoreReward);
        }

        // 4. Rớt Item hồi máu theo tỷ lệ
        if (healItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }

        // 5. Hủy GameObject sau 0.8s để chạy hết Animation Die
        Destroy(gameObject, 0.8f);
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