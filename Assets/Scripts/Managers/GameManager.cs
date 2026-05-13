using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestor central del juego que controla las transiciones entre fases
/// y mantiene el estado global del juego.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GamePhase { Phase1_Collection, Phase2_Driving, Menu, GameOver, Victory }
    
    [SerializeField] private GamePhase currentPhase = GamePhase.Menu;
    [SerializeField] private List<string> friendsToCollect = new List<string>();
    [SerializeField] private List<string> friendsCollected = new List<string>();
    
    [SerializeField] private int totalFriendsNeeded = 3;
    [SerializeField] private float timeLimit = 600f; // 10 minutos para conducir
    
    private float elapsedTime = 0f;
    private bool gameActive = false;

    public event System.Action<GamePhase> OnPhaseChanged;
    public event System.Action<int, int> OnFriendsCountChanged;
    public event System.Action<float, float> OnTimeChanged;

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
        InitializeFriends();
    }

    private void Update()
    {
        if (gameActive && currentPhase == GamePhase.Phase2_Driving)
        {
            elapsedTime += Time.deltaTime;
            OnTimeChanged?.Invoke(elapsedTime, timeLimit);
            
            if (elapsedTime >= timeLimit)
            {
                EndGame(true);
            }
        }
    }

    private void InitializeFriends()
    {
        friendsToCollect.Clear();
        friendsCollected.Clear();
        
        for (int i = 1; i <= totalFriendsNeeded; i++)
        {
            friendsToCollect.Add($"Friend_{i}");
        }
    }

    public void StartPhase1()
    {
        currentPhase = GamePhase.Phase1_Collection;
        gameActive = true;
        OnPhaseChanged?.Invoke(currentPhase);
    }

    public void TransitionToPhase2()
    {
        currentPhase = GamePhase.Phase2_Driving;
        elapsedTime = 0f;
        OnPhaseChanged?.Invoke(currentPhase);
    }

    public void CollectFriend(string friendName)
    {
        if (friendsToCollect.Contains(friendName))
        {
            friendsToCollect.Remove(friendName);
            friendsCollected.Add(friendName);
            OnFriendsCountChanged?.Invoke(friendsCollected.Count, totalFriendsNeeded);
            
            if (friendsCollected.Count >= totalFriendsNeeded)
            {
                TransitionToPhase2();
            }
        }
    }

    public bool HasCollectedAllFriends()
    {
        return friendsCollected.Count >= totalFriendsNeeded;
    }

    public void EndGame(bool victory)
    {
        gameActive = false;
        currentPhase = victory ? GamePhase.Victory : GamePhase.GameOver;
        OnPhaseChanged?.Invoke(currentPhase);
    }

    public GamePhase GetCurrentPhase() => currentPhase;
    public float GetElapsedTime() => elapsedTime;
    public float GetTimeLimit() => timeLimit;
    public int GetCollectedFriendsCount() => friendsCollected.Count;
    public int GetTotalFriendsNeeded() => totalFriendsNeeded;
}
