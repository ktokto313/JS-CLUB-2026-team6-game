using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color healthColor = Color.green;

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
        if (fillImage != null) fillImage.color = healthColor;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}