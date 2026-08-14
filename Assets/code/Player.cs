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

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip dieSound;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    [Header("Score Settings")]
    public int score = 0;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        currentHealth = maxHealth;
        isDead = false;

        if (healthBar == null) healthBar = FindObjectOfType<HealthBar>();

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            // Ép collision detection mượt hơn để tránh lọt/kẹt khe
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
    {
        if (isDead) return;

        // 1. Chỉ lấy Input ở Update
        if (!isKnockedBack)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput = moveInput.normalized;
        }

        // 2. Lật Sprite & Xoay AttackPoint
        if (sr != null)
        {
            if (moveInput.x > 0.01f)
            {
                sr.flipX = false;
                if (attackPoint != null)
                    attackPoint.localPosition = new Vector3(Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0);
            }
            else if (moveInput.x < -0.01f)
            {
                sr.flipX = true;
                if (attackPoint != null)
                    attackPoint.localPosition = new Vector3(-Mathf.Abs(attackPoint.localPosition.x), attackPoint.localPosition.y, 0);
            }
        }

        // 3. Cập nhật Animator
        if (anim != null)
        {
            anim.SetFloat("Horizontal", moveInput.x);
            anim.SetFloat("Vertical", moveInput.y);
            anim.SetFloat("Speed", moveInput.sqrMagnitude);
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
        if (isDead)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        // Di chuyển bằng Rigidbody2D trong FixedUpdate tránh bị trễ nhịp làm kẹt
        if (!isKnockedBack && rb != null)
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null) enemyScript.TakeDamage(attackDamage);

            Boss2D boss = enemy.GetComponent<Boss2D>();
            if (boss != null) boss.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        TakeDamage(damageAmount, Vector2.zero);
    }

    public void TakeDamage(float damageAmount, Vector2 attackerPosition)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (healthBar != null) healthBar.SetHealth(currentHealth);

        if (sr != null)
        {
            sr.color = Color.red;
            Invoke(nameof(ResetColor), 0.15f);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            if (anim != null) anim.SetTrigger("Hurt");

            if (audioSource != null && hurtSound != null) audioSource.PlayOneShot(hurtSound);

            if (rb != null && attackerPosition != Vector2.zero)
            {
                Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
                // Stop mọi Coroutine knockback cũ trước khi gọi cái mới để tránh kẹt vĩnh viễn
                StopCoroutine(nameof(ApplyKnockback));
                StartCoroutine(ApplyKnockback(knockbackDirection));
            }
        }
    }

    private void ResetColor()
    {
        if (sr != null) sr.color = Color.white;
    }

    private IEnumerator ApplyKnockback(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = direction * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);

        // Đảm bảo mở lại trạng thái di chuyển cho Player
        isKnockedBack = false;
        if (!isDead && rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        if (audioSource != null && dieSound != null) audioSource.PlayOneShot(dieSound);

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (anim != null) anim.SetTrigger("Die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Invoke(nameof(ShowGameOverUI), 1.5f);
    }

    private void ShowGameOverUI()
    {
        GameOverManager gameOverManager = FindObjectOfType<GameOverManager>();
        if (gameOverManager != null) gameOverManager.SetupGameOver();
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0f, maxHealth);
        if (healthBar != null) healthBar.SetHealth(currentHealth);
    }

    public void AddScore(int amount)
    {
        if (isDead) return;
        score += amount;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}