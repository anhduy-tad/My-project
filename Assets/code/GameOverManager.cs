using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverUI; // Bảng UI Game Over

    public void SetupGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true); // Hiển thị bảng Game Over
        }
    }

    // Nút Nối vào Button "Restart"
    public void RestartGame()
    {
        // Load lại Scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Nút Nối vào Button "Main Menu"
    public void LoadMainMenu()
    {
        // Chuyển về Scene Menu (thay "MainMenu" bằng tên Scene của bạn)
        SceneManager.LoadScene("MainMenu");
    }
}