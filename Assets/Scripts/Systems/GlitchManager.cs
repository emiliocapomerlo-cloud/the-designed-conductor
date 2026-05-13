using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de Glitches que simula fallos de software como mecánicas intencionales.
/// Altera la visión, sonido y controles del jugador dinámicamente.
/// </summary>
public class GlitchManager : MonoBehaviour
{
    public static GlitchManager Instance { get; private set; }

    [System.Serializable]
    public class GlitchLevel
    {
        public string name;
        public float intensity; // 0 a 1
        public float visionBlur;
        public float controlInversion; // 0 a 1 probabilidad de inversión
        public float reactionTimeDelay;
        public float audioDistortion;
    }

    [SerializeField] private GlitchLevel[] glitchLevels;
    [SerializeField] private float glitchAccumulationRate = 0.1f;
    [SerializeField] private float glitchDecayRate = 0.05f;

    private float currentGlitchIntensity = 0f;
    private int currentGlitchLevel = 0;
    private bool controlsInverted = false;
    private float inversionDuration = 0f;
    private float inversionTimer = 0f;

    public event System.Action<float> OnGlitchIntensityChanged;
    public event System.Action<bool> OnControlsInverted;
    public event System.Action<float> OnVisionBlurChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (glitchLevels == null || glitchLevels.Length == 0)
        {
            InitializeDefaultGlitchLevels();
        }
    }

    private void Update()
    {
        // Decaer glitch naturalmente
        if (currentGlitchIntensity > 0)
        {
            currentGlitchIntensity -= glitchDecayRate * Time.deltaTime;
            currentGlitchIntensity = Mathf.Clamp01(currentGlitchIntensity);
            UpdateGlitchLevel();
        }

        // Actualizar inversión de controles
        if (controlsInverted)
        {
            inversionTimer -= Time.deltaTime;
            if (inversionTimer <= 0)
            {
                SetControlsInverted(false);
            }
        }
    }

    public void AccumulateGlitch(float amount = 1f)
    {
        currentGlitchIntensity = Mathf.Clamp01(currentGlitchIntensity + amount * glitchAccumulationRate);
        UpdateGlitchLevel();
    }

    private void UpdateGlitchLevel()
    {
        int newLevel = Mathf.Min((int)(currentGlitchIntensity * glitchLevels.Length), glitchLevels.Length - 1);
        
        if (newLevel != currentGlitchLevel)
        {
            currentGlitchLevel = newLevel;
        }

        OnGlitchIntensityChanged?.Invoke(currentGlitchIntensity);
        OnVisionBlurChanged?.Invoke(GetCurrentVisionBlur());
    }

    public void TriggerControlInversion(float duration = 3f)
    {
        controlsInverted = true;
        inversionDuration = duration;
        inversionTimer = duration;
        OnControlsInverted?.Invoke(true);
    }

    private void SetControlsInverted(bool inverted)
    {
        controlsInverted = inverted;
        OnControlsInverted?.Invoke(inverted);
    }

    public float GetCurrentGlitchIntensity() => currentGlitchIntensity;
    public float GetCurrentVisionBlur() => glitchLevels[currentGlitchLevel].visionBlur;
    public float GetCurrentReactionDelay() => glitchLevels[currentGlitchLevel].reactionTimeDelay;
    public float GetCurrentAudioDistortion() => glitchLevels[currentGlitchLevel].audioDistortion;
    public bool AreControlsInverted() => controlsInverted;

    private void InitializeDefaultGlitchLevels()
    {
        glitchLevels = new GlitchLevel[]
        {
            new GlitchLevel 
            { 
                name = "Clean", 
                intensity = 0f, 
                visionBlur = 0f, 
                controlInversion = 0f, 
                reactionTimeDelay = 0f,
                audioDistortion = 0f
            },
            new GlitchLevel 
            { 
                name = "Minor", 
                intensity = 0.33f, 
                visionBlur = 2f, 
                controlInversion = 0.1f, 
                reactionTimeDelay = 0.1f,
                audioDistortion = 0.2f
            },
            new GlitchLevel 
            { 
                name = "Moderate", 
                intensity = 0.66f, 
                visionBlur = 5f, 
                controlInversion = 0.3f, 
                reactionTimeDelay = 0.3f,
                audioDistortion = 0.5f
            },
            new GlitchLevel 
            { 
                name = "Severe", 
                intensity = 1f, 
                visionBlur = 10f, 
                controlInversion = 0.6f, 
                reactionTimeDelay = 0.6f,
                audioDistortion = 0.8f
            }
        };
    }
}
