using UnityEngine;

public class HealItem : MonoBehaviour
{
    [Header("Chỉ số hồi máu")]
    public float healAmount = 20f; // Số máu hồi lại cho Player

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem đối tượng va chạm có phải Player không
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // Hồi máu cho Player
                player.Heal(healAmount);
                Debug.Log($"❤️ Player đã nhặt bình máu! Hồi {healAmount} máu.");

                // Xóa item sau khi nhặt
                Destroy(gameObject);
            }
        }
    }
}