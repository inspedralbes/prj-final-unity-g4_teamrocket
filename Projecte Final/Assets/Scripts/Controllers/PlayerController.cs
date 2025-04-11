using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private float horizontal;
    private float vertical;
    public float speed;
    public float speedBase = 5f; // Velocidad normal del jugador
    public float speedBoost = 10f; // Velocidad duplicada cuando se presiona Shift
    public float maxHealth = 100f;

    public HealthBar healthBar;
    // private bool controlesInvertidos = false;
    // private float tiempoRestanteInversion = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        // TiempoEspejoRestante();
        // Si se mantiene presionada la tecla Shift, la velocidad se duplica
        speed = Input.GetKey(KeyCode.LeftShift) ? speedBoost : speedBase;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, vertical * speed);
    }

    public void TakeDamage(float damage) {
        float newHealth = maxHealth - damage;
        maxHealth = Mathf.Clamp(newHealth, 0, 100f);

        if(maxHealth <= 0) {
            Die();
        }

        healthBar.UpdateHealth(maxHealth);
    }

    void Die() {
        // GameObject.Find("Barras").SetActive(false);
        gameObject.SetActive(false);
    }

    /*
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
    */
}