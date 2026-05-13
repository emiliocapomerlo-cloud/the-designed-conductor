using UnityEngine;

/// <summary>
/// Controlador de física del vehículo en la Fase 2.
/// Implementa física inercial pesada con controles manuales de pedales,
/// marchas y volante con memoria de giro.
/// </summary>
public class VehiclePhysicsController : MonoBehaviour
{
    public static VehiclePhysicsController Instance { get; private set; }

    [Header("Física del Vehículo")]
    [SerializeField] private Rigidbody2D vehicleRB;
    [SerializeField] private float enginePower = 100f;
    [SerializeField] private float brakePower = 150f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float maxReverseSpeed = -10f;
    [SerializeField] private float friction = 0.1f;
    [SerializeField] private float steeringResponse = 2f;
    [SerializeField] private float steeringDecay = 5f; // Velocidad de retorno del volante

    [Header("Control")]
    [SerializeField] private float wheelRotationMemory = 0.8f; // 0 a 1, controla cuánto "recuerda" el volante
    [SerializeField] private float maxWheelRotation = 45f;

    private float currentEngineInput = 0f;
    private float currentBrakeInput = 0f;
    private float currentWheelRotation = 0f;
    private float targetWheelRotation = 0f;
    private float currentSpeed = 0f;
    private int currentGear = 0; // -1: Reversa, 0: Neutral, 1: Adelante

    public event System.Action<float> OnSpeedChanged;
    public event System.Action<int> OnGearChanged;
    public event System.Action<float> OnWheelRotationChanged;

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
        if (vehicleRB == null)
        {
            vehicleRB = GetComponent<Rigidbody2D>();
        }
        if (vehicleRB == null)
        {
            vehicleRB = gameObject.AddComponent<Rigidbody2D>();
            vehicleRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        currentGear = 1; // Iniciar en adelante
    }

    private void Update()
    {
        HandleInput();
        UpdateWheelMemory();
    }

    private void FixedUpdate()
    {
        ApplyPhysics();
    }

    private void HandleInput()
    {
        // Control de acelerador y frenos
        currentEngineInput = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            currentEngineInput = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            currentBrakeInput = 1f;
        else
            currentBrakeInput = 0f;

        // Control de volante con memoria
        float steeringInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            steeringInput = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            steeringInput = 1f;

        // Aplicar memoria al volante (no vuelve al centro automáticamente)
        targetWheelRotation += steeringInput * steeringResponse;
        targetWheelRotation = Mathf.Clamp(targetWheelRotation, -maxWheelRotation, maxWheelRotation);

        // Cambio de marchas
        if (Input.GetKeyDown(KeyCode.E))
            ChangeGear(1); // Adelante
        if (Input.GetKeyDown(KeyCode.Q))
            ChangeGear(-1); // Reversa
        if (Input.GetKeyDown(KeyCode.R))
            ChangeGear(0); // Neutral
    }

    private void UpdateWheelMemory()
    {
        // El volante decae lentamente hacia el centro cuando no hay input
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            // Está siendo controlado, mantener la posición
        }
        else
        {
            // Decaer hacia el centro
            targetWheelRotation = Mathf.Lerp(targetWheelRotation, 0f, steeringDecay * Time.deltaTime);
        }

        // Suavizar la transición del volante
        currentWheelRotation = Mathf.Lerp(currentWheelRotation, targetWheelRotation, wheelRotationMemory);
        OnWheelRotationChanged?.Invoke(currentWheelRotation);
    }

    private void ApplyPhysics()
    {
        // Calcular velocidad actual
        currentSpeed = vehicleRB.velocity.magnitude;
        OnSpeedChanged?.Invoke(currentSpeed);

        // Aplicar aceleración y frenado
        Vector2 forceDirection = transform.up; // El vehículo avanza en +Y
        float forceToApply = 0f;

        if (currentGear != 0)
        {
            if (currentEngineInput > 0)
            {
                float speedLimit = currentGear > 0 ? maxSpeed : Mathf.Abs(maxReverseSpeed);
                if ((currentGear > 0 && currentSpeed < speedLimit) ||
                    (currentGear < 0 && currentSpeed < Mathf.Abs(maxReverseSpeed)))
                {
                    forceToApply = enginePower * currentEngineInput * currentGear;
                }
            }

            if (currentBrakeInput > 0)
            {
                forceToApply -= brakePower * currentBrakeInput * Mathf.Sign(currentSpeed);
            }
        }

        vehicleRB.AddForce(forceDirection * forceToApply, ForceMode2D.Force);

        // Aplicar fricción
        vehicleRB.velocity *= (1 - friction * Time.fixedDeltaTime);

        // Aplicar rotación basada en el volante y la velocidad
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float rotationForce = currentWheelRotation * currentSpeed * 2f;
            vehicleRB.angularVelocity = rotationForce;
        }
    }

    private void ChangeGear(int gear)
    {
        currentGear = gear;
        OnGearChanged?.Invoke(currentGear);
        Debug.Log($"[Vehicle] Gear changed to: {(gear == 1 ? "Forward" : gear == -1 ? "Reverse" : "Neutral")}");
    }

    public void ApplyExternalForce(Vector2 force)
    {
        vehicleRB.AddForce(force, ForceMode2D.Force);
    }

    public float GetCurrentSpeed() => currentSpeed;
    public int GetCurrentGear() => currentGear;
    public float GetWheelRotation() => currentWheelRotation;
    public Vector2 GetVelocity() => vehicleRB.velocity;
}
