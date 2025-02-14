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

    private int damage = 1;

    public int vida = 3;
    public BarraVida barraVida; // Referencia a la barra de vida
    public float tiempoEntreGolpes = 0.8f; // Controla la frecuencia del daño

    public Collider2D miHitbox; // La hitbox específica del jugador que debe ser golpeada
    private bool controlesInvertidos = false;
    private float tiempoRestanteInversion = 0f;

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        barraVida.SetMaxHealth(vida);
    }

    void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical = Input.GetAxisRaw("Vertical");
        TiempoEspejoRestante();
        // Si se mantiene presionada la tecla Shift, la velocidad se duplica
        Speed = Input.GetKey(KeyCode.LeftShift) ? SpeedBoost : SpeedBase;
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Vertical * Speed);
    }

    public void RecibirDano(int cantidad)
    {
        vida -= cantidad;
        if (vida < 0) vida = 0;
        barraVida.ActualizarVida(vida);
        Debug.Log("Vida restante: " + vida);

        if (vida <= 0)
        {
            Debug.Log("Muerto");
            CancelInvoke("RecibirDanoPeriodico"); // Detiene el daño cuando la vida llega a 0
            GameObject.Find("Barras").SetActive(false);
            gameObject.SetActive(false);
            SceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
        }
    }

    private void RecibirDanoPeriodico()
    {
        RecibirDano(damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.IsTouching(miHitbox))
        {
            // Obtener el script del enemigo
            EnemigoBase enemigo = collision.GetComponent<EnemigoBase>();
            if (enemigo != null) // Verificar que el enemigo tiene el script
            {
            damage = enemigo.damage; // Obtener el valor de daño del enemigo
            }
            InvokeRepeating("RecibirDanoPeriodico", 0f, tiempoEntreGolpes);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            CancelInvoke("RecibirDanoPeriodico");
        }
    }
    public void TiempoEspejoRestante(){
        tiempoRestanteInversion -= Time.deltaTime;
        if (tiempoRestanteInversion <= 0)
        {
            controlesInvertidos = false;
        }
        if (controlesInvertidos) {
            Horizontal = -Horizontal;
            Vertical = -Vertical;
        }
    }
    public void InvertirControles(float tiempo)
    {
        controlesInvertidos = true;
        tiempoRestanteInversion = tiempo;
    }
}
