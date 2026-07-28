using UnityEngine;

public class quai : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectRange = 1f;
    public float attackRange = 1f;

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

        // Ngoài tầm phát hiện
        if (distance > detectRange)
        {
            if (anim != null)
                anim.SetFloat("Speed", 0);

            rb.velocity = Vector2.zero;
            return;
        }

        // Hướng tới Player
        Vector2 direction = (player.position - transform.position).normalized;

        // Quay mặt (Sprite gốc nhìn sang phải)
        if (sr != null)
        {
            if (direction.x > 0.05f)
                sr.flipX = false;
            else if (direction.x < -0.05f)
                sr.flipX = true;
        }

        // Đi theo
        if (distance > attackRange)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

            if (anim != null)
            {
                anim.SetFloat("Horizontal", direction.x);
                anim.SetFloat("Vertical", direction.y);
                anim.SetFloat("Speed", 1);
            }
        }
        else
        {
            if (anim != null)
                anim.SetFloat("Speed", 0);

            if (attackTimer <= 0)
            {
                attackTimer = attackCooldown;

                if (anim != null)
                    anim.SetTrigger("Attack");
            }
        }
    }
}