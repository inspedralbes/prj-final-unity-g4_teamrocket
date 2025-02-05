using UnityEngine;

public class StalkerAI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform player;  // Referencia al jugador
    [SerializeField] private Transform respawnWaypoint;  // El waypoint específico donde respawnea el monstruo
    [SerializeField] private float speed = 3f;  // Velocidad de persecución

    void Update()
    {
        // El monstruo sigue al jugador
        ChasePlayer();
    }

    void ChasePlayer()
    {
        // El monstruo sigue al jugador
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
    }

    // Detecta la colisión con el jugador
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // Asegúrate de que el jugador tenga la etiqueta "Player"
        {
            RespawnAtWaypoint();  // Si el monstruo toca al jugador, respawnea en el waypoint
        }
    }

    void RespawnAtWaypoint()
    {
        // Teletransportar al monstruo al waypoint específico
        transform.position = respawnWaypoint.position;
        Debug.Log("El monstruo ha respawneado en el waypoint específico.");
    }
}