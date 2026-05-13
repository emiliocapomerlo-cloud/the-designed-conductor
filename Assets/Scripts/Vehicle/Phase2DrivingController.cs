using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de la pantalla de conducción en Fase 2.
/// Maneja la perspectiva de "falsa primera persona" con sprites 2D superpuestos
/// que simulan la profundidad de la carretera.
/// </summary>
public class Phase2DrivingController : MonoBehaviour
{
    [SerializeField] private VehiclePhysicsController vehicleController;
    [SerializeField] private Camera mainCamera;
    
    [Header("UI")]
    [SerializeField] private Text speedText;
    [SerializeField] private Text gearText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text glitchIndicator;
    [SerializeField] private RawImage roadsideLeft;
    [SerializeField] private RawImage roadsideRight;
    [SerializeField] private Image blurEffect;

    [Header("Efectos Visuales")]
    [SerializeField] private float roadsideScrollSpeed = 0.1f;
    [SerializeField] private Texture2D roadSideTexture;

    private float roadsideOffsetX = 0f;
    private Material blurMaterial;
    private GlitchManager glitchManager;
    private EventManager eventManager;

    private void Start()
    {
        glitchManager = GlitchManager.Instance;
        eventManager = EventManager.Instance;

        if (glitchManager != null)
        {
            glitchManager.OnVisionBlurChanged += UpdateBlurEffect;
        }

        if (vehicleController != null)
        {
            vehicleController.OnSpeedChanged += UpdateSpeedUI;
            vehicleController.OnGearChanged += UpdateGearUI;
        }

        if (blurEffect != null)
        {
            blurMaterial = blurEffect.material;
        }
    }

    private void Update()
    {
        UpdateUI();
        ScrollRoadSide();
    }

    private void UpdateUI()
    {
        // Velocidad
        if (speedText != null && vehicleController != null)
        {
            float speed = vehicleController.GetCurrentSpeed();
            speedText.text = $"Velocidad: {speed:F1} km/h";
        }

        // Marcha
        if (gearText != null && vehicleController != null)
        {
            int gear = vehicleController.GetCurrentGear();
            string gearName = gear == 1 ? "D" : gear == -1 ? "R" : "N";
            gearText.text = $"Marcha: {gearName}";
        }

        // Tiempo
        if (timeText != null && GameManager.Instance != null)
        {
            float elapsed = GameManager.Instance.GetElapsedTime();
            float limit = GameManager.Instance.GetTimeLimit();
            timeText.text = $"Tiempo: {elapsed:F1}s / {limit:F1}s";
        }

        // Indicador de Glitch
        if (glitchIndicator != null && glitchManager != null)
        {
            float glitchIntensity = glitchManager.GetCurrentGlitchIntensity();
            glitchIndicator.text = $"Glitch: {glitchIntensity:P}";
            
            // Cambiar color según intensidad
            Color indicatorColor = Color.Lerp(Color.green, Color.red, glitchIntensity);
            glitchIndicator.color = indicatorColor;
        }
    }

    private void UpdateSpeedUI(float speed)
    {
        // Callback cuando la velocidad cambia
    }

    private void UpdateGearUI(int gear)
    {
        // Callback cuando cambia la marcha
    }

    private void UpdateBlurEffect(float blurAmount)
    {
        if (blurMaterial != null)
        {
            blurMaterial.SetFloat("_BlurAmount", blurAmount);
        }
        
        if (blurEffect != null)
        {
            Color blurColor = blurEffect.color;
            blurColor.a = blurAmount / 10f; // Normalizar para visualización
            blurEffect.color = blurColor;
        }
    }

    private void ScrollRoadSide()
    {
        if (vehicleController == null || roadsideLeft == null || roadsideRight == null)
            return;

        Vector2 velocity = vehicleController.GetVelocity();
        roadsideOffsetX += velocity.y * roadsideScrollSpeed * Time.deltaTime;

        Vector2 offsetLeft = new Vector2(roadsideOffsetX, 0f);
        Vector2 offsetRight = new Vector2(roadsideOffsetX, 0f);

        roadsideLeft.uvRect = new Rect(offsetLeft, new Vector2(1f, 1f));
        roadsideRight.uvRect = new Rect(offsetRight, new Vector2(1f, 1f));
    }

    private void OnDestroy()
    {
        if (glitchManager != null)
        {
            glitchManager.OnVisionBlurChanged -= UpdateBlurEffect;
        }
    }
}
