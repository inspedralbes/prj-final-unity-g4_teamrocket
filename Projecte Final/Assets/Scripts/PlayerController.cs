using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public Camera playerCameraPrefab; // Prefab de la cámara (asígnale un prefab de cámara desde el Inspector)
    public Transform conoLuzTransform; // Asigna el transform del cono de luz desde el Inspector
    public float zDistanceFromCamera = 10f; // Distancia desde la cámara
    public float rotationOffset = 90f; // Ajusta este valor según la orientación de tu sprite

    private Camera playerCamera; // Cámara instanciada para este jugador

    public override void OnStartLocalPlayer()
    {
        // Instanciar y configurar la cámara para el jugador local
        if (playerCameraPrefab != null)
        {
            // Instanciar la cámara como hijo del personaje
            playerCamera = Instantiate(playerCameraPrefab, transform);
            playerCamera.gameObject.SetActive(true);

            // Asegurarse de que la cámara no sea la cámara principal
            playerCamera.tag = "Untagged"; // No usar el tag "MainCamera"

            // Asignar un depth único basado en el connectionId del jugador
            playerCamera.depth = 0; // El jugador local tiene depth 0

            // Configurar la posición inicial de la cámara
            playerCamera.transform.localPosition = new Vector3(0, 0, -10); // Ajusta la posición Z según sea necesario
        }

        // Activar el cono de luz solo para el jugador local
        if (conoLuzTransform != null)
        {
            conoLuzTransform.gameObject.SetActive(true);
        }
    }

    void Start()
    {
        // Desactivar la cámara de otros jugadores
        if (!isLocalPlayer && playerCamera != null)
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Solo el jugador local puede mover la cámara y el cono de luz
        if (!isLocalPlayer) return;

        // Controlar el cono de luz
        if (conoLuzTransform != null)
        {
            // Obtén la posición del ratón en la pantalla
            Vector3 mousePosition = Input.mousePosition;

            // Ajusta la coordenada Z para la conversión a coordenadas del mundo
            mousePosition.z = zDistanceFromCamera;

            // Convierte la posición del ratón a coordenadas del mundo
            Vector3 worldMousePosition = playerCamera.ScreenToWorldPoint(mousePosition);
            worldMousePosition.z = 0f; // Asegúrate de que la coordenada Z sea 0 (si estás trabajando en 2D)

            // Calcula la dirección del ratón respecto a la luz
            Vector3 direction = (worldMousePosition - conoLuzTransform.position).normalized;

            // Calcula el ángulo en grados
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Ajusta el ángulo para que esté en el rango [0, 360]
            if (angle < 0) angle += 360;

            // Aplica la rotación al cono de luz
            conoLuzTransform.rotation = Quaternion.Euler(0, 0, angle - rotationOffset);
        }
    }
}