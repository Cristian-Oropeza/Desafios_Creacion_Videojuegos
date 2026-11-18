using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlayer : MonoBehaviour
{
    ControlesPlayer controles;
    Vector2 movimiento;
    Rigidbody2D rb;
    private float salto = 5.0F;

    Animator animator;

    
    void Awake()
    {
        controles = new ControlesPlayer();
        movimiento = new Vector2();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        movimiento = controles.Player.Move.ReadValue<Vector2>();
        this.CambioEstado();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(movimiento.x * 5f, rb.linearVelocity.y);
    }

    private void CambioEstado()
    {
        if (movimiento.x > 0)
        {
            animator.SetInteger("PlayerEstado", 1);
        } else if (movimiento.x < 0)
        {
            animator.SetInteger("PlayerEstado", 2);
        } else if (movimiento.y > 0)
        {
            animator.SetInteger("PlayerEstado", 3);
        } else if (movimiento.y < 0)
        {
            animator.SetInteger("PlayerEstado", 4);
        } else
        {
            animator.SetInteger("PlayerEstado", 5);
        }
    }

    public void OnEnable()
    {
        controles.Enable();
        controles.Player.Saltar.performed += this.OnSaltar;
    }

    public void OnDisable()
    {
        controles.Disable();
        controles.Player.Saltar.performed -= OnSaltar;
    }

    private void OnSaltar(InputAction.CallbackContext context)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, salto);
    }
}
