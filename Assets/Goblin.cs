using UnityEngine;

public class Goblin : MonoBehaviour
{
    [Header("Máu Quái")]
    public float maxHealth = 50f;
    public float currentHealth;

    [Header("Mục tiêu & Tốc độ")]
    public Transform player;          // Để trống (None), Code sẽ tự tìm!
    public float moveSpeed = 2.5f;

    [Header("Tấn công & Phạm vi")]
    public float detectRange = 6f;
    public float attackRange = 1.2f;
    public float attackRate = 1.2f;   // Thời gian giữa 2 lần cắn (giây)
    public float attackDamage = 5f;
    private float nextAttackTime = 0f;

    [Header("Phần thưởng & Vật phẩm rơi")]
    public int scoreReward = 2;
    public GameObject healItemPrefab; // Kéo Prefab bình máu vào đây (nếu có)
    [Range(0f, 1f)] public float dropChance = 0.5f; // Tỉ lệ rớt item (50%)

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

        // Cấu hình Rigidbody2D tự động để tránh xoay té ngửa
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Tự động tìm Player
        FindPlayer();
    }

    void Update()
    {
        if (isDead) return;

        if (player == null)
        {
            FindPlayer();
            SetMovingAnimation(false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Lật mặt quái theo hướng Player
        FlipTowardsPlayer();

        // 1. Vào tầm đánh -> Tấn công
        if (distanceToPlayer <= attackRange)
        {
            SetMovingAnimation(false);

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
            }
        }
        // 2. Vào tầm phát hiện -> Đuổi theo Player
        else if (distanceToPlayer <= detectRange)
        {
            SetMovingAnimation(true);
            MoveTowardsPlayer();
        }
        // 3. Quá xa -> Đứng yên
        else
        {
            SetMovingAnimation(false);
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
        if (rb == null || player == null) return;

        // Di chuyển bằng vật lý Rigidbody2D mượt mà
        Vector2 direction = (player.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
    }

    void FlipTowardsPlayer()
    {
        if (player == null) return;

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

    void SetMovingAnimation(bool isMoving)
    {
        if (anim != null) anim.SetBool("isMoving", isMoving);
    }

    void AttackPlayer()
    {
        if (anim != null) anim.SetTrigger("danh");

        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(attackDamage, transform.position);
        }
    }

    // ==========================================
    // NHẬN SÁT THƯƠNG TỪ PLAYER
    // ==========================================
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"⚔️ [{gameObject.name}] Máu còn: {currentHealth}/{maxHealth}");

        // Nhấp nháy đỏ khi bị trúng đòn
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

        SetMovingAnimation(false);

        if (anim != null) anim.SetTrigger("die");

        // Tắt Collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Cộng điểm
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(scoreReward);
        }

        // Rớt Item hồi máu theo tỷ lệ
        if (healItemPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        }

        // Tắt script và xóa xác sau 1.2s
        this.enabled = false;
        Destroy(gameObject, 1.2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}