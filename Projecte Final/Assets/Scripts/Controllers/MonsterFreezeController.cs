using UnityEngine;
using UnityEngine.AI;

public class MonsterFreezeController : EnemyBase
{
    [Header("Configuración")]
    public Transform player;  // Referencia al jugador
    public float minDistance = 2f;  // Distancia mínima para volver a moverse

    private NavMeshAgent agent;

    void Start() 
    {
        damage = 100;
        speed = 3f;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
    }

    void Update()
    {
        if (!agent.isStopped)  // Si el agente no está detenido, persigue al jugador
        {
            ChasePlayer();
        }
        else
        {
            // Verificar si el monstruo puede volver a perseguir al jugador
            CheckIfPlayerIsFar();
        }
    }

    void ChasePlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    // Detecta la colisión con el jugador
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // Asegúrate de que el jugador tenga la etiqueta "Player"
        {
            FreezeMonster();  // Congelar el monstruo cuando toque al jugador

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }

        if (other.CompareTag("Vision"))  // Si deseas que la visión sea un trigger que active el congelamiento
        {
            FreezeMonster();  // Congelar al monstruo cuando detecte al jugador
        }
    }

    void FreezeMonster()
    {
        agent.isStopped = true;  // Detener el movimiento del agente
        agent.velocity = Vector3.zero;  // Asegura que el agente no tenga velocidad
        Debug.Log("El monstruo se ha quedado quieto al tocar al jugador.");
    }

    void CheckIfPlayerIsFar()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > minDistance)  // Si el jugador se aleja más allá de la distancia mínima
        {
            agent.isStopped = false;  // El monstruo comienza a moverse de nuevo
            Debug.Log("El monstruo comienza a perseguir nuevamente.");
        }
    }
}