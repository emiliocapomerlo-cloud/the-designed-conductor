using UnityEngine;

/// <summary>
/// Controlador de la Fase 1 (Recolección en Top-Down).
/// Gestiona la escena del estacionamiento y transiciones.
/// </summary>
public class Phase1CollectionController : MonoBehaviour
{
    [SerializeField] private Transform parkingArea;
    [SerializeField] private Transform carSpawnPoint;
    [SerializeField] private Canvas uiCanvas;

    [SerializeField] private Text friendsCountText;
    [SerializeField] private Button startButton;

    private int friendsCollected = 0;
    private int totalFriendsNeeded = 3;

    private void Start()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnFriendsCountChanged += UpdateFriendsUI;
            totalFriendsNeeded = gameManager.GetTotalFriendsNeeded();
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartPhase1);
        }

        UpdateFriendsUI(0, totalFriendsNeeded);
    }

    private void Update()
    {
        // Detectar si todos los amigos han sido recolectados
        if (GameManager.Instance != null && GameManager.Instance.HasCollectedAllFriends())
        {
            // Mostrar opción para empezar conducción
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartPhase2();
            }
        }
    }

    private void UpdateFriendsUI(int collected, int total)
    {
        friendsCollected = collected;
        totalFriendsNeeded = total;

        if (friendsCountText != null)
        {
            friendsCountText.text = $"Amigos recolectados: {friendsCollected}/{totalFriendsNeeded}";
            
            if (friendsCollected >= totalFriendsNeeded)
            {
                friendsCountText.text += "\n¡Presiona ENTER para comenzar la conducción!";
            }
        }
    }

    private void OnStartPhase1()
    {
        GameManager.Instance?.StartPhase1();
    }

    private void StartPhase2()
    {
        GameManager.Instance?.TransitionToPhase2();
        // Aquí se cargaría la escena de conducción
    }
}
