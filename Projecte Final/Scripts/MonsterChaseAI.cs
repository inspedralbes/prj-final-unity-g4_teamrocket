using UnityEngine;

public class MonsterChaseAI : MonoBehaviour
{
    [Header("Persecución")]
    [SerializeField] private Transform player;
    [SerializeField] private float minDistance;
    [SerializeField] private float visionRange;
    [SerializeField] private float speed = 2f;

    private bool isChasing = false;

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
        if(Vector2.Distance(transform.position, player.position) > minDistance) {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else {
            Attack();
        }
    }

    public void Attack() {
        Debug.Log("Atacar");
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
