using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Referencia al transform del jugador
    public Vector3 offset = new Vector3(0, 0, -10); // Desplazamiento de la cámara

    void Update()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}