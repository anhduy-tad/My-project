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
    public bool isSmooth = false;
    public float smoothSpeed = 5f;

    private float targetHealth;

    public void SetMaxHealth(float health)
    {
        if (slider == null) slider = GetComponent<Slider>();

        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;
            targetHealth = health;
        }

        UpdateColor();
    }

    public void SetHealth(float health)
    {
        targetHealth = health;

        if (!isSmooth && slider != null)
        {
            slider.value = health;
            UpdateColor();
        }
    }

    private void Update()
    {
        // CHỐNG LỖI NULL: Chỉ chạy hiệu ứng mượt khi đã có Slider
        if (slider == null) return;

        if (isSmooth && Mathf.Abs(slider.value - targetHealth) > 0.01f)
        {
            slider.value = Mathf.Lerp(slider.value, targetHealth, Time.deltaTime * smoothSpeed);
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        if (useGradientColor && fillImage != null && slider != null && slider.maxValue > 0)
        {
            fillImage.color = healthGradient.Evaluate(slider.normalizedValue);
        }
    }
}