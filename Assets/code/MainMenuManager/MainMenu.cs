using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có thư viện này để chuyển Scene

public class MainMenu : MonoBehaviour
{
    // Hàm này sẽ gắn vào nút "New Game" hoặc "Play"
    public void PlayGame()
    {
        SceneManager.LoadScene("Map2");

        // Hoặc bạn có thể dùng chỉ số Index của Scene trong Build Settings:
        // SceneManager.LoadScene(1);
    }

    // Hàm gắn vào nút "Thoát"
    public void QuitGame()
    {
        Debug.Log("Đã thoát game!");
        Application.Quit(); // Chỉ hoạt động khi đã Build ra file .exe
    }
}