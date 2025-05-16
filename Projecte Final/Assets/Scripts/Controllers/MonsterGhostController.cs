using UnityEngine;
using System.Collections;
using System.Linq;

public class MonsterGhostController : EnemyBase
{
    public float tiempoEfecto = 5f;

    [SerializeField] private Transform[] waypoints;
    private int currentWaypoint = 0;

    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float reachDistance = 0.1f;

    private bool isWaiting = false;
    private Rigidbody2D rb;

    [System.Obsolete]
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        speed = 5f;

        GetWaypoints waypointProvider = FindObjectOfType<GetWaypoints>();
        waypoints = waypointProvider.waypoints.ToArray();

        if (waypoints.Length > 1)
        {
            ShuffleWaypoints();
        }
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        Vector2 currentPosition = rb.position;
        Vector2 targetPosition = waypoints[currentWaypoint].position;
        Vector2 direction = (targetPosition - currentPosition).normalized;

        // Calcula nueva posición
        Vector2 newPosition = currentPosition + direction * speed * Time.fixedDeltaTime;

        // Mueve el Rigidbody2D sin colisión sólida (atravesando paredes)
        rb.MovePosition(newPosition);

        // Comprueba si ha llegado al waypoint
        if (Vector2.Distance(currentPosition, targetPosition) < reachDistance)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        isWaiting = false;
    }

    void ShuffleWaypoints()
    {
        Transform first = waypoints[0];
        waypoints = waypoints.Skip(1).OrderBy(x => Random.value).ToArray();
        waypoints = new Transform[] { first }.Concat(waypoints).ToArray();
    }

    private void OnTriggerEnter2D(Collider2D other) // ✅
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.InvertirControles(tiempoEfecto);
            }
        }
    }
}