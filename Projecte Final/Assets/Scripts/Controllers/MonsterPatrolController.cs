using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;

public class MonsterPatrolController : EnemyBase
{
    [Header("Player Detection")]
    public Transform player;
    public float minDistance = 1.5f;
    public float visionRange = 5f;
    public float visionAngle = 60f;
    public float detectionRadius = 2f;
    public float currentAngle = 0f;

    [Header("Patrol")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waitTime = 1f;
    private int currentWaypoint = 0;
    private bool isWaiting;

    private NavMeshAgent agent;
    private bool isChasing = false;
    private Coroutine damageCoroutine;

    void Start()
    {
        damage = 10;
        speed = 10f;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
        agent.updatePosition = true;
        
        // Fuerza rotación correcta inicial
        transform.rotation = Quaternion.identity;

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
        if (agent.speed != speed) // Mantiene la velocidad constante
        {
            agent.speed = speed;
        }
        Vector2 playerDirection = player.position - transform.position;

        // Si el jugador está dentro del cono o dentro del collider, sigue persiguiendo
        if (IsPlayerInVisionCone(playerDirection) || IsPlayerInDetectionRadius())
        {
            isChasing = true;
            UpdateVisionAngle(playerDirection);
            ChasePlayer();
        }
        else
        {
            isChasing = false;
            Patrol();
        }
    }

    // --- Patrol
    void Patrol()
    {
        if (isWaiting || isChasing) return;

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

            UpdateVisionAngleToWaypoint(waypoints[currentWaypoint].position);
        }
    }

    void ShuffleWaypoints()
    {
        Transform firstWaypoint = waypoints[0];
        waypoints = waypoints.Skip(1).OrderBy(x => Random.value).ToArray();
        waypoints = new Transform[] { firstWaypoint }.Concat(waypoints).ToArray();
    }

    // --- Chase
    void ChasePlayer()
    {
        if (Vector2.Distance(transform.position, player.position) > minDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            // Attack();
        }
    }

    // void Attack()
    // {
    //     Debug.Log("¡Atacar al jugador!");
    // }

    // Verifica si el jugador está dentro del círculo de detección
    bool IsPlayerInDetectionRadius()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject p in players)
        {
            float distance = Vector2.Distance(transform.position, p.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = p;
            }
        }

        if (closestPlayer != null)
        {
            player = closestPlayer.transform;
            return closestDistance <= detectionRadius;
        }

        return false;
    }


    public void UpdateVisionAngleToWaypoint(Vector2 waypointPosition)
    {
        Vector2 direction = (waypointPosition - (Vector2)transform.position).normalized;
        currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }


    // Comprobar si el jugador está dentro del cono de visión
    bool IsPlayerInVisionCone(Vector2 playerDirection)
    {
        float angleDifference = Vector2.Angle(GetDirectionFromAngle(currentAngle), playerDirection);
        return angleDifference < visionAngle / 2f && playerDirection.magnitude <= visionRange;
    }

    // Actualizar el ángulo del cono si el jugador está dentro
    void UpdateVisionAngle(Vector2 playerDirection)
    {
        currentAngle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;
    }

    // Convertir un ángulo en un vector de dirección
    Vector2 GetDirectionFromAngle(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    // Dibujar el Cono de Visión en el Editor (Gizmo)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Dibuja el círculo de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Líneas del cono de visión
        Vector2 centerDir = GetDirectionFromAngle(currentAngle) * visionRange;
        Vector2 leftAngle = Quaternion.Euler(0, 0, -visionAngle / 2) * centerDir;
        Vector2 rightAngle = Quaternion.Euler(0, 0, visionAngle / 2) * centerDir;

        Gizmos.DrawLine(transform.position, (Vector2)transform.position + leftAngle);
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + rightAngle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                damageCoroutine = StartCoroutine(DealDamageOverTime(player));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
        }
    }

    IEnumerator DealDamageOverTime(PlayerController player)
    {
        while (true)
        {
            player.TakeDamage(damage);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
