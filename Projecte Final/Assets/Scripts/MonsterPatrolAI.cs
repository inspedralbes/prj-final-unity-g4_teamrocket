using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class MonsterPatrolAI : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float waitTime = 1f;

    [Header("Patrulla")]
    [SerializeField] private Transform[] waypoints;

    private int currentWaypoint = 0;
    private bool isWaiting;
    private NavMeshAgent agent;
    private MonsterChaseAI chaseAI;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoBraking = false;

        chaseAI = GetComponent<MonsterChaseAI>();

        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            agent.Warp(waypoints[0].position);
        }

        if (waypoints.Length > 1)
        {
            ShuffleWaypoints();
        }

        MoveToNextWaypoint();
    }

    void Update()
    {
        if (chaseAI != null && chaseAI.IsChasing())
        {
            return;
        }

        Patrol();
    }

    void Patrol()
    {
        if (isWaiting) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        MoveToNextWaypoint();

        isWaiting = false;
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypoint].position);

            chaseAI.UpdateVisionAngleToWaypoint(waypoints[currentWaypoint].position);
        }
    }

    void ShuffleWaypoints()
    {
        Transform firstWaypoint = waypoints[0];
        waypoints = waypoints.Skip(1).OrderBy(x => Random.value).ToArray();
        waypoints = new Transform[] { firstWaypoint }.Concat(waypoints).ToArray();
    }
}
