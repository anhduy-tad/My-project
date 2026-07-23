using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float patrolDistance = 6f;
    public float chaseRange = 6f;
    public float chaseSpeedMultiplier = 1.5f;

    [Header("Health")]
    public int currentHealth = 100;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;

    private Vector3 startPos;
    private int direction = 1;
    private float desiredVelocityX;

    private enum State
    {
        Patrol,
        Chase,
        Return
    }

    private State currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Khóa xoay Z để physics không làm đè/đổ Enemy
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        FindPlayer();

        startPos = transform.position;
        currentState = State.Patrol;
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
        }

        float distance = player == null
            ? Mathf.Infinity
            : Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                if (distance <= chaseRange)
                    currentState = State.Chase;
                else
                    Patrol();
                break;

            case State.Chase:
                if (distance > chaseRange)
                    currentState = State.Return;
                else
                    Chase();
                break;

            case State.Return:
                ReturnToStart();
                break;
        }
    }

    void FixedUpdate()
    {
        // Cập nhật vận tốc vật lý
        rb.velocity = new Vector2(desiredVelocityX, rb.velocity.y);
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Patrol()
    {
        float offset = transform.position.x - startPos.x;

        // Chỉ đổi hướng nếu đang đi tiến về phía biên (tránh giật lag lặp đi lặp lại)
        if (offset >= patrolDistance && direction > 0)
            direction = -1;
        else if (offset <= -patrolDistance && direction < 0)
            direction = 1;

        desiredVelocityX = direction * moveSpeed;
        Flip(direction);
    }

    void Chase()
    {
        if (player == null) return;

        float xDiff = player.position.x - transform.position.x;

        if (Mathf.Abs(xDiff) > 0.1f)
        {
            float dir = Mathf.Sign(xDiff);
            desiredVelocityX = dir * moveSpeed * chaseSpeedMultiplier;
            Flip((int)dir);
        }
        else
        {
            desiredVelocityX = 0;
        }
    }

    void ReturnToStart()
    {
        float xDiff = startPos.x - transform.position.x;

        // Nếu đã về gần đúng điểm ban đầu
        if (Mathf.Abs(xDiff) < 0.2f)
        {
            direction = xDiff >= 0 ? 1 : -1;
            currentState = State.Patrol;
            Patrol();
            return;
        }

        float dir = Mathf.Sign(xDiff);
        desiredVelocityX = dir * moveSpeed;
        Flip((int)dir);
    }

    void Flip(int dir)
    {
        if (dir == 0) return;

        // Lật SpriteRenderer thay vì lật transform.localScale
        // Giúp giữ cố định Collider2D, tránh hiện tượng va chạm gây giật/chạy tại chỗ
        spriteRenderer.flipX = (dir < 0);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // Vẽ bán kính tuần tra và đuổi theo trên Editor để dễ căn chỉnh
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? startPos : transform.position;

        // Bán kính Chase
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Giới hạn Patrol
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin + Vector3.left * patrolDistance, origin + Vector3.right * patrolDistance);
    }
}