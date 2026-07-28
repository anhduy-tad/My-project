using UnityEngine;

public class quai : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectRange = 8f;   // Tầm nhìn thấy Player
    public float attackRange = 3f;   // Tầm đánh (ví dụ bắn đạn / chém xa)
    public float keepDistance = 2f;  // Khoảng cách tối thiểu quái muốn giữ với Player

    [Header("Attack")]
    public float attackCooldown = 1f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private float attackTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        attackTimer = 0f;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        attackTimer -= Time.fixedDeltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. Quá xa (ngoài tầm nhìn) -> Đứng yên
        if (distance > detectRange)
        {
            StopMoving();
            return;
        }

        // Vector hướng từ Quái tới Player
        Vector2 direction = (player.position - transform.position).normalized;

        // Quay mặt trái/phải theo vị trí Player
        if (sr != null)
        {
            if (direction.x > 0.05f)
                sr.flipX = false;
            else if (direction.x < -0.05f)
                sr.flipX = true;
        }

        // 2. Player lại QUÁ GẦN (nhỏ hơn keepDistance) -> Quái LÙI LẠI
        if (distance < keepDistance)
        {
            // Di chuyển ngược hướng (-direction)
            rb.velocity = -direction * moveSpeed;

            if (anim != null)
            {
                anim.SetFloat("Horizontal", -direction.x);
                anim.SetFloat("Vertical", -direction.y);
                anim.SetFloat("Speed", 1);
            }
        }
        // 3. Player ở QUÁ XA tầm đánh -> Quái TIẾN LẠI GẦN
        else if (distance > attackRange)
        {
            rb.velocity = direction * moveSpeed;

            if (anim != null)
            {
                anim.SetFloat("Horizontal", direction.x);
                anim.SetFloat("Vertical", direction.y);
                anim.SetFloat("Speed", 1);
            }
        }
        // 4. Ở trong vùng khoảng cách đẹp (giữa keepDistance và attackRange) -> ĐỨNG LẠI & ĐÁNH
        else
        {
            StopMoving();

            // Thực hiện đòn đánh khi hồi chiêu xong
            if (attackTimer <= 0)
            {
                attackTimer = attackCooldown;

                if (anim != null)
                    anim.SetTrigger("Attack");
            }
        }
    }

    private void StopMoving()
    {
        rb.velocity = Vector2.zero;
        if (anim != null)
            anim.SetFloat("Speed", 0);
    }

    // Vẽ vòng tròn tầm nhìn/khoảng cách trong Scene View để dễ chỉnh
    private void OnDrawGizmosSelected()
    {
        // Vòng tròn vàng: Tầm phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Vòng tròn đỏ: Tầm đánh
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Vòng tròn xanh lá: Khoảng cách tối thiểu quái muốn giữ
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, keepDistance);
    }
}