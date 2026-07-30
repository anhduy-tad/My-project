using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.5f;
    public float detectionRange = 15f;

    [Header("Target & References")]
    [Tooltip("Kéo đối tượng muốn đuổi theo vào đây (Nếu để trống, sẽ tự tìm object có Tag là 'Player')")]
    public Transform target; // Đã đổi/thêm tên chuẩn là target

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

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
        if (target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget <= detectionRange)
        {
            moveDirection = (target.position - transform.position).normalized;
        }
        else
        {
            moveDirection = Vector2.zero;
        }

        // Lật sprite
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
        if (rb == null)
        {
            Debug.LogError("🔴 Slime chưa có Rigidbody2D! Hãy thêm Rigidbody2D vào Slime.");
            return;
        }

        // Đẩy vật lý di chuyển
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    // Vẽ vòng tròn tầm phát hiện trong cửa sổ Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}