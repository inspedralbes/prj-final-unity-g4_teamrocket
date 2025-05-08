using UnityEngine;

public class Batear : MonoBehaviour
{
    public float velocidadAtaque = 360f; // grados por segundo
    private float anguloInicial = 60f;
    private float anguloFinal = -60f;
    private float anguloActual;
    private bool atacando = false;
    private PlayerController playerController;
    private Camera mainCamera;
    private Quaternion rotacionInicialHaciaRaton;

    void OnEnable()
    {
        mainCamera = Camera.main;
        anguloActual = anguloInicial;
        atacando = true;
        playerController = transform.root.GetComponent<PlayerController>();
        
        // Calcular dirección al ratón
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direccion = (mousePosition - transform.position).normalized;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        
        // Guardar la rotación inicial hacia el ratón
        rotacionInicialHaciaRaton = Quaternion.Euler(0f, 0f, angulo - 90);
        
        // Aplicar rotación inicial
        transform.rotation = rotacionInicialHaciaRaton;
    }

    void Update()
    {
        if (!atacando) return;

        // Aplicar animación de golpe en la dirección del ratón
        anguloActual -= velocidadAtaque * Time.deltaTime;
        transform.rotation = rotacionInicialHaciaRaton * Quaternion.Euler(0f, 0f, anguloActual);

        if (anguloActual <= anguloFinal)
        {
            atacando = false;
            gameObject.SetActive(false);
            if (playerController != null)
                playerController.puedeAtacar = true;
        }
    }
}