using UnityEngine;

public class Batear : MonoBehaviour
{
    public float velocidadAtaque = 360f; // grados por segundo
    private float anguloInicial = 0f;
    private float anguloFinal = -120f;
    private float anguloActual;
    private bool atacando = false;
    private PlayerController playerController;

    void OnEnable()
    {
        anguloActual = anguloInicial;
        atacando = true;
        playerController = transform.root.GetComponent<PlayerController>();
        transform.localRotation = Quaternion.Euler(0f, 0f, anguloInicial);
    }

    void Update()
    {
        if (!atacando) return;

        anguloActual -= velocidadAtaque * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(0f, 0f, anguloActual);

        if (anguloActual <= anguloFinal)
        {
            atacando = false;
            gameObject.SetActive(false);
            if (playerController != null)
                playerController.puedeAtacar = true;
        }
    }
}
