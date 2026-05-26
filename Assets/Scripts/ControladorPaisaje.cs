using UnityEngine;

public class ControladorPaisaje : MonoBehaviour
{
    public VolanteManejo volante;
    public PalancaManejo palanca;

    // Ponemos los dos RectTransform de las calles
    public RectTransform calle1;
    public RectTransform calle2;

    public float velocidadAuto = 250f;
    public float fuerzaGiro = 1.8f;

    // El alto de tus imágenes de calle (coincide con el tamaño que le diste)
    private float altoCalle = 800f; 

    void Update()
    {
        float factorMarcha = palanca.enMarchaAdelante ? 1f : -1f;

        // 1. Calcular los movimientos
        float desplaceY = -velocidadAuto * factorMarcha * Time.deltaTime;
        float rotacionVolante = Mathf.DeltaAngle(0, volante.transform.eulerAngles.z);
        float desplaceX = rotacionVolante * fuerzaGiro * factorMarcha * Time.deltaTime;

        Vector2 movimientoVec = new Vector2(desplaceX, desplaceY);

        // 2. Mover ambas calles al mismo tiempo
        calle1.anchoredPosition += movimientoVec;
        calle2.anchoredPosition += movimientoVec;

        // 3. Sistema de bucle para marcha adelante (Drive)
        if (palanca.enMarchaAdelante)
        {
            if (calle1.anchoredPosition.y <= -altoCalle)
                calle1.anchoredPosition = new Vector2(calle1.anchoredPosition.x, calle2.anchoredPosition.y + altoCalle);

            if (calle2.anchoredPosition.y <= -altoCalle)
                calle2.anchoredPosition = new Vector2(calle2.anchoredPosition.x, calle1.anchoredPosition.y + altoCalle);
        }
        // 4. Sistema de bucle para marcha atrás (Reverse)
        else
        {
            if (calle1.anchoredPosition.y >= altoCalle)
                calle1.anchoredPosition = new Vector2(calle1.anchoredPosition.x, calle2.anchoredPosition.y - altoCalle);

            if (calle2.anchoredPosition.y >= altoCalle)
                calle2.anchoredPosition = new Vector2(calle2.anchoredPosition.x, calle1.anchoredPosition.y - altoCalle);
        }
    }
}