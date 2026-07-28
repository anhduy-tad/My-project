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
    }

    void Update()
    {
        move = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
            move.y = 1;

        if (Input.GetKey(KeyCode.S))
            move.y = -1;

        if (Input.GetKey(KeyCode.A))
            move.x = -1;

        if (Input.GetKey(KeyCode.D))
            move.x = 1;

        move.Normalize();

        // Quay mặt trái / phải
        if (sr != null)
        {
            if (move.x > 0.05f)
                sr.flipX = false;   // Nhìn phải (mặt gốc)

            else if (move.x < -0.05f)
                sr.flipX = true;    // Nhìn trái
        }

        if (anim != null)
        {
            anim.SetFloat("Horizontal", move.x);
            anim.SetFloat("Vertical", move.y);
            anim.SetFloat("Speed", move.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
    }
}