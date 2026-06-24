using System;
using UnityEngine;

public class ControladorPaisaje : MonoBehaviour
{
    public VolanteManejo volante;
    public PalancaManejo palanca;

    public RectTransform calle1;
    public RectTransform calle2;
    public RectTransform cabina;
    public PerspectivaCaminoVisual caminoPerspectiva;

    public float velocidadAuto = 190f;
    public float fuerzaGiro = 1.8f;
    public float altoCalle = 800f;
    public float limiteHorizontalCamino = 260f;
    public float intensidadVibracionCabina = 3f;

    private Vector2 posicionInicialCabina;
    private float avanceAcumulado;
    private float desplazamientoHorizontal;
    private float multiplicadorFuerzaGiro = 1f;
    private float multiplicadorVibracion = 1f;
    private float derivaInvoluntaria;
    private float oscilacionVolante;
    private float tiempoEfectoDecision;
    private float duracionEfectoDecision;
    private float multiplicadorVelocidad = 1f;
    private float multiplicadorLimiteCamino = 1f;
    private float respuestaVolante = 1f;
    private float pulsoVelocidad;
    private float sacudidaCabina;
    private float rotacionVolanteFiltrada;
    private float velocidadActual;
    private string nombreEfectoDecision = "";

    public bool HayEfectoDecision => tiempoEfectoDecision > 0f;
    public float ProgresoEfectoDecision => duracionEfectoDecision <= 0f ? 0f : tiempoEfectoDecision / duracionEfectoDecision;
    public float SegundosRestantesEfecto => tiempoEfectoDecision;
    public string NombreEfectoDecision => nombreEfectoDecision;
    public float VelocidadActual => velocidadActual;
    public float VelocidadMaximaVisual => Mathf.Max(1f, velocidadAuto * 1.5f);

    // Posicion lateral del auto dentro del camino: -1 = borde izquierdo,
    // 0 = centro y 1 = borde derecho. Los obstaculos la usan para calcular
    // impactos sin depender de colliders fisicos en la interfaz.
    public float PosicionLateralNormalizada
    {
        get
        {
            float limiteActual = limiteHorizontalCamino * multiplicadorLimiteCamino;
            return limiteActual <= 0f ? 0f : Mathf.Clamp(desplazamientoHorizontal / limiteActual, -1f, 1f);
        }
    }

    // Distancia total recorrida hacia adelante (sirve para saber si llegamos a casa).
    public float AvanceAcumulado => avanceAcumulado;

    // Qué tan pegado al borde del camino está el auto (0 = centrado, 1 = contra el borde).
    public float DesvioNormalizado
    {
        get
        {
            float limiteActual = limiteHorizontalCamino * multiplicadorLimiteCamino;
            if (limiteActual <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(Mathf.Abs(desplazamientoHorizontal) / limiteActual);
        }
    }

    private void Start()
    {
        velocidadAuto = 190f;
        CabinaEnviroBootstrap.AplicarVisualCabina();

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
            velocidadActual = 0f;
            return;
        }

        float factorMarcha = palanca.enMarchaAdelante ? 1f : -1f;
        float rotacionVolante = Mathf.DeltaAngle(0f, volante.transform.eulerAngles.z);
        rotacionVolanteFiltrada = Mathf.Lerp(rotacionVolanteFiltrada, rotacionVolante, Mathf.Clamp01(respuestaVolante * Time.deltaTime * 12f));
        float rotacionBase = respuestaVolante >= 0.99f ? rotacionVolante : rotacionVolanteFiltrada;
        float rotacionAfectada = CalcularRotacionAfectada(rotacionBase);
        float velocidadConPulso = velocidadAuto * multiplicadorVelocidad * CalcularPulsoVelocidad();
        velocidadActual = Mathf.Abs(velocidadConPulso);
        float avanceFrame = velocidadConPulso * factorMarcha * Time.deltaTime;
        float limiteActual = limiteHorizontalCamino * multiplicadorLimiteCamino;

        avanceAcumulado += avanceFrame;
        desplazamientoHorizontal = Mathf.Clamp(
            desplazamientoHorizontal + rotacionAfectada * fuerzaGiro * multiplicadorFuerzaGiro * factorMarcha * Time.deltaTime + CalcularDerivaFrame(),
            -limiteActual,
            limiteActual
        );

        ActualizarCaminoPerspectiva();
        SincronizarCalles();

        VibrarCabina(rotacionAfectada);
        ActualizarEfectoDecision();
    }

    public void AplicarEfectoDecision(float duracion, float nuevoMultiplicadorGiro, float nuevoMultiplicadorVibracion, float nuevaDerivaInvoluntaria, float nuevaOscilacionVolante)
    {
        AplicarEfectoDecision("Control alterado", duracion, nuevoMultiplicadorGiro, nuevoMultiplicadorVibracion, nuevaDerivaInvoluntaria, nuevaOscilacionVolante, 1f, 1f, 1f, 0f, 0f);
    }

    public void AplicarEfectoDecision(string nombreEfecto, float duracion, float nuevoMultiplicadorGiro, float nuevoMultiplicadorVibracion, float nuevaDerivaInvoluntaria, float nuevaOscilacionVolante, float nuevoMultiplicadorVelocidad, float nuevoMultiplicadorLimiteCamino)
    {
        AplicarEfectoDecision(nombreEfecto, duracion, nuevoMultiplicadorGiro, nuevoMultiplicadorVibracion, nuevaDerivaInvoluntaria, nuevaOscilacionVolante, nuevoMultiplicadorVelocidad, nuevoMultiplicadorLimiteCamino, 1f, 0f, 0f);
    }

    public void AplicarEfectoDecision(string nombreEfecto, float duracion, float nuevoMultiplicadorGiro, float nuevoMultiplicadorVibracion, float nuevaDerivaInvoluntaria, float nuevaOscilacionVolante, float nuevoMultiplicadorVelocidad, float nuevoMultiplicadorLimiteCamino, float nuevaRespuestaVolante, float nuevoPulsoVelocidad, float nuevaSacudidaCabina)
    {
        duracionEfectoDecision = Mathf.Max(0f, duracion);
        tiempoEfectoDecision = duracionEfectoDecision;
        multiplicadorFuerzaGiro = Mathf.Max(0f, nuevoMultiplicadorGiro);
        multiplicadorVibracion = Mathf.Max(0f, nuevoMultiplicadorVibracion);
        derivaInvoluntaria = Mathf.Max(0f, nuevaDerivaInvoluntaria);
        oscilacionVolante = Mathf.Max(0f, nuevaOscilacionVolante);
        multiplicadorVelocidad = Mathf.Max(0f, nuevoMultiplicadorVelocidad);
        multiplicadorLimiteCamino = Mathf.Max(0.1f, nuevoMultiplicadorLimiteCamino);
        respuestaVolante = Mathf.Max(0.02f, nuevaRespuestaVolante);
        pulsoVelocidad = Mathf.Max(0f, nuevoPulsoVelocidad);
        sacudidaCabina = Mathf.Max(0f, nuevaSacudidaCabina);
        rotacionVolanteFiltrada = Mathf.DeltaAngle(0f, volante != null ? volante.transform.eulerAngles.z : 0f);
        nombreEfectoDecision = string.IsNullOrEmpty(nombreEfecto) ? "Control alterado" : nombreEfecto;

        if (caminoPerspectiva != null)
        {
            bool usarTierra = string.Equals(nombreEfectoDecision, "Atajo de tierra", StringComparison.OrdinalIgnoreCase);
            caminoPerspectiva.SetModoSuperficie(usarTierra);
        }
    }

    private float CalcularRotacionAfectada(float rotacionVolante)
    {
        if (!HayEfectoDecision || oscilacionVolante <= 0f)
        {
            return rotacionVolante;
        }

        float ruidoLento = (Mathf.PerlinNoise(Time.time * 1.7f, 7.3f) - 0.5f) * oscilacionVolante;
        float bamboleo = Mathf.Sin(Time.time * 5.5f) * oscilacionVolante * 0.35f;
        return rotacionVolante + ruidoLento + bamboleo;
    }

    private float CalcularDerivaFrame()
    {
        if (!HayEfectoDecision || derivaInvoluntaria <= 0f)
        {
            return 0f;
        }

        float direccion = Mathf.Sin(Time.time * 2.1f) + (Mathf.PerlinNoise(Time.time * 1.2f, 11f) - 0.5f);
        return direccion * derivaInvoluntaria * Time.deltaTime;
    }

    private float CalcularPulsoVelocidad()
    {
        if (!HayEfectoDecision || pulsoVelocidad <= 0f)
        {
            return 1f;
        }

        float pulso = Mathf.Sin(Time.time * 7.5f) * 0.5f + (Mathf.PerlinNoise(Time.time * 4f, 19f) - 0.5f);
        return Mathf.Max(0.1f, 1f + pulso * pulsoVelocidad);
    }

    private void ActualizarEfectoDecision()
    {
        if (!HayEfectoDecision)
        {
            return;
        }

        tiempoEfectoDecision -= Time.deltaTime;

        if (tiempoEfectoDecision <= 0f)
        {
            tiempoEfectoDecision = 0f;
            duracionEfectoDecision = 0f;
            multiplicadorFuerzaGiro = 1f;
            multiplicadorVibracion = 1f;
            derivaInvoluntaria = 0f;
            oscilacionVolante = 0f;
            multiplicadorVelocidad = 1f;
            multiplicadorLimiteCamino = 1f;
            respuestaVolante = 1f;
            pulsoVelocidad = 0f;
            sacudidaCabina = 0f;
            nombreEfectoDecision = "";

            if (caminoPerspectiva != null)
            {
                caminoPerspectiva.SetModoSuperficie(false);
            }

            if (cabina != null)
            {
                cabina.anchoredPosition = posicionInicialCabina;
            }
        }
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
        if (cabina == null || (intensidadVibracionCabina <= 0f && !HayEfectoDecision))
        {
            return;
        }

        float intensidadBase = intensidadVibracionCabina;
        if (HayEfectoDecision && intensidadBase <= 0f)
        {
            intensidadBase = 3f;
        }

        float ruido = Mathf.PerlinNoise(Time.time * 12f, 0f) - 0.5f;
        float sacudida = HayEfectoDecision ? Mathf.Sin(Time.time * 22f) * sacudidaCabina : 0f;
        float giroNormalizado = Mathf.Clamp(rotacionVolante / 90f, -1f, 1f);
        Vector2 vibracion = new Vector2(giroNormalizado * 2f + sacudida * 0.35f, ruido * intensidadBase * multiplicadorVibracion + sacudida);
        cabina.anchoredPosition = posicionInicialCabina + vibracion;
    }
}
