using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        startPos = transform.position;
        currentState = State.Patrol;
    }

    void Update()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
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
        rb.velocity = new Vector2(desiredVelocityX, rb.velocity.y);
    }

    void Patrol()
    {
        desiredVelocityX = direction * moveSpeed;

        float offset = transform.position.x - startPos.x;

        if (offset >= patrolDistance)
            direction = -1;
        else if (offset <= -patrolDistance)
            direction = 1;

        Flip(direction);
    }

    void Chase()
    {
        if (player == null) return;

        float dir = Mathf.Sign(player.position.x - transform.position.x);

        desiredVelocityX = dir * moveSpeed * chaseSpeedMultiplier;

        Flip((int)dir);
    }

    void ReturnToStart()
    {
        float dir = Mathf.Sign(startPos.x - transform.position.x);

        desiredVelocityX = dir * moveSpeed;

        Flip((int)dir);

        if (Vector2.Distance(transform.position, startPos) < 0.1f)
        {
            desiredVelocityX = 0;
            currentState = State.Patrol;
        }
    }

    void Flip(int dir)
    {
        if (dir == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
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
}