using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 5f;
    public int currentHealth = 100;

    private Animator anim;
    private Rigidbody2D rb;

    [HideInInspector] public float moveX = 0f;
    [HideInInspector] public float moveY = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveX = 0f;
        moveY = 0f;

        // Bắt phím di chuyển
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1f;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveY = 1f;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1f;
        }

        bool isMoving = (moveX != 0 || moveY != 0);

        // Gửi trạng thái di chuyển sang Animator
        anim.SetBool("IsRun", isMoving);

        // Cập nhật hướng Idle
        if (isMoving)
        {
            IdleAnimation();
        }

        // Đánh bằng phím J hoặc Chuột trái
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        Vector2 movement = new Vector2(moveX, moveY).normalized;
        rb.velocity = movement * speed;
    }

    void IdleAnimation()
    {
        if (moveY == -1) { anim.SetFloat("IdleX", 0); anim.SetFloat("IdleY", -1); }
        if (moveY == 1) { anim.SetFloat("IdleX", 0); anim.SetFloat("IdleY", 1); }
        if (moveX == -1) { anim.SetFloat("IdleX", -1); anim.SetFloat("IdleY", 0); }
        if (moveX == 1) { anim.SetFloat("IdleX", 1); anim.SetFloat("IdleY", 0); }
    }

    void Attack()
    {
        anim.SetTrigger("isattack");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Va chạm với quái
        if (collision.gameObject.CompareTag("Enemies"))
        {
            Debug.Log("Đã chạm vào quái!");
            TakeDamage(10);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Nhặt vật phẩm (chỉ xóa vật phẩm, không cộng điểm)
        if (other.CompareTag("Item"))
        {
            Destroy(other.gameObject);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Debug.Log(gameObject.name + " đã chết!");
        Destroy(gameObject);
    }
}