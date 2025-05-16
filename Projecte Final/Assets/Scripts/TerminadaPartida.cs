using UnityEngine.SceneManagement;
using UnityEngine;

public class TerminadaPartida : MonoBehaviour
{
    private BoxCollider2D boxcollider;
    void Update()
    {
        GameObject[] keys = GameObject.FindGameObjectsWithTag("Key");
        GameObject salida = GameObject.FindGameObjectWithTag("Salida");
        boxcollider = salida.GetComponent<BoxCollider2D>();
        if (keys.Length > 0)
        {
            boxcollider.enabled = false;
            for (int i = 0; i < keys.Length; i++)
            {
                Debug.Log("Hay " + keys.Length + " llaves");
            }
        }
        if (keys.Length == 0)
        {
            Debug.Log("activar boxcollider");
            boxcollider.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Cambiar escena");
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene("LevelComplete", LoadSceneMode.Additive);
        }
    }
}
