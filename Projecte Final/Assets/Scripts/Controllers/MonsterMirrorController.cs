using UnityEngine;

public class MonsterMirrorController : EnemyBase
{
    public float tiempoEfecto = 5f;
    // public PlayerController playerController;
    public Collider2D espejocollider;

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
        if(collision.gameObject.CompareTag("Player") && collision.collider.IsTouching(espejocollider))
        {
            playerController.InvertirControles(tiempoEfecto);
        }
    }
    */

    /*
    public void TiempoEspejoRestante(){
        tiempoRestanteInversion -= Time.deltaTime;
        if (tiempoRestanteInversion <= 0)
        {
            controlesInvertidos = false;
        }
        if (controlesInvertidos) {
            Horizontal = -Horizontal;
            Vertical = -Vertical;
        }
    }
    
    public void InvertirControles(float tiempo)
    {
        controlesInvertidos = true;
        tiempoRestanteInversion = tiempo;
    }
    */
}
