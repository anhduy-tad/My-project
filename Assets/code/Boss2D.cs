using System.Collections;
using UnityEngine;

public enum BossState { Idle, Chase, Telegraph, Attacking, Cooldown }

public class Boss2D : MonoBehaviour
{
    [Header("--- THÔNG SỐ BOSS ---")]
    public float maxHealth = 1000f;
    public float currentHealth;
    public float moveSpeed = 3.5f;
    public bool isPhaseTwo = false;

    [Header("--- PHẠM VI & HỒI CHIÊU ---")]
    public float detectRange = 10f;
    public float attackRange = 1.8f;
    public float attackCooldown = 2f;

    [Header("--- SÁT THƯƠNG & CHÉM ---")]
    public Transform attackPoint;
    public float attackRadius = 1.2f;
    public float damagePhaseOne = 20f;
    public float damagePhaseTwo = 35f;
    public float telegraphTime = 0.5f;
    public LayerMask playerLayer;

    [Header("--- THAM CHIẾU ---")]
    public Transform player;

    private BossState currentState = BossState.Idle;
    private float cooldownTimer;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        if (player == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (attackPoint == null) attackPoint = transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Chỉ lật mặt khi KHÔNG trong trạng thái đang gồng/chém
        if (currentState != BossState.Telegraph && currentState != BossState.Attacking)
        {
            FlipTowardPlayer();
        }

        // --- STATE MACHINE ---
        switch (currentState)
        {
            case BossState.Idle:
                if (distanceToPlayer <= detectRange)
                {
                    currentState = BossState.Chase;
                }
                break;

            case BossState.Chase:
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

                if (distanceToPlayer <= attackRange)
                {
                    StartCoroutine(AttackRoutine());
                }
                break;

            case BossState.Cooldown:
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0)
                {
                    currentState = BossState.Chase;
                }
                break;
        }
    }

    private IEnumerator AttackRoutine()
    {
        currentState = BossState.Telegraph;

        // Cảnh báo đòn đánh (Màu vàng)
        if (!isFlashing) spriteRenderer.color = Color.yellow;

        yield return new WaitForSeconds(telegraphTime);

        // Thực hiện đòn chém
        currentState = BossState.Attacking;
        if (!isFlashing) spriteRenderer.color = isPhaseTwo ? Color.red : originalColor;

        PerformMeleeSlash();

        currentState = BossState.Cooldown;
        cooldownTimer = attackCooldown;
    }

    private void PerformMeleeSlash()
    {
        float damageToDeal = isPhaseTwo ? damagePhaseTwo : damagePhaseOne;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D hit in hitPlayers)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log($"Boss chém trúng Player! Gây {damageToDeal} damage.");

                // Mở comment dòng dưới và đổi "Player" thành tên script máu của Player nếu có:
                // hit.GetComponent<Player>()?.TakeDamage(damageToDeal);
            }
        }
    }

    private void FlipTowardPlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (player.position.x < transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
    }

    // ==========================================
    // --- HỆ THỐNG NHẬN DAMAGE TỪ PLAYER ---
    // ==========================================
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Boss mất {damage} HP! Còn lại: {currentHealth}/{maxHealth}");

        // Chớp đỏ báo hiệu dính đòn
        StartCoroutine(FlashDamageEffect());

        // Kiểm tra Chuyển Phase 2 khi HP <= 50%
        if (currentHealth <= maxHealth * 0.5f && !isPhaseTwo)
        {
            EnterPhaseTwo();
        }

        // Kiểm tra Chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamageEffect()
    {
        isFlashing = true;

        // Nháy màu đỏ trong 0.15s
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);

        spriteRenderer.color = isPhaseTwo ? Color.red : originalColor;
        isFlashing = false;
    }

    private void EnterPhaseTwo()
    {
        isPhaseTwo = true;
        moveSpeed *= 1.3f;
        attackCooldown *= 0.6f;
        telegraphTime *= 0.7f;
        originalColor = Color.red; // Đổi màu gốc sang Đỏ ở Phase 2
        spriteRenderer.color = Color.red;
    }

    private void Die()
    {
        Debug.Log("Boss đã bị tiêu diệt!");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}