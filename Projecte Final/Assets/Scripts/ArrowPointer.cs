using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    void Update()
    {
        GameObject closestItem = FindClosestWithTag("Item");

        GameObject target = closestItem;

        if (target == null)
        {
            // Si no hay items, busca el objeto con tag Finish
            target = GameObject.FindWithTag("Finish");
        }

        if (target != null)
        {
            // Calcula la dirección hacia el objetivo
            Vector3 direction = target.transform.position - transform.position;

            // Calcula el ángulo y rota la flecha (en 2D sobre el eje Z)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    GameObject FindClosestWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        foreach (GameObject obj in objects)
        {
            float dist = Vector3.Distance(currentPos, obj.transform.position);
            if (dist < minDistance)
            {
                closest = obj;
                minDistance = dist;
            }
        }

        return closest;
    }
}
