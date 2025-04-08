using UnityEngine;
using UnityEngine.UI;

public class BarraVida : MonoBehaviour
{
    public Image rellenoBarraVida;
    private float vidaMaxima;

    public void SetMaxHealth(int maxVida)
    {
        vidaMaxima = maxVida;
        ActualizarVida(maxVida);
    }

    public void ActualizarVida(int vidaActual)
    {
        rellenoBarraVida.fillAmount = (float)vidaActual / vidaMaxima;
    }
}