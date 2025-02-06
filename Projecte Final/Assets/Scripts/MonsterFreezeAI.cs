using UnityEngine;

public class MonsterFreezeAI : MonoBehaviour
{
    // Arreglar

    [Header("Configuración")]
    [SerializeField] private Transform player;  // Referencia al jugador
    [SerializeField] private float speed = 3f;  // Velocidad de persecución
    [SerializeField] private float minDistance = 2f;  // Distancia mínima para volver a moverse

    private bool isFrozen = false;  // Variable para saber si el monstruo está congelado

    void Update()
    {
        if (!isFrozen)  // Si el monstruo no está congelado, persigue al jugador
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
        // El monstruo sigue al jugador
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
    }

    // Detecta la colisión con el jugador
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // Asegúrate de que el jugador tenga la etiqueta "Player"
        {
            FreezeMonster();  // Congelar el monstruo cuando toque al jugador
        }
    }

    void FreezeMonster()
    {
        isFrozen = true;  // Detener el movimiento del monstruo
        Debug.Log("El monstruo se ha quedado quieto al tocar al jugador.");
    }

    void CheckIfPlayerIsFar()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > minDistance)  // Si el jugador se aleja más allá de la distancia mínima
        {
            isFrozen = false;  // El monstruo comienza a moverse de nuevo
            Debug.Log("El monstruo comienza a perseguir nuevamente.");
        }
    }
}
