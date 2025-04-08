using UnityEngine;
using UnityEngine.UI;

public class BarraStamina : MonoBehaviour
{
    public Image rellenoBarraStamina;
    private float StaminaMaxima;

    public void SetMaxStamina(int maxStamina)
    {
        StaminaMaxima = maxStamina;
        ActualizarStamina(maxStamina);
    }

    public void ActualizarStamina(int staminaActual)
    {
        rellenoBarraStamina.fillAmount = (float)staminaActual / StaminaMaxima;
    }
}