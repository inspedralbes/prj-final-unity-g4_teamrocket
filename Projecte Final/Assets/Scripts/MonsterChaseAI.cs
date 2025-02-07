using UnityEngine;

public class MonsterChaseAI : MonoBehaviour
{
    [Header("Persecución")]
    [SerializeField] private Transform player; // Referencia al jugador
    [SerializeField] private float minDistance = 1.5f; // Distancia mínima para atacar
    [SerializeField] private float visionRange = 5f; // Rango de visión

    private bool isChasing = false;
    private UnityEngine.AI.NavMeshAgent agent; // Referencia al NavMeshAgent

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updateRotation = false; // Desactiva la rotación automática
        agent.updateUpAxis = false;   // Evita cambios en el eje Z (útil en 2D)
    }

    void Update()
    {
        // Verificar si el jugador está dentro del rango de visión
        if (Vector2.Distance(transform.position, player.position) <= visionRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        // Usamos el NavMeshAgent para perseguir al jugador
        if (Vector2.Distance(transform.position, player.position) > minDistance)
        {
            agent.SetDestination(player.position); // Establecer la posición del jugador como destino
        }
        else
        {
            Attack();
        }
    }

    public void Attack()
    {
        Debug.Log("¡Atacar al jugador!");
    }

    public bool IsChasing()
    {
        return isChasing;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}