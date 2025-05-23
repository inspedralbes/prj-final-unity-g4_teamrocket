using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class MonsterFreezeController : EnemyBase
{
    [Header("Configuración")]
    public float minDistance = 2f;
    public float freezeDuration = 3f;
    
    private NavMeshAgent agent;
    private Transform currentTarget;
    private Animator animator;
    private bool isFrozen = false;
    private float freezeEndTime;

    void Start() 
    {
        damage = 100;
        speed = 3f;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
    }

    void Update()
    {
        if (isFrozen)
        {
            // Verificar si debe descongelarse
            if (Time.time >= freezeEndTime)
            {
                UnfreezeMonster();
            }
        }
        else
        {
            // Perseguir al jugador más cercano
            UpdateClosestPlayerTarget();
            
            if (currentTarget != null)
            {
                agent.SetDestination(currentTarget.position);
            }
        }
    }

    private void UpdateClosestPlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players == null || players.Length == 0) 
        {
            currentTarget = null;
            return;
        }

        currentTarget = players
            .Where(p => p != null && p.activeInHierarchy)
            .OrderBy(p => Vector2.Distance(transform.position, p.transform.position))
            .FirstOrDefault()?.transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Vision"))
        {
            FreezeMonster();
            
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerController>()?.TakeDamage(damage);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if ((other.CompareTag("Vision") || other.CompareTag("Player")) && !isFrozen)
        {
            FreezeMonster();
        }
    }

    void FreezeMonster()
    {
        if (isFrozen) return;

        // Detener movimiento
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        
        // Pausar animación
        if (animator != null)
        {
            animator.speed = 0f; // Congela la animación en el frame actual
        }
        
        isFrozen = true;
        freezeEndTime = Time.time + freezeDuration;
    }

    void UnfreezeMonster()
    {
        // Reanudar movimiento
        agent.isStopped = false;
        
        // Reanudar animación
        if (animator != null)
        {
            animator.speed = 1f; // Vuelve a la velocidad normal
        }
        
        isFrozen = false;
    }
}