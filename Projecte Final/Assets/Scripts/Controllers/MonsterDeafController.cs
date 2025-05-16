using Mirror;
using UnityEngine;
using System.Linq;

public class MonsterDeafController : NetworkBehaviour
{
    public float hearingThreshold = 0.01f;
    public float speed = 3f;
    public float memoryDuration = 5f;

    private Transform targetPlayer;
    private Vector3 lastHeardPosition;
    private float memoryTimer = 0f;

    [System.Obsolete]
    void Update()
    {
        if (!isServer) return;

        var players = FindObjectsOfType<PlayerMic>();
        var loudPlayers = players
            .Where(p => p.currentMicVolume > hearingThreshold)
            .OrderByDescending(p => p.currentMicVolume)
            .ToList();

        if (loudPlayers.Count > 0)
        {
            targetPlayer = loudPlayers[0].transform;
            lastHeardPosition = targetPlayer.position;
            memoryTimer = memoryDuration;

            Debug.Log("Persiguiendo jugador: " + loudPlayers[0].name);
        }
        else if (memoryTimer > 0)
        {
            memoryTimer -= Time.deltaTime;
            targetPlayer = null; // Deja de seguir directamente al jugador

            // Sigue la última posición conocida
            float distance = Vector3.Distance(transform.position, lastHeardPosition);
            if (distance > 0.1f)
            {
                Vector3 direction = (lastHeardPosition - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;

                Debug.Log("Buscando en última posición: " + lastHeardPosition);
            }
        }
        else
        {
            targetPlayer = null; // Olvida después de cierto tiempo
        }

        // Si todavía tiene a un jugador ruidoso como objetivo
        if (targetPlayer != null)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }
}