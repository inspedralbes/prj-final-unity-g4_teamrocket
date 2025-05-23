using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class TerminadaPartida : MonoBehaviour
{
    private BoxCollider2D boxcollider;
    private GameObject[] keys;
    private GameObject salida;
    public TMP_Text textFaltaLlaves;
    void Update()
    {
        keys = GameObject.FindGameObjectsWithTag("Key");
        salida = GameObject.FindGameObjectWithTag("Salida");
        boxcollider = salida.GetComponent<BoxCollider2D>();
        if (keys.Length > 0)
        {
            boxcollider.enabled = true;
            boxcollider.isTrigger = false;
            for (int i = 0; i < keys.Length; i++)
            {
                Debug.Log("Hay " + keys.Length + " llaves");
            }
        }
        if (keys.Length == 0)
        {
            Debug.Log("activar boxcollider");
            boxcollider.enabled = true;
            boxcollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Cambiar escena");
        if (collision.CompareTag("Player") && keys.Length <= 0)
        {
            SceneManager.LoadScene("LevelComplete", LoadSceneMode.Additive);
        }
        else
        {
            textFaltaLlaves.text = $"Et falten {keys.Length} claus";
            textFaltaLlaves.gameObject.SetActive(true);
        }
    }
}
