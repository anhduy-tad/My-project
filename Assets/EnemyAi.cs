using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Chỉ số Quái")]
    public float maxHealth = 50f;
    private float currentHealth;
    public float speed = 2f;
    public float attackDamage = 10f;    // Sát thương gây ra cho Player
    public float chaseRange = 6f;       // Tầm nhìn đuổi theo
    public float attackRange = 1.2f;    // Tầm vung đòn
    public float attackCooldown = 1.5f;

    private Transform playerTransform;
    private Player playerScript;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float nextAttackTime = 0f;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Tìm Player theo Tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerScript = playerObj.GetComponent<Player>();
        }
    }

    void Update()
    {
        if (isDead || playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 1. Đuổi theo Player
        if (distanceToPlayer <= chaseRange && distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        // 2. Đứng lại Đánh Player
        else if (distanceToPlayer <= attackRange)
        {
            FlipSprite(playerTransform.position.x - transform.position.x);
            SetMovingAnimation(false);

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        else
        {
            SetMovingAnimation(false);
        }
    }

    void ChasePlayer()
    {
        float directionX = playerTransform.position.x - transform.position.x;
        FlipSprite(directionX);

        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
        SetMovingAnimation(true);
    }

    void FlipSprite(float directionX)
    {
        if (spriteRenderer != null)
        {
            if (directionX > 0) spriteRenderer.flipX = false;
            else if (directionX < 0) spriteRenderer.flipX = true;
        }
    }

    void Attack()
    {
        if (anim != null) anim.SetTrigger("attack");

        // GỌI HÀM TRỪ MÁU PLAYER & TRUYỀN VỊ TRÍ ĐỂ PLAYER BỊ KNOCKBACK (ĐẨY LÙI)
        if (playerScript != null && !playerScript.isDead)
        {
            playerScript.TakeDamage(attackDamage, transform.position);
        }
    }

    // ==========================================
    // CƠ CHẾ QUÁI NHẬN SÁT THƯƠNG TỪ PLAYER
    // ==========================================
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"💥 Quái {gameObject.name} bị chém! Máu còn: {currentHealth}/{maxHealth}");

        if (anim != null) anim.SetTrigger("hurt"); // Animation bị thương (nếu có)

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        SetMovingAnimation(false);

        if (anim != null) anim.SetTrigger("die"); // Animation chết (nếu có)

        // Tắt va chạm để không cản đường Player nữa
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Xóa quái khỏi game sau 1 giây
        Destroy(gameObject, 1f);
    }

    void SetMovingAnimation(bool isMoving)
    {
        if (anim != null) anim.SetBool("isMoving", isMoving);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
