using UnityEngine;
using UnityEngine.EventSystems;

// Script tal cual lo proporciono el profe en la actividad.
public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Componentes de la UI")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Configuracion")]
    public float handleRange = 1f;

    private Vector2 inputVector = Vector2.zero;

    public float Horizontal => inputVector.x;
    public float Vertical => inputVector.y;
    public Vector2 Direction => inputVector;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position = RectTransformUtility.WorldToScreenPoint(null, background.position);
        Vector2 radius = background.sizeDelta / 2f;

        inputVector = (eventData.position - position) / (radius * background.lossyScale.x);
        inputVector = (inputVector.magnitude > 1f) ? inputVector.normalized : inputVector;

        handle.anchoredPosition = inputVector * (radius.x * handleRange);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}
