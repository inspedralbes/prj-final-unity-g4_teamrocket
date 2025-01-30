using UnityEngine;
using UnityEngine.InputSystem; // Importar el nuevo sistema de entrada

public class playerMovement : MonoBehaviour
{
    public float speed = 5f; // Velocidad de movimiento
    private Vector2 inputVector; // Vector de entrada

    // Método llamado por el Input System para leer el movimiento
    public void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>(); // Leer el input como un Vector2
    }

    void Update()
    {
        // Aplicar movimiento basado en el input
        Vector3 movement = new Vector3(inputVector.x, inputVector.y, 0f);
        transform.position += movement * speed * Time.deltaTime;
    }
}
