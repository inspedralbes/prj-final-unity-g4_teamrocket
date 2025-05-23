using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    private float Horizontal;
    private float Vertical;
    private float Speed = 10f;

    private bool controlesInvertidos = false;
    private float tiempoRestanteInversion = 0f;

    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Controles normales
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical = Input.GetAxisRaw("Vertical");

        // Invertir si el efecto está activo
        if (controlesInvertidos)
        {
            Horizontal *= -1;
            Vertical *= -1;

            tiempoRestanteInversion -= Time.deltaTime;
            if (tiempoRestanteInversion <= 0)
            {
                controlesInvertidos = false;
            }
        }
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Vertical * Speed);
    }

    public void InvertirControles(float tiempo)
    {
        controlesInvertidos = true;
        tiempoRestanteInversion = tiempo;
    }
}