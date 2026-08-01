using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Combat Settings")]
    public Transform attackPoint;      // Điểm xuất phát của đòn đánh (kéo 1 GameObject con vào)
    public float attackRange = 0.5f;   // Bán kính vùng chém
    public LayerMask enemyLayers;      // Chọn Layer 'Enemy' để chém trúng quái
    public float attackDamage = 20f;   // Sát thương Player gây ra
    public float attackRate = 2f;      // Số lần chém tối đa trong 1 giây
    private float nextAttackTime = 0f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;   // Lực đẩy lùi khi bị Enemy đánh
    public float knockbackDuration = 0.2f; // Thời gian bị khựng do đẩy lùi
    private bool isKnockedBack = false;

    private Rigidbody2D rb;
    private Vector2 move;
    private Animator anim;
    private SpriteRenderer sr;

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
    }

    void Update()
    {
        if (isDead) return;

        // 1. Nhận input di chuyển (chỉ nhận khi không bị Knockback)
        if (!isKnockedBack)
        {
            move.x = Input.GetAxisRaw("Horizontal");
            move.y = Input.GetAxisRaw("Vertical");
            move = move.normalized;
        }

        // 2. Lật Sprite trái / phải & xoay điểm AttackPoint theo hướng nhìn
        if (sr != null)
        {
            if (move.x > 0.01f)
            {
                sr.flipX = false;
                if (attackPoint != null)
                    attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0);
            }
            else if (move.x < -0.01f)
            {
                sr.flipX = true;
                if (attackPoint != null)
                    attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0);
            }
        }

        // 3. Cập nhật Animator
        if (anim != null)
        {
            anim.SetFloat("Horizontal", move.x);
            anim.SetFloat("Vertical", move.y);
            anim.SetFloat("Speed", move.sqrMagnitude);
        }

        // 4. Xử lý Đánh bằng phím J (Đưa ra ngoài độc lập với Animator)
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Không ghi đè velocity nếu đang trong thời gian bị bật lùi (Knockback)
        if (!isKnockedBack)
        {
            rb.velocity = move * moveSpeed;
        }
    }

    // ==========================================
    // CƠ CHẾ TẤN CÔNG (CHÉM ENEMY)
    // ==========================================
    void Attack()
    {
        // Trigger animation nếu có
        if (anim != null) anim.SetTrigger("Attack");

        if (attackPoint == null)
        {
            Debug.LogWarning("⚠️ Chưa kéo AttackPoint vào Player Inspector!");
            return;
        }

        // Quét tất cả Enemy trong vùng chém
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Gây sát thương cho từng Enemy trúng đòn
        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
            }
        }
    }

    // ==========================================
    // CƠ CHẾ NHẬN SÁT THƯƠNG & BỊ ĐẨY LÙI
    // ==========================================
    public void TakeDamage(float damageAmount, Vector2 attackerPosition)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"💥 Player bị đánh! Mất {damageAmount} máu. Máu còn lại: {currentHealth}/{maxHealth}");

        if (anim != null) anim.SetTrigger("Hurt");

        // Xử lý bật lùi (Knockback) ra xa khỏi Enemy
        if (rb != null)
        {
            Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
            StartCoroutine(ApplyKnockback(knockbackDirection));
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

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("☠️ Player đã chết!");

        if (anim != null) anim.SetTrigger("Die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    // Vẽ vùng chém trong cửa sổ Scene để dễ căn chỉnh
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}