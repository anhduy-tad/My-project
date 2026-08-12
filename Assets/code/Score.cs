using UnityEngine;
using TMPro; // Thư viện bắt buộc để dùng TextMeshPro

public class ScoreManager : MonoBehaviour
{
    // Singleton giúp gọi ScoreManager từ bất kỳ đâu
    public static ScoreManager instance;

    [Header("--- UI TEXTMESHPRO ---")]
    public TextMeshProUGUI scoreText; // Kéo 'Score Number' vào đây

    [Header("--- ĐIỂM SỐ ---")]
    public int score = 0; // Biến lưu điểm hiện tại

    private void Awake()
    {
        // Khởi tạo Singleton chuẩn
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Cập nhật giao diện điểm ban đầu khi vừa mở game
        UpdateScoreUI();
    }

    // TỰ ĐỘNG CẬP NHẬT UI LIÊN TỤC MỖI FRAME (Chống lỗi không lên điểm)
    private void Update()
    {
        UpdateScoreUI();
    }

    // Hàm gọi từ bên ngoài để cộng điểm (Ví dụ: ScoreManager.instance.AddScore(10);)
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"Nhặt vật phẩm thành công! +{amount} điểm. Tổng điểm hiện tại: {score}");

        UpdateScoreUI(); // Cập nhật lại UI ngay khi có điểm mới
    }

    // Hàm cập nhật điểm lên màn hình TextMeshPro
    public void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            // Hiển thị số điểm dạng 2 chữ số (00, 10, 20...) cho đúng chuẩn UI góc trên
            scoreText.text = score.ToString("D2");
        }
    }
}