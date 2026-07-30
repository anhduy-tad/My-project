using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 move;
    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Giúp nhân vật không bị xoay tròn khi va chạm với vật thể khác
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        // Nhận input mượt hơn bằng GetAxisRaw (hỗ trợ cả W,A,S,D và phím mũi tên)
        move.x = Input.GetAxisRaw("Horizontal");
        move.y = Input.GetAxisRaw("Vertical");

        // Chuẩn hóa vector để di chuyển chéo không bị nhanh hơn
        move = move.normalized;

        // Quay mặt trái / phải
        if (sr != null)
        {
            if (move.x > 0.01f)
                sr.flipX = false;   // Nhìn phải
            else if (move.x < -0.01f)
                sr.flipX = true;    // Nhìn trái
        }

        // Cập nhật Animator
        if (anim != null)
        {
            anim.SetFloat("Horizontal", move.x);
            anim.SetFloat("Vertical", move.y);
            anim.SetFloat("Speed", move.sqrMagnitude);

            // Đánh bằng phím J
            if (Input.GetKeyDown(KeyCode.J))
            {
                anim.SetTrigger("Attack");
            }
        }
    }

    void FixedUpdate()
    {
        // Gán trực tiếp velocity giúp nhân vật lướt mượt và trượt cạnh tường chuẩn hơn
        rb.velocity = move * moveSpeed;
    }
}