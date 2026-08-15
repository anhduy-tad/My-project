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
    public AudioSource sfxSource;      // Đổi tên để phân biệt với SFX
    public AudioSource bgmSource;      // AudioSource riêng cho Nhạc nền
    public AudioClip bgmSound;         // File Nhạc nền
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

        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();

        // Xử lý phát nhạc nền
        if (bgmSource != null && bgmSound != null)
        {
            bgmSource.clip = bgmSound;
            bgmSource.loop = true; // Lặp lại liên tục
            bgmSource.Play();
        }

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
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (!isKnockedBack)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput = moveInput.normalized;
        }

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

        if (anim != null)
        {
            anim.SetFloat("Horizontal", moveInput.x);
            anim.SetFloat("Vertical", moveInput.y);
            anim.SetFloat("Speed", moveInput.sqrMagnitude);
        }

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

        if (!isKnockedBack && rb != null)
        {
            rb.velocity = moveInput * moveSpeed;
        }
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("Attack");

        if (sfxSource != null && attackSound != null)
        {
            sfxSource.PlayOneShot(attackSound);
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

            if (sfxSource != null && hurtSound != null) sfxSource.PlayOneShot(hurtSound);

            if (rb != null && attackerPosition != Vector2.zero)
            {
                Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
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
        if (sfxSource != null && dieSound != null) sfxSource.PlayOneShot(dieSound);

        // Tùy chọn: Tắt nhạc nền khi Player chết
        if (bgmSource != null) bgmSource.Stop();

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