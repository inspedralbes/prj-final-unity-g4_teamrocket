using UnityEngine;
using UnityEngine.AI;

public class MonsterStalkerAI : EnemyBase
{
    [Header("Configuración")]
    [SerializeField] private Transform player;  // Referencia al jugador
    [SerializeField] private Transform respawnWaypoint;  // El waypoint específico donde respawnea el monstruo
    // [SerializeField] private float speed = 3f;  // Velocidad de persecución

    private NavMeshAgent agent;

    void Start()
    {
        damage = 2;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // Desactiva la rotación automática
        agent.updateUpAxis = false;   // Evita cambios en el eje Z (útil en 2D)
        agent.speed = speed; // Asigna la velocidad al NavMeshAgent
    }

    void Update()
    {
        ChasePlayer();
    }

    void ChasePlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vision"))  
        {
            RespawnAtWaypoint(); 
        }
    }

    void RespawnAtWaypoint()
    {
        if (respawnWaypoint != null)
        {
            agent.Warp(respawnWaypoint.position); // Mueve correctamente el NavMeshAgent
            Debug.Log("El monstruo ha respawneado en el waypoint específico.");
        }
        else
        {
            Debug.LogWarning("No se ha asignado un waypoint de respawn.");
        }
    }
}