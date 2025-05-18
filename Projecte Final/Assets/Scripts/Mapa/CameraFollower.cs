using UnityEngine;
using System.Collections;

public class PlayerFinder : MonoBehaviour
{
    private GameObject[] players;

    private int currentPlayerIndex = 0;
    private Transform target;
    public int initialPlayers { get; private set; } = 0;

    void Start()
    {
        StartCoroutine(FindPlayersAfterDelay());
    }

    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 0 && target == null)
        {
            target = players[0].transform;
        }

        if (players.Length < initialPlayers)
        {
            // Si target es null o el jugador actual fue destruido, asigna el primero válido
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                currentPlayerIndex = 0;
                target = players[currentPlayerIndex].transform;
            }

            // Cambiar jugador con click izquierdo
            if (Input.GetMouseButtonDown(0))
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
                target = players[currentPlayerIndex].transform;
            }
        }

        // Mover cámara suavemente hacia el jugador actual
        if (target != null)
        {
            Vector3 newPos = target.position;
            newPos.z = Camera.main.transform.position.z;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPos, Time.deltaTime * 5f);
        }
    }

    IEnumerator FindPlayersAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        initialPlayers = players.Length;
        Debug.Log($"Número de objetos con tag 'Player': {players.Length}");

        foreach (GameObject player in players)
        {
            Debug.Log($"Encontrado: {player.name}");
        }

        if (players.Length > 0)
        {
            target = players[0].transform; // Inicializar el target al primero
        }
    }
}
