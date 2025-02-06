using UnityEngine;
using UnityEngine.Rendering.Universal; // Importar Light2D

public class PlayerVision : MonoBehaviour
{
    public Light2D visionLight;
    
    void Update()
    {
        // Obtén la posición del ratón en la pantalla
        Vector3 mousePosition = Input.mousePosition;

        // Convierte la posición del ratón a coordenadas del mundo
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0f));
        worldMousePosition.z = 0f; // Asegúrate de que la coordenada Z sea 0 (si estás trabajando en 2D)

        // Calcula la dirección del ratón respecto al objeto (en este caso, la luz del jugador o la posición central de la cámara)
        Vector3 direction = (worldMousePosition - visionLight.transform.position).normalized;

        // Calcula el ángulo en grados
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ajusta el ángulo para que esté en el rango [0, 360]
        if (angle < 0) angle += 360;

        // Rota la luz 2D hacia la posición del ratón
        visionLight.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
