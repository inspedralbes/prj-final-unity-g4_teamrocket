using UnityEngine;
using Mirror;

public class PlayerVision : NetworkBehaviour
{
    public Transform visionLight; // Referencia a la luz
    public float zDistanceFromCamera = 10f; // Distancia desde la cámara
    public float rotationOffset = 90f; // Ajusta este valor según la orientación de tu sprite
    
    private Camera playerCamera;
    private AudioListener playerAudioListener;

    void Start()
    {
        // Solo configuramos la cámara para el jugador local
        if (isLocalPlayer)
        {
            // Buscamos la cámara en los hijos del jugador
            playerCamera = GetComponentInChildren<Camera>();
            playerAudioListener = GetComponentInChildren<AudioListener>();
            
            // Activamos la cámara y audio listener solo para el jugador local
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                
                // Asegurarse de que solo hay un AudioListener en la escena
                if (playerAudioListener != null)
                {
                    playerAudioListener.enabled = true;
                }
            }
        }
        else
        {
            // Desactivamos la cámara si no somos el jugador local
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.gameObject.SetActive(false);
            
            AudioListener audioLis = GetComponentInChildren<AudioListener>();
            if (audioLis != null) audioLis.enabled = false;
        }
    }

    void Update()
    {
        // Solo procesamos la visión para el jugador local
        if (!isLocalPlayer) return;

        if (playerCamera == null) 
        {
            // Intentamos encontrar la cámara nuevamente por si no se encontró al inicio
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null) return;
        }

        // Obtén la posición del ratón en la pantalla
        Vector3 mousePosition = Input.mousePosition;

        // Ajusta la coordenada Z para la conversión a coordenadas del mundo
        mousePosition.z = zDistanceFromCamera;

        // Convierte la posición del ratón a coordenadas del mundo usando la cámara del jugador
        Vector3 worldMousePosition = playerCamera.ScreenToWorldPoint(mousePosition);
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