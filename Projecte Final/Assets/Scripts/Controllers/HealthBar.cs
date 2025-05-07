using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillingBarLife;
    private float maxHealth;

    public void SetMaxHealth(float maxVida)
    {
        maxHealth = maxVida;
        UpdateHealth(maxVida);
    }

    public void UpdateHealth(float actualHealth)
    {
        fillingBarLife.fillAmount = actualHealth / maxHealth;
    }
}