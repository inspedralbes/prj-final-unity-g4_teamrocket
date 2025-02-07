using UnityEngine;

public class PlayerVision : MonoBehaviour
{
    public Transform visionLight; // Referencia a la luz
    public float zDistanceFromCamera = 10f; // Distancia desde la cámara
    public float rotationOffset = 90f; // Ajusta este valor según la orientación de tu sprite

    void Update()
    {
        // Obtén la posición del ratón en la pantalla
        Vector3 mousePosition = Input.mousePosition;

        // Ajusta la coordenada Z para la conversión a coordenadas del mundo
        mousePosition.z = zDistanceFromCamera;

        // Convierte la posición del ratón a coordenadas del mundo
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldMousePosition.z = 0f; // Asegúrate de que la coordenada Z sea 0 (si estás trabajando en 2D)

        // Calcula la dirección del ratón respecto a la luz
        Vector3 direction = (worldMousePosition - visionLight.position).normalized;

        // Calcula el ángulo en grados
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ajusta el ángulo para que esté en el rango [0, 360]
        if (angle < 0) angle += 360;

        visionLight.transform.rotation = Quaternion.Euler(0, 0, angle - rotationOffset);
    }
}