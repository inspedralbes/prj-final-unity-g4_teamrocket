using UnityEngine;
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
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        // TiempoEspejoRestante();
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
    
    public void InvertirControles(float tiempo)
    {
        controlesInvertidos = true;
        tiempoRestanteInversion = tiempo;
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
    */
}