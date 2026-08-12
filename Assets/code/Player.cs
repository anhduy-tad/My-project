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
    public HealthBar healthBar;

    [Header("Combat Settings")]
    public Transform attackPoint;
    public float attackRange = 0.8f;
    public LayerMask enemyLayers;
    public float attackDamage = 20f;
    public float attackRate = 2f;
    private float nextAttackTime = 0f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    [Header("Score Settings")]
    public int score = 0;

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

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        // Nếu đã chết -> Dừng mọi input di chuyển và đánh đấm
        if (isDead) return;

        // 1. Nhận input di chuyển
        if (!isKnockedBack)
        {
            move.x = Input.GetAxisRaw("Horizontal");
            move.y = Input.GetAxisRaw("Vertical");
            move = move.normalized;
        }

        // 2. Lật Sprite & Xoay AttackPoint
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

        // 4. Đánh bằng phím J
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
        // Khi chết -> Ép vận tốc về 0 để dừng ngay lập tức
        if (isDead)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        if (!isKnockedBack && rb != null)
        {
            rb.velocity = move * moveSpeed;
        }
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");

        if (attackPoint == null)
        {
            Debug.LogWarning("⚠️ Chưa kéo AttackPoint vào Player Inspector!");
            return;
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
            }

            Boss2D boss = enemy.GetComponent<Boss2D>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(float damageAmount, Vector2 attackerPosition)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        // Kiểm tra máu nếu hết thì Die, ngược lại thì chạy anim Hurt
        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            if (anim != null) anim.SetTrigger("Hurt");

            if (rb != null && attackerPosition != Vector2.zero)
            {
                Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
                StartCoroutine(ApplyKnockback(knockbackDirection));
            }
        }
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = direction * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        if (!isDead)
        {
            rb.velocity = Vector2.zero;
        }
        isKnockedBack = false;
    }

    // ==========================================
    // XỬ LÝ KHI PLAYER CHẾT (DIE ANIMATION)
    // ==========================================
    private void Die()
    {
        if (isDead) return; // Tránh gọi Die trùng lặp nhiều lần

        isDead = true;
        Debug.Log("☠️ Player đã chết!");

        // 1. Dừng chuyển động vật lý
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // Chuyển sang Kinematic để quái/vật phẩm không xô đẩy xác Player
        }

        // 2. Kích hoạt Trigger "Die" trong Animator
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 3. Tắt Collider để quái không đánh tiếp và không va chạm với bản đồ
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }

    public void AddScore(int amount)
    {
        if (isDead) return;

        score += amount;
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(amount);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        Item item = collision.GetComponent<Item>();
        if (item != null)
        {
            item.Collect(this);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        Item item = collision.gameObject.GetComponent<Item>();
        if (item != null)
        {
            item.Collect(this);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}