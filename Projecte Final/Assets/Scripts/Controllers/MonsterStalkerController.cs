using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class MonsterStalkerController : EnemyBase
{
    private NavMeshAgent agent;
    private Transform currentTarget;
    private float targetUpdateCooldown = 2f;
    private float lastTargetUpdateTime;
    
    // Sistema de waypoints
    private GetWaypoints waypointProvider;
    private Transform[] waypoints;
    private Transform currentWaypoint;

    void Start()
    {
        damage = 50;
        speed = 3f;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
        
        // Inicializar sistema de waypoints
        waypointProvider = FindObjectOfType<GetWaypoints>();
        if (waypointProvider != null)
        {
            waypoints = waypointProvider.waypoints.ToArray();
            SelectRandomWaypoint();
        }
        else
        {
            Debug.LogError("No se encontró GetWaypoints en la escena");
        }
        
        UpdateRandomPlayerTarget();
        lastTargetUpdateTime = Time.time;
    }

    void Update()
    {
        if (Time.time - lastTargetUpdateTime > targetUpdateCooldown)
        {
            UpdateRandomPlayerTarget();
            lastTargetUpdateTime = Time.time;
        }

        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
    }

    private void SelectRandomWaypoint()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            int randomIndex = Random.Range(0, waypoints.Length);
            currentWaypoint = waypoints[randomIndex];
            transform.position = currentWaypoint.position;
            agent.Warp(currentWaypoint.position);
        }
    }

    private void UpdateRandomPlayerTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        var validPlayers = players.Where(p => 
            p.activeInHierarchy && 
            p.GetComponent<SpriteRenderer>()?.enabled == true).ToArray();

        if (validPlayers.Length > 0)
        {
            int randomIndex = Random.Range(0, validPlayers.Length);
            currentTarget = validPlayers[randomIndex].transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Vision"))  
        {
            RespawnAtRandomWaypoint(); 
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

    void RespawnAtRandomWaypoint()
    {
        if (waypointProvider != null && waypoints.Length > 0)
        {
            SelectRandomWaypoint();
            Debug.Log("Monstruo respawneado en waypoint aleatorio");
            
            // Buscar nuevo objetivo inmediatamente
            UpdateRandomPlayerTarget();
        }
        else
        {
            Debug.LogWarning("No hay waypoints disponibles para respawn");
        }
    }
}