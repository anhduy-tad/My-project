using System.Collections;
using UnityEngine;

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
    private float nextAttackTime = 0f;

    [Header("--- SÁT THƯƠNG & CHẾM ---")]
    public Transform attackPoint;
    public float attackRadius = 1.2f;
    public float damagePhaseOne = 20f;
    public float damagePhaseTwo = 35f;
    public float telegraphTime = 0.5f;
    public LayerMask playerLayer;

    [Header("--- THAM CHIẾU ---")]
    public Transform playar; // Giữ nguyên tên 'playar' để khớp với Inspector của bạn

    private Rigidbody2D rb;
    private Animator anim;
    private bool isAttacking = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 1. Khởi tạo máu ban đầu (tránh lỗi máu = 0 khiến Boss đứng im)
        currentHealth = maxHealth;

        // 2. Tự tìm Player bằng Tag nếu chưa gán trong Inspector
        if (playar == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) playar = pObj.transform;
        }
    }

    void Update()
    {
        if (isDead || playar == null || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playar.position);

        // Kích hoạt Phase 2 khi máu Boss giảm xuống dưới 50%
        if (currentHealth <= maxHealth * 0.5f && !isPhaseTwo)
        {
            isPhaseTwo = true;
            moveSpeed *= 1.25f; // Tăng tốc độ chạy ở Phase 2
            Debug.Log("🔥 BOSS NỔI GIẬN: CHUYỂN SANG PHASE 2!");
        }

        // Lật hướng nhìn của Boss theo vị trí Player
        FlipTowardsPlayer();

        // XL1: Trong tầm đánh -> Tấn công
        if (distanceToPlayer <= attackRange)
        {
            SetMovingAnimation(false);

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformAttack());
                nextAttackTime = Time.time + attackCooldown;
            }
        }
        // XL2: Trong tầm phát hiện -> Di chuyển đuổi theo
        else if (distanceToPlayer <= detectRange)
        {
            SetMovingAnimation(true);
            transform.position = Vector2.MoveTowards(transform.position, playar.position, moveSpeed * Time.deltaTime);
        }
        // XL3: Ngoài tầm phát hiện -> Đứng yên
        else
        {
            SetMovingAnimation(false);
        }
    }

    private void FlipTowardsPlayer()
    {
        if (playar.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (playar.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void SetMovingAnimation(bool isMoving)
    {
        if (anim != null)
        {
            anim.SetBool("isMoving", isMoving);
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        SetMovingAnimation(false);

        // Kích hoạt Animation chém
        if (anim != null)
        {
            anim.SetTrigger("danh");
        }

        // Chờ thời gian báo đòn (Telegraph)
        yield return new WaitForSeconds(telegraphTime);

        // Tính lượng sát thương theo Phase
        float damageToDeal = isPhaseTwo ? damagePhaseTwo : damagePhaseOne;

        // Vị trí chém: Nếu có AttackPoint thì lấy theo AttackPoint, ngược lại lấy vị trí Boss
        Vector3 checkPos = (attackPoint != null) ? attackPoint.position : transform.position;

        // Quét tìm xem Player có đứng trong tầm chém không
        Collider2D hitPlayer = Physics2D.OverlapCircle(checkPos, attackRadius, playerLayer);

        if (hitPlayer != null)
        {
            Player pScript = hitPlayer.GetComponent<Player>();
            if (pScript != null)
            {
                pScript.TakeDamage(damageToDeal, transform.position);
                Debug.Log($"⚔️ BOSS vung đòn! Player mất {damageToDeal} máu!");
            }
        }

        // Chờ đòn đánh hoàn tất
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"💥 Boss trúng đòn! Máu còn: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        SetMovingAnimation(false);

        if (anim != null)
        {
            anim.SetTrigger("die");
        }

        Debug.Log("☠️ BOSS ĐÃ BỊ TIÊU DIỆT!");

        // Vô hiệu hóa va chạm và Script khi Boss chết
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ vòng tròn phát hiện (màu vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Vẽ vòng tròn tầm đánh (màu đỏ)
        Vector3 checkPos = (attackPoint != null) ? attackPoint.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(checkPos, attackRadius);
    }
}