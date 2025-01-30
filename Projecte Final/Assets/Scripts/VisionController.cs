using UnityEngine;

public class VisionController : MonoBehaviour
{
    public float lightAngle = 45f; // Ángulo de visión (puedes ajustarlo en el Inspector)
    private Material visionMaterial; // Material que usa el shader

    void Start()
    {
        // Obtén el material del objeto que usa el shader
        visionMaterial = GetComponent<Renderer>().material;
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

        // Pasa el ángulo de dirección al shader
        visionMaterial.SetFloat("_LightDirection", angle);

        // Pasa el ángulo de visión al shader (opcional, si quieres ajustarlo dinámicamente)
        visionMaterial.SetFloat("_LightAngle", lightAngle);
        Debug.Log("Mouse Position: " + mousePosition);
        Debug.Log("World Mouse Position: " + worldMousePosition);
        Debug.Log("Angle: " + angle);
    }
}