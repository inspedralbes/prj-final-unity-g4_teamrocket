using UnityEngine;
using System.Collections;

public class PlayerFinder : MonoBehaviour
{
    private Transform parentTransform;
    private Transform target;
    private GameObject[] allPlayers;
    private GameObject[] validPlayers; // Jugadores v�lidos (excluyendo al padre)
    private SpriteRenderer parentSprite;

    void Start()
    {
        parentTransform = transform.parent;

        if (parentTransform != null)
        {
            parentSprite = parentTransform.GetComponent<SpriteRenderer>();
            target = parentTransform;
        }

        StartCoroutine(FindPlayersAfterDelay());
    }

    void Update()
    {
        bool parentActive = parentSprite != null && parentSprite.enabled;

        // Actualizar lista de jugadores v�lidos (excluyendo al padre)
        UpdateValidPlayers();

        // Comportamiento de seguimiento
        if (parentActive)
        {
            target = parentTransform;
        }
        else if (target == parentTransform || target == null)
        {
            // Si el padre se desactiv� y no hay objetivo v�lido
            if (validPlayers.Length > 0)
            {
                target = validPlayers[0].transform;
            }
        }

        // Cambio de objetivo con click izquierdo (solo si el padre no est� activo)
        if (!parentActive && Input.GetMouseButtonDown(0) && validPlayers.Length > 0)
        {
            if (target == null || !IsValidTarget(target.gameObject))
            {
                target = validPlayers[0].transform;
            }
            else
            {
                int currentIndex = System.Array.IndexOf(validPlayers, target.gameObject);
                int nextIndex = (currentIndex + 1) % validPlayers.Length;
                target = validPlayers[nextIndex].transform;
            }
        }

        // Movimiento de la c�mara
        if (target != null)
        {
            Vector3 newPos = target.position;
            newPos.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 5f);
        }
    }

    IEnumerator FindPlayersAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateValidPlayers();
    }

    void UpdateValidPlayers()
    {
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
        validPlayers = System.Array.FindAll(allPlayers, p => p.transform != parentTransform);
    }

    bool IsValidTarget(GameObject obj)
    {
        return obj != null && obj.transform != parentTransform;
    }
}