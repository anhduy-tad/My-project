using UnityEngine;

public class Boss2D : MonoBehaviour
{
    [Header("Máu Boss")]
    public float maxHealth = 300f;
    public float currentHealth;

    [Header("Mục tiêu & Tốc độ")]
    public Transform player;          // Để trống (None), Code sẽ tự tìm!
    public float moveSpeed = 2.5f;

    [Header("Tấn công & Phạm vi")]
    public float detectRange = 8f;
    public float attackRange = 1.5f;
    public float attackRate = 1.0f;
    public float attackDamage = 20f;
    private float nextAttackTime = 0f;

    [Header("Phần thưởng & Vật phẩm rơi")]
    public int scoreReward = 10;
    public GameObject healItemPrefab;
    [Range(0f, 1f)] public float dropChance = 1.0f;

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

        // Cấu hình Rigidbody2D y hệt Quái Nhỏ
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Tắt trọng lực để không rớt map
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Đảm bảo Z = 0
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        FindPlayer();
    }

    void Update()
    {
        if (isDead) return;

        // 1. Chưa có Player -> Tự tìm lại
        if (player == null)
        {
            FindPlayer();
            SetMovingAnimation(false);
            return;
        }

        // Tính khoảng cách 2D tới Player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Lật mặt Boss theo Player
        FlipTowardsPlayer();

        // 2. Vào tầm đánh -> Đứng lại tấn công
        if (distanceToPlayer <= attackRange)
        {
            SetMovingAnimation(false);

            if (Time.time >= nextAttackTime)
            {
                AttackPlayer();
                nextAttackTime = Time.time + attackRate;
            }
        }
        // 3. Vào tầm phát hiện -> Đuổi theo (chạy animation đi bộ)
        else if (distanceToPlayer <= detectRange)
        {
            SetMovingAnimation(true);
            MoveTowardsPlayer();
        }
        // 4. Ngoại tầm -> Đứng yên
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
        // Di chuyển đơn giản giống quái nhỏ
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
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
        // Khớp biến 'isMoving' trong Animator
        if (anim != null) anim.SetBool("isMoving", isMoving);
    }

    void AttackPlayer()
    {
        // Khớp Trigger 'danh' trong Animator
        if (anim != null) anim.SetTrigger("danh");

        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.TakeDamage(attackDamage);
        }
    }

    // ==========================================
    // NHẬN SÁT THƯƠNG TỪ PLAYER
    // ==========================================
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"⚔️ [BOSS] Nhận {damageAmount} sát thương! Máu còn: {currentHealth}/{maxHealth}");

        // Nháy đỏ
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
    // XỬ LÝ BOSS CHẾT & RỚT ĐỒ
    // ==========================================
    void Die()
    {
        if (isDead) return;
        isDead = true;

        SetMovingAnimation(false);

        // Khớp Trigger 'die' trong Animator
        if (anim != null) anim.SetTrigger("die");

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

        this.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}