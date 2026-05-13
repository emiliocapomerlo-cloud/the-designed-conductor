using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manejador de eventos de UI para diálogos y decisiones del jugador.
/// </summary>
public class EventDialogHandler : MonoBehaviour
{
    [SerializeField] private Canvas dialogCanvas;
    [SerializeField] private Text dialogText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;

    private EventManager eventManager;
    private bool dialogActive = false;

    private void Start()
    {
        eventManager = EventManager.Instance;
        
        if (eventManager != null)
        {
            eventManager.OnEventTriggered += ShowEventDialog;
        }

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptDialog);
        if (rejectButton != null)
            rejectButton.onClick.AddListener(OnRejectDialog);

        HideDialog();
    }

    private void ShowEventDialog(EventManager.GameEvent gameEvent)
    {
        dialogActive = true;
        
        if (dialogText != null)
        {
            dialogText.text = $"{gameEvent.eventTitle}\n\n{gameEvent.eventDescription}";
        }

        if (dialogCanvas != null)
        {
            dialogCanvas.gameObject.SetActive(true);
        }
    }

    private void OnAcceptDialog()
    {
        HideDialog();
        if (eventManager != null)
        {
            eventManager.RegisterPlayerDecision("accept");
        }
    }

    private void OnRejectDialog()
    {
        HideDialog();
        if (eventManager != null)
        {
            eventManager.RegisterPlayerDecision("reject");
        }
    }

    private void HideDialog()
    {
        dialogActive = false;
        if (dialogCanvas != null)
        {
            dialogCanvas.gameObject.SetActive(false);
        }
    }
}
