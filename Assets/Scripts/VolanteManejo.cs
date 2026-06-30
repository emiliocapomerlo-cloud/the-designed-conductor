using UnityEngine;
using UnityEngine.EventSystems;

// Volante UI con entrada acotada. La escena de manejo lee EntradaGiroNormalizada
// para mover el auto en coordenadas de camino, no los grados crudos del objeto.
public class VolanteManejo : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private float anguloMaximo = 135f;
    [SerializeField] private float velocidadRetorno = 4f;
    [SerializeField] private float suavizadoVisual = 18f;
    [SerializeField] private float radioMinimoArrastre = 18f;
    [SerializeField] private bool volverAlCentroAlSoltar = false;

    private RectTransform rectTransform;
    private float anguloActual;
    private float anguloObjetivo;
    private float anguloPunteroAnterior;
    private bool arrastrando;

    // -1 = izquierda, 0 = centro, 1 = derecha.
    public float EntradaGiroNormalizada => anguloMaximo <= 0f ? 0f : Mathf.Clamp(-anguloActual / anguloMaximo, -1f, 1f);

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        anguloActual = Mathf.DeltaAngle(0f, transform.localEulerAngles.z);
        anguloObjetivo = anguloActual;
        AplicarRotacion();
    }

    private void Update()
    {
        if (!arrastrando && volverAlCentroAlSoltar && Mathf.Abs(anguloObjetivo) > 0.01f)
        {
            anguloObjetivo = Mathf.Lerp(anguloObjetivo, 0f, 1f - Mathf.Exp(-velocidadRetorno * Time.deltaTime));
        }

        float suavizado = 1f - Mathf.Exp(-suavizadoVisual * Time.deltaTime);
        anguloActual = Mathf.LerpAngle(anguloActual, anguloObjetivo, suavizado);
        AplicarRotacion();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!TryObtenerAnguloPuntero(eventData, out anguloPunteroAnterior))
        {
            arrastrando = false;
            return;
        }

        arrastrando = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!arrastrando || !TryObtenerAnguloPuntero(eventData, out float anguloPuntero))
        {
            return;
        }

        float delta = Mathf.DeltaAngle(anguloPunteroAnterior, anguloPuntero);
        anguloPunteroAnterior = anguloPuntero;
        anguloObjetivo = Mathf.Clamp(anguloObjetivo + delta, -anguloMaximo, anguloMaximo);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        arrastrando = false;
    }

    private bool TryObtenerAnguloPuntero(PointerEventData eventData, out float angulo)
    {
        if (rectTransform != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 local))
        {
            if (local.sqrMagnitude < radioMinimoArrastre * radioMinimoArrastre)
            {
                angulo = 0f;
                return false;
            }

            angulo = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
            return true;
        }

        Vector2 direccion = eventData.position - (Vector2)transform.position;
        if (direccion.sqrMagnitude < radioMinimoArrastre * radioMinimoArrastre)
        {
            angulo = 0f;
            return false;
        }

        angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        return true;
    }

    private void AplicarRotacion()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, anguloActual);
    }
}
