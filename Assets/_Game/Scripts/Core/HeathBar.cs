using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (slider == null) return;
        
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
        Vector3 parentScale = transform.parent.localScale;
        transform.localScale = new Vector3(
            Mathf.Sign(parentScale.x) * Mathf.Abs(transform.localScale.x), 
            transform.localScale.y, 
            transform.localScale.z
        );
    }
}