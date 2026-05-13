using UnityEngine;

/// <summary>
/// Aplicador de efectos de glitch a la cámara (visión borrosa, distorsión, etc.)
/// </summary>
public class CameraGlitchEffect : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Material glitchMaterial;
    
    private GlitchManager glitchManager;
    private float currentBlur = 0f;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        glitchManager = GlitchManager.Instance;
        
        if (glitchManager != null)
        {
            glitchManager.OnVisionBlurChanged += ApplyBlurEffect;
            glitchManager.OnControlsInverted += HandleControlInversion;
        }
    }

    private void ApplyBlurEffect(float blurAmount)
    {
        currentBlur = blurAmount;
        
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat("_BlurAmount", blurAmount);
        }

        // Alternar foco de la cámara como efecto visual
        if (blurAmount > 5f)
        {
            targetCamera.fieldOfView = Mathf.Lerp(60f, 40f, blurAmount / 10f);
        }
    }

    private void HandleControlInversion(bool inverted)
    {
        Debug.Log($"[CameraGlitch] Controls inverted: {inverted}");
        
        if (inverted)
        {
            // Podría hacer que la cámara se voltee o inverta, pero lo dejamos como aviso
            if (glitchMaterial != null)
            {
                glitchMaterial.SetFloat("_InvertX", 1f);
            }
        }
        else
        {
            if (glitchMaterial != null)
            {
                glitchMaterial.SetFloat("_InvertX", 0f);
            }
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (glitchMaterial != null && currentBlur > 0)
        {
            Graphics.Blit(source, destination, glitchMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
