using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour, IStunnable
{
    // Movimiento y física
    private Rigidbody2D rb;
    private float horizontal;
    private float vertical;
    private float speed = 5f;
    private float speedBase = 5f;
    private float speedBoost = 10f;
    
    // Atributos del jugador
    //private int enemigosTocandoHitbox = 0;
    //private int damage = 1;
    //public int vida = 3;
    public float maxHealth = 100f;
    public int stamina = 100;
    public int staminaMax = 100;
    public int regeneracioStamina = 1;
    
    // Referencias UI
    //public BarraVida barraVida;
    public HealthBar healthBar;
    public BarraStamina barraStamina;
    
    // Combate
    public float tiempoEntreGolpes = 0.5f;
    public Collider2D miHitbox;
    public bool puedeAtacar = true;
    public GameObject bate;
    
    // Tienda
    [SerializeField] private ShopController shopController;
    private bool canMove = true;
    private bool canOpenShop = true;
    
    // Linterna
    private bool flashing = true;
    private bool linternaActiva = false;
    public Light2D luzLinterna;
    public Collider2D colliderLinternaNormal;
    public Collider2D colliderLinternaAmplio;
    private float tiempoLinternaEncendida = 0f;
    private float duracionMaximaLinterna = 180f;
    
    // Brújula
    private bool brujula = true;
    public GameObject objetoBrujula;

    // Añade estas nuevas variables para el stun
    private bool isStunned = false;
    private float stunTimer = 0f;
    private Coroutine stunCoroutine;
    [SerializeField] private GameObject stunEffect;

    private Animator animator;
    private Vector2 lastDirection;
    private bool forceAnimationUpdate;

    void Start()
    {
        canOpenShop = false;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.Update(0f); // Fuerza una actualización inmediata
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", 0);
        animator.SetFloat("Speed", 0);
            
        // Inicializar barras de UI
        //if (barraVida != null) barraVida.SetMaxHealth(vida);
        if (healthBar != null) healthBar.SetMaxHealth(maxHealth);
        if (barraStamina != null) barraStamina.SetMaxStamina(stamina);
        
        // Configurar tienda
        if (shopController != null)
        {
            shopController.OnShopToggle += HandleShopToggle;
            shopController.ToggleShop(false);
        }
        
        // Inicializar linterna
        if (colliderLinternaAmplio != null) colliderLinternaAmplio.enabled = false;
    }

    private void OnDestroy()
    {
        if (shopController != null)
        {
            shopController.OnShopToggle -= HandleShopToggle;
        }
    }

    void Update()
    {
        if (!canMove || isStunned) return;

        HandleMovementInput();
        HandleShopInput();
        HandleEquipmentInput();
    }

    // Implementación de la interfaz IStunnable
    public void Stun(float duration)
    {
        // Si ya está aturdido, reiniciamos el timer
        if (isStunned)
        {
            stunTimer = duration;
            return;
        }

        isStunned = true;
        stunTimer = duration;
        
        // Opcional: Activar efecto visual
        if (stunEffect != null) stunEffect.SetActive(true);
        
        // Detener el movimiento inmediatamente
        rb.linearVelocity = Vector2.zero;
        
        // Iniciar corutina para el stun
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        while (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            yield return null;
        }

        // Finalizar el stun
        isStunned = false;
        
        // Opcional: Desactivar efecto visual
        if (stunEffect != null) stunEffect.SetActive(false);
    }

    private void HandleMovementInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if (stamina > 0)
        {
            speed = Input.GetKey(KeyCode.LeftShift) ? speedBoost : speedBase;
        }

        if (speed == speedBoost)
        {
            ReducirStamina(1);
        }
        else if (stamina < staminaMax)
        {
            stamina += regeneracioStamina;
            if (barraStamina != null) barraStamina.ActualizarStamina(stamina);
        }

        if (stamina <= 0)
        {
            speed = speedBase;
        }

        Vector2 inputDirection = new Vector2(horizontal, vertical);
    
        // Actualización inmediata (sin normalizar para mantener magnitud)
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", inputDirection.magnitude);

        // // Forzar cambio inmediato si hay input
        // if (inputDirection.magnitude > 0.1f)
        // {
        //     animator.Play("Movement", 0, 0f); // Fuerza reinicio de animación
        // }
    }

    private void HandleShopInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && canOpenShop)
        {
            ToggleShop();
        }
    }

    private void HandleEquipmentInput()
    {
        // Linterna
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

        // Brújula
        if (brujula)
        {
            ActivateBrujula();
        }
        else
        {
            DesactivarBrujula();
        }

        // Ataque con bate
        if (puedeAtacar && Input.GetMouseButtonDown(0))
        {
            puedeAtacar = false;
            if (bate != null) bate.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        if (!canMove || isStunned) return;
        rb.linearVelocity = new Vector2(horizontal * speed, vertical * speed);
    }

    #region Brújula
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
    #endregion

    #region Linterna
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
        if (luzLinterna != null) luzLinterna.pointLightOuterRadius = 10.4f;
        if (colliderLinternaNormal != null) colliderLinternaNormal.enabled = false;
        if (colliderLinternaAmplio != null) colliderLinternaAmplio.enabled = true;
    }

    private void ApagarLinterna()
    {
        linternaActiva = false;
        if (luzLinterna != null) luzLinterna.pointLightOuterRadius = 5.2f;
        if (colliderLinternaAmplio != null) colliderLinternaAmplio.enabled = false;
        if (colliderLinternaNormal != null) colliderLinternaNormal.enabled = true;
    }
    #endregion

    #region Atributos y daño
    public void ReducirStamina(int cantidad)
    {
        stamina -= cantidad;
        if (stamina < 0) stamina = 0;
        if (barraStamina != null) barraStamina.ActualizarStamina(stamina);
    }

    public void TakeDamage(float damage)
    {
        float newHealth = maxHealth - damage;
        maxHealth = Mathf.Clamp(newHealth, 0, 100f);

        if (maxHealth <= 0)
        {
            Morir();
        }

        if (healthBar != null) healthBar.UpdateHealth(maxHealth);
    }

    /*public void RecibirDano(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            Morir();
        }
        if (barraVida != null) barraVida.ActualizarVida(vida);
    }*/

    private void Morir()
    {
        Debug.Log("Muerto");
        GameObject.Find("Barras")?.SetActive(false);
        gameObject.SetActive(false);
        SceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
    }

    /*private void Die()
    {
        GameObject.Find("Barras")?.SetActive(false);
        gameObject.SetActive(false);
    }*/

    /*private void RecibirDanoPeriodico()
    {
        RecibirDano(damage);
    }*/
    #endregion

    #region Colisiones y tienda
    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (collision.CompareTag("Enemy") && collision.IsTouching(miHitbox))
        {
            HandleEnemyCollision(collision);
        }*/
        if (collision.CompareTag("Tienda"))
        {
            canOpenShop = true;
            Debug.Log("Puedes abrir la tienda con E");
        } else {
            canOpenShop = false;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Tienda"))
        {
            canOpenShop = false;
        }
    }

    /*private void HandleEnemyCollision(Collider2D collision)
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
    }*/

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
            rb.linearVelocity = Vector2.zero;
        }
    }

    // public void Stun(float duration)
    // {
    //     throw new System.NotImplementedException();
    // }
    #endregion
}