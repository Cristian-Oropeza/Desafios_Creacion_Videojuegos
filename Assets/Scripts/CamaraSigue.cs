using UnityEngine;

// Hace que la camara siga al jugador de forma suave.
// Se pone en la Main Camera y se le asigna 'objetivo' con el GameObject Aguila.
public class CamaraSigue : MonoBehaviour
{
    public Transform objetivo;                // el Aguila que debe seguir
    public float velocidadSeguimiento = 5f;   // que tan rapido se "pega" a su objetivo
    private Vector3 offset;                   // distancia inicial entre la camara y el objetivo

    void Start()
    {
        if (objetivo != null)
            offset = transform.position - objetivo.position;
    }

    // LateUpdate se ejecuta DESPUES de todos los Update, es el lugar correcto para mover camaras
    void LateUpdate()
    {
        if (objetivo == null) return;
        Vector3 destino = objetivo.position + offset;
        transform.position = Vector3.Lerp(transform.position, destino, velocidadSeguimiento * Time.deltaTime);
    }
}
