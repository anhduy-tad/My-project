using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float detectionRange = 15f;

    [Header("Health & Combat Settings")]
    public float maxHealth = 50f;
    public float currentHealth;
    public float damage = 10f;             // Sát thương gây ra cho Player
    public float attackCooldown = 1f;      // Thời gian giãn cách giữa mỗi lần gây sát thương
    private float lastAttackTime;
    private bool isDead = false;

    [Header("Enemy Knockback Settings")]
    public float knockbackForce = 4f;       // Lực nẩy lùi khi Enemy bị Player chém
    public float knockbackDuration = 0.15f; // Thời gian khựng lại
    private bool isKnockedBack = false;

    [Header("Target & References")]
    [Tooltip("Kéo đối tượng muốn đuổi theo vào đây (Nếu để trống, sẽ tự tìm object có Tag là 'Player')")]
    public Transform target;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Tự tìm Player bằng Tag nếu ô target đang để trống (None)
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
                Debug.Log("🟢 Đã tìm thấy Target (Player) thành công!");
            }
            else
            {
                Debug.LogError("🔴 KHÔNG tìm thấy Target! Hãy kéo Target vào Inspector hoặc gắn Tag 'Player' cho Player.");
            }
        }
    }

    void Update()
    {
        if (isDead || target == null) return;

        // Tính hướng di chuyển nếu không trong trạng thái bị đẩy lùi
        if (!isKnockedBack)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if (distanceToTarget <= detectionRange)
            {
                moveDirection = (target.position - transform.position).normalized;
            }
            else
            {
                moveDirection = Vector2.zero;
            }
        }

        // Lật sprite theo hướng di chuyển
        if (sr != null && Mathf.Abs(moveDirection.x) > 0.01f)
        {
            sr.flipX = moveDirection.x < 0;
        }

        // Cập nhật Animator
        if (anim != null)
        {
            anim.SetFloat("Horizontal", moveDirection.x);
            anim.SetFloat("Vertical", moveDirection.y);
            anim.SetFloat("Speed", moveDirection.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (rb == null)
        {
            Debug.LogError("🔴 Enemy chưa có Rigidbody2D! Hãy thêm Rigidbody2D vào Enemy.");
            return;
        }

        // Đẩy vật lý di chuyển (chỉ di chuyển khi không bị khựng)
        if (!isKnockedBack)
        {
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // ==========================================
    // CHẠM VÀO PLAYER -> TRỪ MÁU PLAYER
    // ==========================================
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    // Truyền vị trí Enemy sang để Player bị bật lùi
                    player.TakeDamage(damage, transform.position);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    // ==========================================
    // MÁU & SÁT THƯƠNG CỦA ENEMY
    // ==========================================
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"Enemy bị chém! Máu còn lại: {currentHealth}/{maxHealth}");

        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        // Tạo hiệu ứng bị đẩy lùi nhẹ khi trúng đòn
        if (target != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)target.position).normalized;
            StartCoroutine(ApplyKnockback(knockbackDir));
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = direction * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Enemy đã bị tiêu diệt!");

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // Tắt va chạm và dừng di chuyển
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Xóa Enemy khỏi Scene sau 1.5 giây
        Destroy(gameObject, 1.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}