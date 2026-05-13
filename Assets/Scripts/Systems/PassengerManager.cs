using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gestor de pasajeros que controla su comportamiento caótico
/// y su impacto en la física del vehículo.
/// </summary>
public class PassengerManager : MonoBehaviour
{
    public static PassengerManager Instance { get; private set; }

    [System.Serializable]
    public class Passenger
    {
        public string name;
        public Transform transform;
        public Rigidbody2D rigidbody;
        public float chaosLevel; // 0 a 1
        public bool isDrunk;
        public Vector2 currentLocalPosition;
    }

    [SerializeField] private List<Passenger> passengers = new List<Passenger>();
    [SerializeField] private float chaosDecay = 0.1f;
    [SerializeField] private float drunkennessDuration = 30f;

    private float[] drunkenTimers;

    public event System.Action<string> OnPassengerAction;
    public event System.Action<int, float> OnChaosLevelChanged;

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
        drunkenTimers = new float[passengers.Count];
    }

    private void Update()
    {
        UpdatePassengers();
    }

    private void UpdatePassengers()
    {
        for (int i = 0; i < passengers.Count; i++)
        {
            // Actualizar duración de embriaguez
            if (passengers[i].isDrunk)
            {
                drunkenTimers[i] -= Time.deltaTime;
                if (drunkenTimers[i] <= 0)
                {
                    passengers[i].isDrunk = false;
                }
            }

            // Decaer caos naturalmente
            if (passengers[i].chaosLevel > 0)
            {
                passengers[i].chaosLevel -= chaosDecay * Time.deltaTime;
                passengers[i].chaosLevel = Mathf.Max(0, passengers[i].chaosLevel);
                OnChaosLevelChanged?.Invoke(i, passengers[i].chaosLevel);
            }

            // Aplicar comportamiento caótico
            if (passengers[i].chaosLevel > 0.3f)
            {
                ApplyChaosMovement(i);
            }
        }
    }

    public void CausePassengerChaos(float amount)
    {
        foreach (var passenger in passengers)
        {
            passenger.chaosLevel = Mathf.Min(1f, passenger.chaosLevel + amount);
            OnPassengerAction?.Invoke($"{passenger.name} is getting chaotic!");
        }
    }

    public void MakePassengerDrunk(int passengerIndex)
    {
        if (passengerIndex >= 0 && passengerIndex < passengers.Count)
        {
            passengers[passengerIndex].isDrunk = true;
            drunkenTimers[passengerIndex] = drunkennessDuration;
            passengers[passengerIndex].chaosLevel = Mathf.Min(1f, passengers[passengerIndex].chaosLevel + 0.5f);
            OnPassengerAction?.Invoke($"{passengers[passengerIndex].name} is drunk!");
        }
    }

    public void AddPassenger(string name, Transform transform, Rigidbody2D rigidbody)
    {
        passengers.Add(new Passenger
        {
            name = name,
            transform = transform,
            rigidbody = rigidbody,
            chaosLevel = 0f,
            isDrunk = false,
            currentLocalPosition = Vector2.zero
        });
    }

    public List<Passenger> GetAllPassengers() => new List<Passenger>(passengers);

    public float GetTotalChaosLevel()
    {
        float totalChaos = 0f;
        foreach (var passenger in passengers)
        {
            totalChaos += passenger.chaosLevel;
        }
        return totalChaos / (passengers.Count > 0 ? passengers.Count : 1);
    }

    private void ApplyChaosMovement(int passengerIndex)
    {
        Passenger passenger = passengers[passengerIndex];
        
        // Movimiento aleatorio dentro del auto
        Vector2 randomDirection = Random.insideUnitCircle;
        Vector2 chaosForce = randomDirection * passenger.chaosLevel * 10f;
        
        if (passenger.rigidbody != null)
        {
            passenger.rigidbody.AddForce(chaosForce, ForceMode2D.Force);
        }
    }
}
