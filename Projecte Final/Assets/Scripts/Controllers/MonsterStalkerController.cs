using UnityEngine;
using UnityEngine.AI;

public class MonsterStalkerController : EnemyBase
{
    public Transform player;  // Referencia al jugador
    public Transform respawnWaypoint;  // El waypoint específico donde respawnea el monstruo

    private NavMeshAgent agent;

    void Start()
    {
        damage = 50;
        speed = 3f;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
    }

    void Update()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        } else {
            Debug.Log("No existo");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vision"))  
        {
            RespawnAtWaypoint(); 
        }

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
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