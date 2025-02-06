using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Importar NavMesh
using System.Linq;

public class MonsterPatrolAI : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 1f;

    [Header("Patrulla")]
    [SerializeField] private Transform[] waypoints;

    private int currentWaypoint = 0;
    private bool isWaiting;
    private NavMeshAgent agent; // Referencia al NavMeshAgent
    private MonsterChaseAI chaseAI;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // Desactiva la rotación automática
        agent.updateUpAxis = false;   // Evita cambios en el eje Z (útil en 2D)
        agent.autoBraking = false; // Evita que frene entre waypoints

        // Obtener la referencia del script de persecución
        chaseAI = GetComponent<MonsterChaseAI>();

        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position; // TELETRANSPORTAR al primer waypoint
            agent.Warp(waypoints[0].position); // Asegurar que el NavMeshAgent inicie correctamente
        }

        if (waypoints.Length > 1)
        {
            ShuffleWaypoints();
        }

        MoveToNextWaypoint();
    }

    void Update()
    {
        // Si el monstruo está persiguiendo, no patrulla
        if (chaseAI != null && chaseAI.IsChasing())
        {
            return;
        }

        Patrol();
    }

    void Patrol()
    {
        if (isWaiting) return;

        // Si el monstruo llega al waypoint
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        // Pasar al siguiente waypoint
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        MoveToNextWaypoint();

        isWaiting = false;
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypoint].position);
        }
    }

    void ShuffleWaypoints()
    {
        Transform firstWaypoint = waypoints[0];
        waypoints = waypoints.Skip(1).OrderBy(x => Random.value).ToArray();
        waypoints = new Transform[] { firstWaypoint }.Concat(waypoints).ToArray();
    }
}