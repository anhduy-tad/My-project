using UnityEngine;

public enum ItemType { Score, Heal }

public class Item : MonoBehaviour
{
    [Header("--- THÔNG SỐ VẬT PHẨM ---")]
    public ItemType itemType = ItemType.Score;
    public int scoreValue = 2; // Đã đổi giá trị coin mặc định thành 2 điểm
    public float healValue = 20f;

    [Header("--- ÂM THANH NHẶT VẬT PHẨM ---")]
    public AudioClip collectSound; // Kéo file âm thanh (tiếng nhặt xu/máu) vào đây
    [Range(0f, 1f)] public float soundVolume = 1f; // Âm lượng tiếng nhặt

    [Header("--- HIỆU ỨNG NHÚN NHẢY (SAU KHI RƠI) ---")]
    public bool enableBobbing = true;
    public float bobbingSpeed = 4f;
    public float bobbingAmount = 0.12f;

    private Vector3 startPos;
    private Rigidbody2D rb;
    private Collider2D col;
    private bool hasLanded = false;
    private bool isCollected = false; // Cờ chặn việc bị nhặt nhiều lần trong 1 frame

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        // Khi đã chạm đất -> Nhún nhảy nhẹ nhàng lên xuống
        if (hasLanded && enableBobbing && !isCollected)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    // 1. Khi đồng xu đang rơi và va chạm (Chưa bật Is Trigger)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCollected) return;

        // Nếu rơi đụng trúng Player -> Nhặt luôn!
        Player player = collision.gameObject.GetComponentInParent<Player>();
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
        if (isCollected) return;

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
        if (isCollected) return;

        // Dùng GetComponentInParent để đảm bảo nhận đúng script Player kể cả khi va chạm vào Feet/Body Collider
        Player player = collision.GetComponentInParent<Player>();
        if (player != null)
        {
            Collect(player);
        }
    }

    // Hàm cộng thưởng cho Player và xóa xu
    public void Collect(Player player)
    {
        // Nếu item đã được nhặt rồi -> Bỏ qua ngay lập tức!
        if (isCollected) return;
        isCollected = true;

        // Tắt ngay Collider để tránh va chạm thêm với các Collider khác của Player trong frame này
        if (col != null) col.enabled = false;

        // Phát âm thanh tại vị trí hiện tại
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, soundVolume);
        }

        if (itemType == ItemType.Score)
        {
            // Kiểm tra ưu tiên gọi qua ScoreManager trước nếu có, hoặc gọi trực tiếp Player
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.AddScore(scoreValue);
            }
            else if (player != null)
            {
                player.AddScore(scoreValue);
            }
        }
        else if (itemType == ItemType.Heal)
        {
            if (player != null)
            {
                player.Heal(healValue);
            }
        }

        Destroy(gameObject); // Biến mất sau khi nhặt
    }
}