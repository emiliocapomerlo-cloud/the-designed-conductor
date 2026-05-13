using UnityEngine;

/// <summary>
/// Configuración de dificultad dinámica que ajusta parámetros según el progreso del jugador.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [System.Serializable]
    public class DifficultySettings
    {
        public string difficultyName;
        public float glitchAccumulationRate = 0.1f;
        public float passengerChaosRate = 0.05f;
        public float eventFrequency = 0.3f;
        public float vehicleResponsiveness = 1f;
        public float timeLimit = 600f;
    }

    [SerializeField] private DifficultySettings easySettings;
    [SerializeField] private DifficultySettings normalSettings;
    [SerializeField] private DifficultySettings hardSettings;

    public enum Difficulty { Easy, Normal, Hard }
    private Difficulty currentDifficulty = Difficulty.Normal;

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

    public void SetDifficulty(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
        ApplyDifficultySettings();
        Debug.Log($"[DifficultyManager] Difficulty set to: {difficulty}");
    }

    private void ApplyDifficultySettings()
    {
        DifficultySettings settings = GetCurrentSettings();
        
        // Aplicar configuraciones a los managers relevantes
        if (GlitchManager.Instance != null)
        {
            // Aquí iría la aplicación de settings
        }

        if (PassengerManager.Instance != null)
        {
            // Aquí iría la aplicación de settings
        }
    }

    public DifficultySettings GetCurrentSettings()
    {
        return currentDifficulty switch
        {
            Difficulty.Easy => easySettings,
            Difficulty.Hard => hardSettings,
            _ => normalSettings
        };
    }

    public Difficulty GetCurrentDifficulty() => currentDifficulty;
}
