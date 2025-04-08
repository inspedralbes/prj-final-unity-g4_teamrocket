using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public Camera playerCameraPrefab; // Prefab de la cámara (asígnale un prefab de cámara desde el Inspector)
    public Transform conoLuzTransform; // Asigna el transform del cono de luz desde el Inspector
    public float zDistanceFromCamera = 10f; // Distancia desde la cámara
    public float rotationOffset = 90f; // Ajusta este valor según la orientación de tu sprite

    private Camera playerCamera; // Cámara instanciada para este jugador

    public override void OnStartLocalPlayer()
    {
        // Instanciar y configurar la cámara para el jugador local
        if (playerCameraPrefab != null)
        {
            // Instanciar la cámara como hijo del personaje
            playerCamera = Instantiate(playerCameraPrefab, transform);
            playerCamera.gameObject.SetActive(true);

            // Asegurarse de que la cámara no sea la cámara principal
            playerCamera.tag = "Untagged"; // No usar el tag "MainCamera"

            // Asignar un depth único basado en el connectionId del jugador
            playerCamera.depth = 0; // El jugador local tiene depth 0

            // Configurar la posición inicial de la cámara
            playerCamera.transform.localPosition = new Vector3(0, 0, -10); // Ajusta la posición Z según sea necesario
        }

        // Activar el cono de luz solo para el jugador local
        if (conoLuzTransform != null)
        {
            conoLuzTransform.gameObject.SetActive(true);
        }
    }

    void Start()
    {
        // Desactivar la cámara de otros jugadores
        if (!isLocalPlayer && playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Solo el jugador local puede mover la cámara y el cono de luz
        if (!isLocalPlayer) return;

        // Controlar el cono de luz
        if (conoLuzTransform != null)
        {
            // Obtén la posición del ratón en la pantalla
            Vector3 mousePosition = Input.mousePosition;

            // Ajusta la coordenada Z para la conversión a coordenadas del mundo
            mousePosition.z = zDistanceFromCamera;

            // Convierte la posición del ratón a coordenadas del mundo
            Vector3 worldMousePosition = playerCamera.ScreenToWorldPoint(mousePosition);
            worldMousePosition.z = 0f; // Asegúrate de que la coordenada Z sea 0 (si estás trabajando en 2D)

            // Calcula la dirección del ratón respecto a la luz
            Vector3 direction = (worldMousePosition - conoLuzTransform.position).normalized;

            // Calcula el ángulo en grados
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Ajusta el ángulo para que esté en el rango [0, 360]
            if (angle < 0) angle += 360;

            // Aplica la rotación al cono de luz
            conoLuzTransform.rotation = Quaternion.Euler(0, 0, angle - rotationOffset);
        }
    }
}using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    private float Horizontal;
    private float Vertical;
    private float Speed = 5f;
    private float SpeedBase = 5f; // Velocidad normal del jugador
    private float SpeedBoost = 10f; // Velocidad duplicada cuando se presiona Shift
    private int enemigosTocandoHitbox = 0; // Contador de enemigos dentro de miHitbox

    private int damage = 1;

    public int vida = 3;
    public int stamina = 100;
    public int staminaMax = 100;
    public int regeneracioStamina = 1;
    public BarraVida barraVida; // Referencia a la barra de vida
    
    public BarraStamina barraStamina; // Referencia a la barra de vida
    public float tiempoEntreGolpes = 0.5f; // Controla la frecuencia del daño
    public float velocidadConsumoStamina = 0.1f; // Controla la frecuencia del daño

    public Collider2D miHitbox; // La hitbox específica del jugador que debe ser golpeada

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        barraVida.SetMaxHealth(vida);
        barraStamina.SetMaxStamina(stamina);
    }

    void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical = Input.GetAxisRaw("Vertical");

        // Si se mantiene presionada la tecla Shift, la velocidad se duplica
        if(stamina != 0){
            Speed = Input.GetKey(KeyCode.LeftShift) ? SpeedBoost : SpeedBase;
        }
        if (Speed == SpeedBoost) {
            ReducirStamina(1);
        }else {
            if (stamina < staminaMax){
                stamina += regeneracioStamina;
                barraStamina.ActualizarStamina(stamina);
            }
        }
        if (stamina <= 0){
            Speed = SpeedBase;
        }
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Vertical * Speed);
    }

    public void ReducirStamina(int cantidad)
    {
        stamina -= cantidad;
        if (stamina < 0) stamina = 0;
        
        barraStamina.ActualizarStamina(stamina);
        Debug.Log("Stamina restante: " + stamina);
    }

    public void RecibirDano(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            Debug.Log("Muerto");
            CancelInvoke("RecibirDanoPeriodico"); // Detiene el daño cuando la vida llega a 0
            GameObject.Find("Barras").SetActive(false);
            gameObject.SetActive(false);
            SceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
        }

        barraVida.ActualizarVida(vida);
        Debug.Log("Vida restante: " + vida);
    }

    private void RecibirDanoPeriodico()
    {
        RecibirDano(damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.IsTouching(miHitbox))
        {
            enemigosTocandoHitbox++; // Un enemigo más dentro
            // Obtener el script del enemigo
            EnemigoBase enemigo = collision.GetComponent<EnemigoBase>();
            if (enemigo != null) // Verificar que el enemigo tiene el script
            {
            damage = enemigo.damage; // Obtener el valor de daño del enemigo
            }
            InvokeRepeating("RecibirDanoPeriodico", 0f, tiempoEntreGolpes);
            Debug.Log("Invoke");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !collision.IsTouching(miHitbox))
    {
        enemigosTocandoHitbox--; // Un enemigo menos dentro

        // Solo cancelar Invoke si ya no quedan enemigos tocando miHitbox
        if (enemigosTocandoHitbox <= 0)
        {
            CancelInvoke("RecibirDanoPeriodico");
            Debug.Log("Cancel Invoke");
        }
    }
    }
}
