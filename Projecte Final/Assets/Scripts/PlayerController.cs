using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal; // Para versiones más recientes de Unity

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

    // Variables para la linterna
    private bool flashing = true; // Indica si el jugador tiene la linterna
    private bool linternaActiva = false; // Indica si la linterna está encendida
    public Light2D luzLinterna; // Referencia al componente Light2D
    public Collider2D colliderLinternaNormal; // Collider del cono normal
    public Collider2D colliderLinternaAmplio; // Collider del cono ampliado
    private float tiempoLinternaEncendida = 0f;
    private float duracionMaximaLinterna = 180f; // 180 segundos

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        barraVida.SetMaxHealth(vida);
        barraStamina.SetMaxStamina(stamina);
        
        // Inicialización de la linterna
        colliderLinternaAmplio.enabled = false; // Asegurarse que el collider amplio está desactivado al inicio
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

        // Lógica de la linterna
        if (flashing && Input.GetKeyDown(KeyCode.F))
        {
            ToggleLinterna();
        }

        if (linternaActiva)
        {
            tiempoLinternaEncendida += Time.deltaTime;
            if (tiempoLinternaEncendida >= duracionMaximaLinterna)
            {
                ApagarLinterna();
            }
        }
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Vertical * Speed);
    }

    // Métodos para la linterna
    private void ToggleLinterna()
    {
        if (linternaActiva)
        {
            ApagarLinterna();
        }
        else
        {
            EncenderLinterna();
        }
    }

    private void EncenderLinterna()
    {
        linternaActiva = true;
        luzLinterna.pointLightOuterRadius = 10.4f; // Doble del radio normal
        colliderLinternaNormal.enabled = false;
        colliderLinternaAmplio.enabled = true;
    }

    private void ApagarLinterna()
    {
        linternaActiva = false;
        luzLinterna.pointLightOuterRadius = 5.2f; // Radio normal
        colliderLinternaAmplio.enabled = false;
        colliderLinternaNormal.enabled = true;
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