using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D Rigidbody2D;
    private float Horizontal;
    private float Vertical;
    private float Speed = 5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Horizontal = Input.GetAxisRaw("Horizontal");
        Vertical = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal * Speed, Vertical * Speed);
    }
}
