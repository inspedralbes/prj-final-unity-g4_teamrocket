using UnityEngine;
using UnityEngine.SceneManagement;

public class TerminadaPartida : MonoBehaviour
{
    public Collider2D miHitBox;
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;

    private bool SalidaActive = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(item1.activeSelf == false && item2.activeSelf == false && item3.activeSelf == false && SalidaActive)
        {
            Debug.Log("Items recolectados!");
            GameObject.Find("HabilitarSalida").SetActive(false);
            SalidaActive = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && collision.IsTouching(miHitBox))
        {
            SceneManager.LoadScene("LevelComplete", LoadSceneMode.Additive);
        }
    }
}
