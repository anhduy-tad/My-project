using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    private Rigidbody2D rb;
    private Vector2 move;
    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Khởi tạo máu ban đầu
        currentHealth = maxHealth;

        // Giúp nhân vật không bị xoay tròn khi va chạm
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        // Nếu đã chết thì không cho di chuyển hay đánh nữa
        if (isDead) return;

        // Nhận input di chuyển
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");

        // Chuẩn hóa vector di chuyển chéo
        move = move.normalized;

        // Lật Sprite trái / phải
        if (sr != null)
        {
            if (move.x > 0.01f)
                sr.flipX = false;
            else if (move.x < -0.01f)
                sr.flipX = true;
        }

        // Cập nhật Animator
        if (anim != null)
        {
            anim.SetFloat("Horizontal", move.x);
            anim.SetFloat("Vertical", move.y);
            anim.SetFloat("Speed", move.sqrMagnitude);

            // Đánh bằng phím J
            if (Input.GetKeyDown(KeyCode.J))
            {
                anim.SetTrigger("Attack");
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero; // Dừng mọi di chuyển khi chết
            return;
        }

        rb.velocity = move * moveSpeed;
    }

    // ==========================================
    // HỆ THỐNG MÁU & SÁT THƯƠNG
    // ==========================================

    /// <summary>
    /// Gọi hàm này từ script của Quái/Bẫy khi gây sát thương cho Player
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Player mất {damageAmount} máu. Máu còn lại: {currentHealth}/{maxHealth}");

        // Trigger animation nhận sát thương (nếu có)
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        // Kiểm tra xem chết chưa
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Gọi hàm này khi Player ăn bình máu/vật phẩm hồi máu
    /// </summary>
    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Player được hồi {healAmount} máu. Máu hiện tại: {currentHealth}/{maxHealth}");
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player đã chết!");

        // Trigger animation chết (nếu trong Animator có tham số Trigger 'Die')
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // Vô hiệu hóa Collider để quái/vật thể khác không va chạm nữa
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
}