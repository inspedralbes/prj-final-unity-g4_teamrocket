using UnityEngine;

public class Batear : MonoBehaviour
{
    public float velocidadAtaque = 360f; // grados por segundo
    private float anguloInicial = 90f;
    private float anguloFinal = -90f;
    private float anguloActual;
    private bool atacando = false;
    private Transform jugador;
    private PlayerController playerController;

    void OnEnable()
    {
        jugador = transform.parent;
        playerController = jugador.GetComponent<PlayerController>();
        anguloActual = anguloInicial;
        atacando = true;
    }

    void Update()
    {
        if (!atacando) return;

        anguloActual -= velocidadAtaque * Time.deltaTime;

        // Calcula la posición del bate como si girara alrededor del jugador
        float radio = 1f; // distancia desde el centro
        Vector3 offset = new Vector3(Mathf.Cos(anguloActual * Mathf.Deg2Rad), Mathf.Sin(anguloActual * Mathf.Deg2Rad), 0) * radio;
        transform.position = jugador.position + offset;

        // Rota el bate para que apunte hacia la trayectoria
        Vector3 direccion = transform.position - jugador.position;
        float rotZ = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90f, 90f, rotZ);

        if (anguloActual <= anguloFinal)
        {
            atacando = false;
            gameObject.SetActive(false);
            if (playerController != null)
            {
                playerController.puedeAtacar = true;
            }
        }
    }
}
