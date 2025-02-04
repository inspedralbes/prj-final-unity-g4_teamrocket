using UnityEngine;

public class VisionController : MonoBehaviour
{
    public float lightAngle = 45f; // Ángulo de visión (puedes ajustarlo en el Inspector)
    public LayerMask wallLayer; // Capa que representa las paredes
    public int rayCount = 36; // Número de rayos para dividir el cono (aumentado para mayor precisión)
    public float visionRadius = 10f; // Radio máximo de la visión
    private Mesh visionMesh; // Malla para el cono de visión

    void Start()
    {
        // Crea una malla para el cono de visión
        visionMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = visionMesh;
    }

    void Update()
    {
        // Obtén la posición del ratón en la pantalla
        Vector3 mousePosition = Input.mousePosition;

        // Convierte la posición del ratón a coordenadas del mundo
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
        worldMousePosition.z = 0f; // Asegúrate de que la coordenada Z sea 0 (si estás trabajando en 2D)

        // Calcula la dirección del ratón respecto al centro de la pantalla
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Vector3 direction = (mousePosition - screenCenter).normalized;

        // Calcula el ángulo en grados
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ajusta el ángulo para que esté en el rango [0, 360]
        if (angle < 0) angle += 360;

        // Genera la malla del cono de visión
        GenerateVisionMesh(angle);
    }

    void GenerateVisionMesh(float centerAngle)
    {
        // Crea los vértices y triángulos de la malla
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        // El primer vértice es el centro del cono
        vertices[0] = Vector3.zero;

        // Calcula los vértices del cono
        float halfAngle = lightAngle / 2f;
        float angleStep = lightAngle / (rayCount - 1);

        for (int i = 0; i < rayCount; i++)
        {
            // Calcula el ángulo para este rayo
            float currentAngle = centerAngle - halfAngle + angleStep * i;

            // Convierte el ángulo a una dirección
            Vector3 direction = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad), 0f);

            // Lanza un rayo para detectar colisiones con las paredes
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, visionRadius, wallLayer);

            // Si el rayo choca con una pared, usa la distancia de la colisión
            if (hit.collider != null)
            {
                vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                // Si no hay pared, usa la distancia máxima
                vertices[i + 1] = direction * visionRadius;
            }

            // Depuración: Dibuja los rayos en la escena
            Debug.DrawRay(transform.position, direction * visionRadius, Color.red);
        }

        // Crea los triángulos de la malla
        for (int i = 0; i < rayCount - 1; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        // Asigna los vértices y triángulos a la malla
        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
    }
}