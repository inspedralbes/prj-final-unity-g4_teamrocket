using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using Mirror;

public class PlayerController : NetworkBehaviour, IStunnable
{
    // Movimiento y física
    public Rigidbody2D rb;
    public float horizontal;
    public float vertical;
    public float speed = 5f;
    public float speedBase = 5f;
    public float speedBoost = 10f;
    
    // Atributos del jugador
    //public int enemigosTocandoHitbox = 0;
    //public int damage = 1;
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
    public bool puedeAtacar = false;
    public GameObject bate;
    public int numBatazos = 0;
    
    // Tienda
    [SerializeField] public ShopController shopController;
    public bool canMove = true;
    public bool canOpenShop = true;
    public int money = 1000;
    
    // Linterna
    public bool flashing = false;
    public bool linternaActiva = false;
    public Light2D luzLinterna;
    public Collider2D colliderLinternaNormal;
    public Collider2D colliderLinternaAmplio;
    public float tiempoLinternaEncendida = 0f;
    public float duracionMaximaLinterna = 180f;
    public float tiempoMostrarLinterna;
    public TMP_Text textTempsLinterna;
    
    // Brújula
    public bool brujula = false;
    public GameObject objetoBrujula;

    // Añade estas nuevas variables para el stun
    public bool isStunned = false;
    public float stunTimer = 0f;
    public Coroutine stunCoroutine;
    [SerializeField] public GameObject stunEffect;
    public TMP_Text textUsosBate;

    public Animator animator;
    public Vector2 lastDirection;
    public bool forceAnimationUpdate;

    [SyncVar] public bool isDead = false;
    public static bool gameOverChecked = false;

    void Start()
    {
        // Evita que el GameObject se destruya al cargar una nueva escena
        DontDestroyOnLoad(gameObject);

        canOpenShop = false;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animator.Update(0f); // Fuerza una actualización inmediata
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Vertical", 0);
        animator.SetFloat("Speed", 0);
        tiempoMostrarLinterna = duracionMaximaLinterna;
            
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

    public void OnDestroy()
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

        // Sistema redundante (seguridad para casos donde Morir() no se llamó correctamente)
        if (isDead && !gameOverChecked && !CheckForLivingPlayers())
        {
            LoadGameOver();
        }
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
        canMove = false;
        
        // Iniciar corutina para el stun
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine());
    }

    public IEnumerator StunRoutine()
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
        canMove = true;
    }

    public void HandleMovementInput()
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

    public void HandleShopInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && canOpenShop)
        {
            ToggleShop();
        }
    }

    public void HandleEquipmentInput()
    {
        // Linterna
        if (flashing && Input.GetKeyDown(KeyCode.F))
        {
            ToggleLinterna();
        }

        if (linternaActiva)
        {
            tiempoLinternaEncendida += Time.deltaTime;
            tiempoMostrarLinterna -= Time.deltaTime;
            textTempsLinterna.text = $"{tiempoMostrarLinterna:F1}";
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
        if (numBatazos > 0 && puedeAtacar && Input.GetMouseButtonDown(0))
        {
            puedeAtacar = false;
            numBatazos--;
            textUsosBate.text = $"{numBatazos}";
            if (bate != null) bate.SetActive(true);
        }
    }

    public void FixedUpdate()
    {
        if (!canMove || isStunned) return;
        rb.linearVelocity = new Vector2(horizontal * speed, vertical * speed);
    }

    #region Brújula
    public void ActivateBrujula()
    {
        if (objetoBrujula != null)
        {
            objetoBrujula.SetActive(true);
        }
    }

    public void DesactivarBrujula()
    {
        if (objetoBrujula != null)
        {
            objetoBrujula.SetActive(false);
        }
    }
    #endregion

    #region Linterna
    public void ToggleLinterna()
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

    public void EncenderLinterna()
    {
        linternaActiva = true;
        if (luzLinterna != null) luzLinterna.pointLightOuterRadius = 10.4f;
        if (colliderLinternaNormal != null) colliderLinternaNormal.enabled = false;
        if (colliderLinternaAmplio != null) colliderLinternaAmplio.enabled = true;
    }

    public void ApagarLinterna()
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

    public void Morir()
    {
        if (isDead) return;
        
        Debug.Log("Jugador muerto - Verificando estado del juego");
        isDead = true;
        
        // Desactivar componentes
        canMove = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        
        // Desactivar hijos específicos
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == "Light2D" || child.name == "ConoLuz" || 
                child.name == "Canvas" || child.name == "MicManager" || 
                child.name == "Brujula")
            {
                child.gameObject.SetActive(false);
            }
        }
        
        GameObject.Find("Barras")?.SetActive(false);
        
        // Primero verificar si hay otros jugadores vivos
        if (CheckForLivingPlayers())
        {
            // Si hay jugadores vivos, activar sistema de cámaras
            StartCoroutine(SetupCameraSystem());
        }
        else
        {
            // Si no hay jugadores vivos, cargar GameOver inmediatamente
            LoadGameOver();
        }
    }

    private bool CheckForLivingPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            // Verificar si el jugador está activo y visible
            if (player != this.gameObject && 
                player.activeInHierarchy && 
                player.GetComponent<SpriteRenderer>()?.enabled == true)
            {
                Debug.Log($"Jugador {player.name} todavía está vivo");
                return true;
            }
        }
        return false;
    }

    private void LoadGameOver()
    {
        if (gameOverChecked) return;
        
        gameOverChecked = true;
        Debug.Log("Todos los jugadores muertos - Cargando GameOver");
        SceneManager.LoadScene("GameOver", LoadSceneMode.Additive);
        
        // Desactivar todas las cámaras
        foreach (Camera cam in Camera.allCameras)
        {
            cam.enabled = false;
        }
    }

    private IEnumerator SetupCameraSystem()
    {
        yield return new WaitForEndOfFrame();
        
        // Verificar nuevamente por si acaso el estado cambió
        if (!CheckForLivingPlayers())
        {
            LoadGameOver();
            yield break;
        }
        
        Camera[] playerCameras = FindFirstObjectsByType<Camera>();
        List<Camera> validCameras = new List<Camera>();
        
        foreach (Camera cam in playerCameras)
        {
            if (cam.CompareTag("MainCamera") && cam.enabled)
            {
                validCameras.Add(cam);
            }
        }
        
        if (validCameras.Count == 0) yield break;
        
        int currentCameraIndex = 0;
        SetActiveCamera(validCameras, currentCameraIndex);
        
        while (isDead && !gameOverChecked)
        {
            // Verificar periódicamente si aún hay jugadores vivos
            if (!CheckForLivingPlayers())
            {
                LoadGameOver();
                yield break;
            }
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                currentCameraIndex--;
                if (currentCameraIndex < 0) currentCameraIndex = validCameras.Count - 1;
                SetActiveCamera(validCameras, currentCameraIndex);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                currentCameraIndex++;
                if (currentCameraIndex >= validCameras.Count) currentCameraIndex = 0;
                SetActiveCamera(validCameras, currentCameraIndex);
            }
            
            yield return null;
        }
    }
    
    private void SetActiveCamera(List<Camera> cameras, int index)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].enabled = (i == index);
        }
    }

    /*public void Die()
    {
        GameObject.Find("Barras")?.SetActive(false);
        gameObject.SetActive(false);
    }*/

    /*public void RecibirDanoPeriodico()
    {
        RecibirDano(damage);
    }*/
    #endregion

    #region Colisiones y tienda
    public void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (collision.CompareTag("Enemy") && collision.IsTouching(miHitbox))
        {
            HandleEnemyCollision(collision);
        }*/
        if (collision.CompareTag("Tienda"))
        {
            canOpenShop = true;
            Debug.Log("Puedes abrir la tienda con E");
        }
        else
        {
            canOpenShop = false;
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Tienda"))
        {
            canOpenShop = false;
        }
    }

    /*public void HandleEnemyCollision(Collider2D collision)
    {
        enemigosTocandoHitbox++;
        EnemigoBase enemigo = collision.GetComponent<EnemigoBase>();
        if (enemigo != null)
        {
            damage = enemigo.damage;
        }
        InvokeRepeating("RecibirDanoPeriodico", 0f, tiempoEntreGolpes);
    }

    public void OnTriggerExit2D(Collider2D collision)
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

    public void ToggleShop()
    {
        if (shopController != null)
        {
            bool newState = !shopController.IsShopVisible();
            shopController.ToggleShop(newState);
        }
    }

    public void HandleShopToggle(bool isOpen)
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