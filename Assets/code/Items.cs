using UnityEngine;

public enum ItemType { Score, Heal }

public class Item : MonoBehaviour
{
    [Header("--- THÔNG SỐ VẬT PHẨM ---")]
    public ItemType itemType = ItemType.Score;
    public int scoreValue = 2; // Đã đổi giá trị coin mặc định thành 2 điểm
    public float healValue = 20f;

    [Header("--- HIỆU ỨNG NHÚN NHẢY (SAU KHI RƠI) ---")]
    public bool enableBobbing = true;
    public float bobbingSpeed = 4f;
    public float bobbingAmount = 0.12f;

    private Vector3 startPos;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasLanded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Khi đã chạm đất -> Nhún nhảy nhẹ nhàng lên xuống
        if (hasLanded && enableBobbing)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    // 1. Khi đồng xu đang rơi và va chạm (Chưa bật Is Trigger)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Nếu rơi đụng trúng Player -> Nhặt luôn!
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            Collect(player);
            return;
        }

        // Nếu rơi đụng đất -> Chờ nảy xong rồi cố định vị trí
        if (!hasLanded)
        {
            Invoke(nameof(FixPositionAfterLanding), 0.4f);
        }
    }

    private void FixPositionAfterLanding()
    {
        hasLanded = true;
        startPos = transform.position;

        // Chuyển Rigidbody sang Kinematic để xu không bị lăn/rơi tiếp
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Bật Trigger để Player chạy qua nhặt mượt mà không bị khựng người
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    // 2. Khi đồng xu đã nằm yên (Đã bật Is Trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            Collect(player);
        }
    }

    // Hàm cộng thưởng cho Player và xóa xu
    public void Collect(Player player)
    {
        if (itemType == ItemType.Score)
        {
            // Cộng 2 điểm vào Player
            player.AddScore(scoreValue);
        }
        else if (itemType == ItemType.Heal)
        {
            player.Heal(healValue);
        }

        Destroy(gameObject); // Biến mất sau khi nhặt
    }
}