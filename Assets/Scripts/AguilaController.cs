using UnityEngine;

// Controlador del Aguila. Soporta teclado (WASD) + Joystick virtual al mismo tiempo.
// Si tocas el joystick, su input tiene prioridad. Si no, usa WASD.
public class AguilaController : MonoBehaviour
{
    // ---------- VARIABLES ----------
    public float velocidad = 5f;              // visible en Inspector
    public VirtualJoystick joystick;          // opcional: se asigna cuando hay joystick en la escena

    private int puntaje = 0;
    private Rigidbody2D rb;
    private Vector2 direccion;

    // ---------- METODOS DE UNITY ----------
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1) Leer teclado
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.W)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;
        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;

        direccion = new Vector2(x, y).normalized;

        // 2) Si hay joystick y se esta usando, su input SOBRESCRIBE al teclado
        if (joystick != null && joystick.Direction.sqrMagnitude > 0.01f)
        {
            direccion = joystick.Direction;
            if (direccion.sqrMagnitude > 1f) direccion = direccion.normalized;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direccion * velocidad;
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (otro.CompareTag("Gema"))
        {
            CambiarPuntaje(10);
            Destroy(otro.gameObject);
        }
        else if (otro.CompareTag("Cereza"))
        {
            CambiarPuntaje(-5);
            Destroy(otro.gameObject);
        }
    }

    // ---------- METODO PROPIO ----------
    void CambiarPuntaje(int cantidad)
    {
        puntaje += cantidad;
        Debug.Log("Puntaje: " + puntaje);
    }
}
