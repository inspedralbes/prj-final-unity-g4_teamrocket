using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal; // Para versiones más recientes de Unity

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    private float Horizontal;
    private float Vertical;
    private float Speed = 5f;
    private float SpeedBase = 5f;
    private float SpeedBoost = 10f;
    private int enemigosTocandoHitbox = 0;
    private int damage = 1;

    public int vida = 3;
    public int stamina = 100;
    public int staminaMax = 100;
    public int regeneracioStamina = 1;
    public BarraVida barraVida;
    public BarraStamina barraStamina;
    public float tiempoEntreGolpes = 0.5f;
    public Collider2D miHitbox;

    [SerializeField] private ShopController shopController;
    private bool canMove = true;
    private bool canOpenShop = true;

    // Variables para la linterna
    private bool flashing = true; // Indica si el jugador tiene la linterna
    private bool linternaActiva = false; // Indica si la linterna está encendida
    public Light2D luzLinterna; // Referencia al componente Light2D
    public Collider2D colliderLinternaNormal; // Collider del cono normal
    public Collider2D colliderLinternaAmplio; // Collider del cono ampliado
    private float tiempoLinternaEncendida = 0f;
    private float duracionMaximaLinterna = 180f; // 180 segundos

    // Variables brujula
    private bool brujula = true;
    public GameObject objetoBrujula; // Referencia al GameObject que representa la brújula

    public bool puedeAtacar = true; // Indica si el jugador puede atacar
    public GameObject bate; // Objeto del bate

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
        barraVida.SetMaxHealth(vida);
        barraStamina.SetMaxStamina(stamina);

        if (shopController != null)
        {
            shopController.OnShopToggle += HandleShopToggle;
            shopController.ToggleShop(false);
        }
    }

    private void OnDestroy()
    {
        if (shopController != null)
        {
            shopController.OnShopToggle -= HandleShopToggle;
        }
        
        // Inicialización de la linterna
        colliderLinternaAmplio.enabled = false; // Asegurarse que el collider amplio está desactivado al inicio
    }

    void Update()
    {
        if (!canMove) return;

        HandleMovementInput();
        HandleShopInput();
    }

    private void HandleMovementInput()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical = Input.GetAxisRaw("Vertical");

        if (stamina != 0)
        {
            Speed = Input.GetKey(KeyCode.LeftShift) ? SpeedBoost : SpeedBase;
        }

        if (Speed == SpeedBoost)
        {
            ReducirStamina(1);
        }
        else if (stamina < staminaMax)
        {
            stamina += regeneracioStamina;
            barraStamina.ActualizarStamina(stamina);
        }

        if (stamina <= 0)
        {
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

        // Lógica Brujula
        if (brujula)
        {
            ActivateBrujula();
        } else{
            DesactivarBrujula();
        }

        if (puedeAtacar && Input.GetMouseButtonDown(0)) // Clic izquierdo
        {
            puedeAtacar = false;
            if (bate != null)
            {
                bate.SetActive(true);
            }
        }
    }

    private void HandleShopInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && canOpenShop)
        {
            ToggleShop();
        }
    }

    private void HandleShopInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && canOpenShop)
        {
            ToggleShop();
        }
    }

    private void FixedUpdate()
    {
        if (!canMove) return;
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Vertical * Speed);
    }

    private void ActivateBrujula()
    {
        if (objetoBrujula != null)
        {
            objetoBrujula.SetActive(true);
        }
    }

    private void DesactivarBrujula()
    {
        if (objetoBrujula != null)
        {
            objetoBrujula.SetActive(false);
        }
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
    }

    public void RecibirDano(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            Morir();
        }
        barraVida.ActualizarVida(vida);
    }

    private void Morir()
    {
        Debug.Log("Muerto");
        CancelInvoke("RecibirDanoPeriodico");
        GameObject.Find("Barras").SetActive(false);
        gameObject.SetActive(false);
        SceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
    }

    private void RecibirDanoPeriodico()
    {
        RecibirDano(damage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.IsTouching(miHitbox))
        {
            HandleEnemyCollision(collision);
        }
        else if (collision.CompareTag("Tienda"))
        {
            canOpenShop = true;
            Debug.Log("Puedes abrir la tienda con E");
        }
    }

    private void HandleEnemyCollision(Collider2D collision)
    {
        enemigosTocandoHitbox++;
        EnemigoBase enemigo = collision.GetComponent<EnemigoBase>();
        if (enemigo != null)
        {
                damage = enemigo.damage;
        }
        InvokeRepeating("RecibirDanoPeriodico", 0f, tiempoEntreGolpes);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && !collision.IsTouching(miHitbox))
        {
            enemigosTocandoHitbox--;
            if (enemigosTocandoHitbox <= 0)
            {
                CancelInvoke("RecibirDanoPeriodico");
            }
        }
    }

    private void ToggleShop()
    {
        if (shopController != null)
        {
            bool newState = !shopController.IsShopVisible();
            shopController.ToggleShop(newState);
        }
    }

    private void HandleShopToggle(bool isOpen)
    {
        canMove = !isOpen;
        
        if (!isOpen)
        {
            Time.timeScale = 1f;
            Rigidbody2D.linearVelocity = Vector2.zero;
        }
    }
}