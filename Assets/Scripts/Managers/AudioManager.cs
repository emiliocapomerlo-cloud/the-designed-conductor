using UnityEngine;

/// <summary>
/// Controlador de la música y sonidos del juego con efectos de distorsión por glitch.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float glitchDistortionAmount = 0.5f;

    private GlitchManager glitchManager;
    private float originalPitch = 1f;

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
        glitchManager = GlitchManager.Instance;
        
        if (musicSource != null)
        {
            originalPitch = musicSource.pitch;
        }

        if (glitchManager != null)
        {
            glitchManager.OnGlitchIntensityChanged += ApplyGlitchAudio;
        }
    }

    private void ApplyGlitchAudio(float glitchIntensity)
    {
        if (musicSource != null)
        {
            // Aplicar distorsión de pitch según glitch
            float pitchVariation = Mathf.Lerp(originalPitch, originalPitch * 0.8f, glitchIntensity);
            musicSource.pitch = pitchVariation;

            // Variar el volumen según el glitch
            float volumeVariation = Mathf.Lerp(1f, 0.5f, glitchIntensity);
            musicSource.volume = volumeVariation;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void StartMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
