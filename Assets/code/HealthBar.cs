using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("--- UI COMPONENTS ---")]
    public Slider slider;
    public Image fillImage;
    [Header("--- CÀI ĐẶT MÀU SẮC (TÙY CHỌN) ---")]
    public bool useGradientColor = true;
    public Gradient healthGradient;

    [Header("--- HIỆU ỨNG TỤT MÁU MƯỢT ---")]
    public bool isSmooth = true;
    public float smoothSpeed = 5f;

    private float targetHealth;


    public void SetMaxHealth(float maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        targetHealth = maxHealth;

        UpdateColor();
    }

    // Cập nhật giá trị máu khi nhận Damage hoặc hồi máu
    public void SetHealth(float currentHealth)
    {
        targetHealth = Mathf.Clamp(currentHealth, 0f, slider.maxValue);

        // Nếu không dùng hiệu ứng mượt thì cập nhật trực tiếp ngay lập tức
        if (!isSmooth)
        {
            slider.value = targetHealth;
            UpdateColor();
        }
    }

    void Update()
    {
        // Xử lý hiệu ứng thanh máu di chuyển mượt mà về giá trị target
        if (isSmooth && Mathf.Abs(slider.value - targetHealth) > 0.01f)
        {
            slider.value = Mathf.Lerp(slider.value, targetHealth, Time.deltaTime * smoothSpeed);
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        if (useGradientColor && fillImage != null)
        {
            // Tự động đổi màu dựa trên tỉ lệ % máu (0.0 đến 1.0)
            fillImage.color = healthGradient.Evaluate(slider.normalizedValue);
        }
    }
}