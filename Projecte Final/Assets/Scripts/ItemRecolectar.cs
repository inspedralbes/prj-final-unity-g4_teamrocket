using UnityEngine;

public class ItemRecolectar : MonoBehaviour
{
    public Collider2D item;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && collision.IsTouching(item)){
            
            gameObject.SetActive(false);
        }
    }
}
