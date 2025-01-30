using UnityEngine;

public class LightFollowMouse : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Manté el moviment en 2D
        transform.position = mousePosition;
    }
}
