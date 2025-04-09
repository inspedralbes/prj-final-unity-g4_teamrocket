using UnityEngine;

public class MonstruoInfectante : EnemyBase
{
    public PlayerController playerController;
    public Collider2D infectanteCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && collision.collider.IsTouching(infectanteCollider))
        {
            float probabilidad = Random.Range(0,2);

            if (probabilidad == 0)
            {
                Debug.Log("Muerto");
                damage = playerController.vida + 20;
                GameObject.Find("SusProta").SetActive(false);
            }
            if(probabilidad == 1)
            {
                Debug.Log("Infectado");
                Infectar();
                
            }
            
        }
    }
    */
    
    private void Infectar()
    {
        playerController.tag = "Infected";
        Debug.Log("Infectado");
    }
    public float GetDamage()
    {
        return damage;
    }

}
