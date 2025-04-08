using UnityEngine;
using UnityEngine.AI;

public class MonsterChaseAI : EnemigoBase
{
    [Header("Persecución")]
    [SerializeField] private Transform player; // Referencia al jugador
    [SerializeField] private float minDistance = 1.5f; // Distancia mínima para atacar
    [SerializeField] private float visionRange = 5f; // Rango de visión
    [SerializeField] private float visionAngle = 60f; // Ángulo del cono de visión
    [SerializeField] private float detectionRadius = 2f; // Rango de detección circular

    private bool isChasing = false;
    private NavMeshAgent agent;
    private float currentAngle = 0f;
    private bool followingPlayer = false;
    private bool playerInsideCollider = false; // Si el jugador está dentro del collider

    void Start()
    {
        damage = 3;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        Vector2 playerDirection = player.position - transform.position;

        // Si el jugador está dentro del cono o dentro del collider, sigue persiguiendo
        if (IsPlayerInVisionCone(playerDirection) || IsPlayerInDetectionRadius() || playerInsideCollider)
        {
            followingPlayer = true;
            isChasing = true;
            UpdateVisionAngle(playerDirection);
            ChasePlayer();
        }
        else
        {
            isChasing = false;
            followingPlayer = false;
        }
    }

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
        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    public bool IsChasing()
    {
        return isChasing;
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
        if (followingPlayer)
        {
            currentAngle = Mathf.Atan2(playerDirection.y, playerDirection.x) * Mathf.Rad2Deg;
        }
    }

    // Convertir un ángulo en un vector de dirección
    Vector2 GetDirectionFromAngle(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    // Manejar colisiones con el collider de detección
    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Player"))
    //     {
    //         playerInsideCollider = true; // Detectó al jugador dentro del collider
    //     }
    // }

    // private void OnTriggerExit2D(Collider2D collision)
    // {
    //     if (collision.CompareTag("Player"))
    //     {
    //         playerInsideCollider = false; // El jugador salió del collider
    //     }
    // }

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
}
