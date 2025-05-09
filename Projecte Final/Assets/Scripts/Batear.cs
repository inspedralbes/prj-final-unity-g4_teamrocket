using UnityEngine;

public class Batear : MonoBehaviour
{
    public float velocidadAtaque = 360f; // grados por segundo
    public float tiempoStun = 2f; // segundos de inmovilización
    private float anguloActual;
    private bool atacando = false;
    private PlayerController playerController;
    private Camera mainCamera;
    private Quaternion rotacionInicialHaciaRaton;
    private float anguloInicial;
    private float anguloFinal;
    private bool golpeDerecha;
    private float velocidadActual;

    void OnEnable()
    {
        mainCamera = Camera.main;
        atacando = true;
        playerController = transform.root.GetComponent<PlayerController>();
        
        // Calcular dirección al ratón
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direccion = (mousePosition - transform.position).normalized;
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        
        // Determinar si es golpe a derecha o izquierda
        golpeDerecha = direccion.x > 0;
        
        // Ajustar ángulos según la dirección
        if (golpeDerecha)
        {
            anguloInicial = 60f;
            anguloFinal = -60f;
            velocidadActual = velocidadAtaque;
        }
        else
        {
            anguloInicial = -60f;
            anguloFinal = 60f;
            velocidadActual = -velocidadAtaque;
        }
        
        anguloActual = anguloInicial;
        rotacionInicialHaciaRaton = Quaternion.Euler(0f, 0f, angulo - 90);
        transform.rotation = rotacionInicialHaciaRaton;
    }

    void Update()
    {
        if (!atacando) return;

        anguloActual -= velocidadActual * Time.deltaTime;
        transform.rotation = rotacionInicialHaciaRaton * Quaternion.Euler(0f, 0f, anguloActual);

        bool animacionCompletada = golpeDerecha 
            ? (anguloActual <= anguloFinal) 
            : (anguloActual >= anguloFinal);

        if (animacionCompletada)
        {
            atacando = false;
            gameObject.SetActive(false);
            if (playerController != null)
                playerController.puedeAtacar = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si golpeamos a un jugador o enemigo
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            // Intentar obtener el componente de movimiento
            MonoBehaviour[] movementScripts = other.GetComponents<MonoBehaviour>();
            foreach (var script in movementScripts)
            {
                if (script is IStunnable stunnable)
                {
                    stunnable.Stun(tiempoStun);
                    break;
                }
            }
        }
    }
}

public interface IStunnable
{
    void Stun(float duration);
}