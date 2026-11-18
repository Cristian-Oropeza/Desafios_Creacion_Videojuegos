using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChecarPiso : MonoBehaviour
{
    private bool enPiso;

    private void OnTriggerEnter2D(Collider2D collision)
    {
            enPiso = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
            enPiso = false;
    }
}
