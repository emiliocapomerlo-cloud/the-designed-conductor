using UnityEngine;

/// <summary>
/// Manejador de input para Fase 2 con soporte para glitches.
/// Invierte los controles cuando es necesario y aplica retrasos.
/// </summary>
public class Phase2InputHandler : MonoBehaviour
{
    private GlitchManager glitchManager;
    private float reactionDelayTimer = 0f;
    private float currentReactionDelay = 0f;

    private bool controlsInverted = false;

    private void Start()
    {
        glitchManager = GlitchManager.Instance;
        
        if (glitchManager != null)
        {
            glitchManager.OnControlsInverted += SetControlsInverted;
        }
    }

    private void Update()
    {
        if (glitchManager != null)
        {
            currentReactionDelay = glitchManager.GetCurrentReactionDelay();
        }

        // Actualizar timer de retraso de reacción
        if (reactionDelayTimer > 0)
        {
            reactionDelayTimer -= Time.deltaTime;
        }
    }

    public bool GetAccelerationInput()
    {
        if (IsDelayed()) return false;
        
        bool input = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        return controlsInverted ? !input : input;
    }

    public bool GetBrakeInput()
    {
        if (IsDelayed()) return false;
        
        bool input = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        return controlsInverted ? !input : input;
    }

    public float GetSteeringInput()
    {
        if (IsDelayed()) return 0f;
        
        float steeringInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            steeringInput -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            steeringInput += 1f;

        return controlsInverted ? -steeringInput : steeringInput;
    }

    public bool GetGearUpInput()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    public bool GetGearDownInput()
    {
        return Input.GetKeyDown(KeyCode.Q);
    }

    public bool GetNeutralInput()
    {
        return Input.GetKeyDown(KeyCode.R);
    }

    private bool IsDelayed()
    {
        return reactionDelayTimer > 0;
    }

    private void SetControlsInverted(bool inverted)
    {
        controlsInverted = inverted;
    }

    public void TriggerReactionDelay()
    {
        reactionDelayTimer = currentReactionDelay;
    }
}
