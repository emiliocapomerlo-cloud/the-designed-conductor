using UnityEngine;

public class ControladorPaisaje : MonoBehaviour
{
    public VolanteManejo volante;
    public PalancaManejo palanca;

    public RectTransform calle1;
    public RectTransform calle2;
    public RectTransform cabina;
    public PerspectivaCaminoVisual caminoPerspectiva;

    public float velocidadAuto = 250f;
    public float fuerzaGiro = 1.8f;
    public float altoCalle = 800f;
    public float limiteHorizontalCamino = 260f;
    public float intensidadVibracionCabina = 3f;

    private Vector2 posicionInicialCabina;
    private float avanceAcumulado;
    private float desplazamientoHorizontal;

    private void Start()
    {
        if (cabina != null)
        {
            posicionInicialCabina = cabina.anchoredPosition;
        }

        SincronizarCalles();
    }

    private void Update()
    {
        if (volante == null || palanca == null || calle1 == null || calle2 == null)
        {
            return;
        }

        float factorMarcha = palanca.enMarchaAdelante ? 1f : -1f;
        float rotacionVolante = Mathf.DeltaAngle(0f, volante.transform.eulerAngles.z);
        float avanceFrame = velocidadAuto * factorMarcha * Time.deltaTime;

        avanceAcumulado += avanceFrame;
        desplazamientoHorizontal = Mathf.Clamp(
            desplazamientoHorizontal + rotacionVolante * fuerzaGiro * factorMarcha * Time.deltaTime,
            -limiteHorizontalCamino,
            limiteHorizontalCamino
        );

        ActualizarCaminoPerspectiva();
        SincronizarCalles();

        VibrarCabina(rotacionVolante);
    }

    private void ActualizarCaminoPerspectiva()
    {
        if (caminoPerspectiva != null)
        {
            caminoPerspectiva.ActualizarMovimiento(avanceAcumulado, desplazamientoHorizontal);
        }
    }

    private void SincronizarCalles()
    {
        if (altoCalle <= 0f || calle1 == null || calle2 == null)
        {
            return;
        }

        if (caminoPerspectiva != null)
        {
            calle1.anchoredPosition = Vector2.zero;
            calle2.anchoredPosition = new Vector2(0f, altoCalle);
            return;
        }

        float yBase = -Mathf.Repeat(avanceAcumulado, altoCalle);
        calle1.anchoredPosition = new Vector2(desplazamientoHorizontal, yBase);
        calle2.anchoredPosition = new Vector2(desplazamientoHorizontal, yBase + altoCalle);
    }

    private void VibrarCabina(float rotacionVolante)
    {
        if (cabina == null || intensidadVibracionCabina <= 0f)
        {
            return;
        }

        float ruido = Mathf.PerlinNoise(Time.time * 12f, 0f) - 0.5f;
        float giroNormalizado = Mathf.Clamp(rotacionVolante / 90f, -1f, 1f);
        Vector2 vibracion = new Vector2(giroNormalizado * 2f, ruido * intensidadVibracionCabina);
        cabina.anchoredPosition = posicionInicialCabina + vibracion;
    }
}
