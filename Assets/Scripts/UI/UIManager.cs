using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestor de UI principal que coordina todos los elementos visuales.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Text gameStateText;
    [SerializeField] private Image healthBar;
    [SerializeField] private Text glitchWarning;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color dangerColor = Color.red;

    private GameManager gameManager;
    private GlitchManager glitchManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        glitchManager = GlitchManager.Instance;

        if (gameManager != null)
        {
            gameManager.OnPhaseChanged += UpdatePhaseUI;
        }

        if (glitchManager != null)
        {
            glitchManager.OnGlitchIntensityChanged += UpdateGlitchWarning;
        }
    }

    private void UpdatePhaseUI(GameManager.GamePhase phase)
    {
        if (gameStateText != null)
        {
            gameStateText.text = phase switch
            {
                GameManager.GamePhase.Phase1_Collection => "FASE 1: Recolección de Amigos",
                GameManager.GamePhase.Phase2_Driving => "FASE 2: Conducción Caótica",
                GameManager.GamePhase.Victory => "¡VICTORIA! Todos llegaron a casa",
                GameManager.GamePhase.GameOver => "GAME OVER - Tiempo agotado",
                _ => "Menú Principal"
            };
        }
    }

    private void UpdateGlitchWarning(float glitchIntensity)
    {
        if (glitchWarning != null)
        {
            glitchWarning.text = glitchIntensity switch
            {
                > 0.66f => "⚠️ GLITCH SEVERO ⚠️",
                > 0.33f => "⚠️ GLITCH MODERADO",
                > 0f => "⚠️ Pequeño glitch",
                _ => ""
            };

            Color warningColor = Color.Lerp(normalColor, dangerColor, glitchIntensity);
            glitchWarning.color = warningColor;
        }
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (gameStateText != null)
        {
            gameStateText.text = message;
            StartCoroutine(ClearMessageAfterDelay(duration));
        }
    }

    private System.Collections.IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // No hacer nada después, simplemente dejar que se actualice naturalmente
    }

    public void UpdateHealthDisplay(float health, float maxHealth)
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = health / maxHealth;
        }
    }
}
