using Mirror;
using UnityEngine;
using System.Linq;

public class MonsterDeafController : EnemyBase
{
    public float hearingThreshold = 0.01f;
    public float memoryDuration = 5f;
    
    [Header("Configuración de Rotación")]
    [Tooltip("Activa para forzar rotación a 0 grados")]
    public bool lockRotation = true;
    [Tooltip("Rotación deseada en grados")]
    public Vector3 targetRotation = Vector3.zero;

    private Transform targetPlayer;
    private Vector3 lastHeardPosition;
    private float memoryTimer = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ForceCorrectRotation();
    }

    void Update()
    {
        if (!isServer) return;

        // Protección activa contra rotación no deseada
        if (lockRotation && transform.eulerAngles != targetRotation)
        {
            ForceCorrectRotation();
        }

        UpdateHearingBehavior();
        UpdateVisualOrientation();
    }

    void LateUpdate()
    {
        // Protección adicional después de todas las actualizaciones
        if (lockRotation)
        {
            ForceCorrectRotation();
        }
    }

    void ForceCorrectRotation()
    {
        transform.rotation = Quaternion.Euler(targetRotation);
    }

    void UpdateHearingBehavior()
    {
        var players = FindObjectsOfType<PlayerMic>()
            .Where(p => p.currentMicVolume > hearingThreshold)
            .OrderByDescending(p => p.currentMicVolume)
            .ToList();

        if (players.Count > 0)
        {
            targetPlayer = players[0].transform;
            lastHeardPosition = targetPlayer.position;
            memoryTimer = memoryDuration;
        }
        else if (memoryTimer > 0)
        {
            memoryTimer -= Time.deltaTime;
            targetPlayer = null;
            
            if (Vector3.Distance(transform.position, lastHeardPosition) > 0.1f)
            {
                transform.position += (lastHeardPosition - transform.position).normalized * speed * Time.deltaTime;
            }
        }
        else
        {
            targetPlayer = null;
        }

        if (targetPlayer != null)
        {
            transform.position += (targetPlayer.position - transform.position).normalized * speed * Time.deltaTime;
        }
    }

    void UpdateVisualOrientation()
    {
        if (spriteRenderer == null) return;

        Vector3 direction = targetPlayer != null ? 
            targetPlayer.position - transform.position : 
            Vector3.zero;

        if (direction != Vector3.zero)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isServer)
        {
            other.GetComponent<PlayerController>()?.TakeDamage(damage);
        }
    }
}