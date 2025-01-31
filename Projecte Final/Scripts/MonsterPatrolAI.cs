using System.Collections;
using UnityEngine;
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
    private MonsterChaseAI chaseAI;

    void Start()
    {
        // Obtener la referencia del script de persecución
        chaseAI = GetComponent<MonsterChaseAI>();

        if (waypoints.Length > 1)
        {
            ShuffleWaypoints();
        }
        transform.position = waypoints[0].position;
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
        if (transform.position != waypoints[currentWaypoint].position)
        {
            transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypoint].position, speed * Time.deltaTime);
        }
        else if (!isWaiting)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        currentWaypoint++;
        if (currentWaypoint == waypoints.Length)
        {
            currentWaypoint = 0;
        }
        isWaiting = false;
    }

    void ShuffleWaypoints()
    {
        Transform firstWaypoint = waypoints[0];
        waypoints = waypoints.Skip(1).OrderBy(x => Random.value).ToArray();
        waypoints = new Transform[] { firstWaypoint }.Concat(waypoints).ToArray();
    }
}
