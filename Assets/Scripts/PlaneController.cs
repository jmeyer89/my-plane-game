using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlaneController : MonoBehaviour
{
    [Header("Engine")]
    public float maxThrust = 8000f;
    public float throttleRampSpeed = 0.4f;

    [Header("Aerodynamics")]
    public float liftMultiplier = 2.8f;
    public float dragMultiplier = 1.2f;
    public float stallAngle = 20f;
    public float minLiftSpeed = 25f;

    [Header("Control Surfaces")]
    public float pitchTorque = 6000f;
    public float rollTorque = 5000f;
    public float yawTorque = 2000f;

    [Header("Ground")]
    public float brakeForce = 4000f;
    public LayerMask groundMask;

    [Header("Fuel")]
    public float fuelCapacity = 100f;
    public float fuelBurnRate = 0.6f;

    [Header("Live Readouts (read only)")]
    [SerializeField] private float throttlePct;
    [SerializeField] private float airspeedKnots;
    [SerializeField] private float altitudeFeet;
    [SerializeField] private float aoaDegrees;
    [SerializeField] private bool engineAlive = true;
    [SerializeField] private bool isGrounded;

    private Rigidbody rb;
    private float throttle = 0f;
    private float fuelRemaining;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 650f;
        rb.linearDamping = 0.02f;
        rb.angularDamping = 2.5f;
        rb.maxAngularVelocity = 3f;
        rb.centerOfMass = new Vector3(0, 0, 0.5f);
        fuelRemaining = fuelCapacity;
        Debug.Log("Forward: " + transform.forward + " Up: " + transform.up + " Right: " + transform.right);
    }

    void Update()
    {
      HandleThrottle();
        UpdateReadouts();
    }

    void FixedUpdate()
    {
        isGrounded = CheckGrounded();
        ApplyThrust();
        ApplyAerodynamics();
        ApplyControlSurfaces();
        ApplyBrakes();
        
        throttle = Mathf.MoveTowards(throttle, 1f, throttleRampSpeed * Time.fixedDeltaTime);
    }

    void HandleThrottle()
    {

        if (Input.GetKey(KeyCode.T)) throttle = Mathf.MoveTowards(throttle, 1f, throttleRampSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.G)) throttle = Mathf.MoveTowards(throttle, 0f, throttleRampSpeed * Time.deltaTime);
        Debug.Log("Throttle: " + throttle);
    }

    void ApplyThrust()
    {
        if (!engineAlive) return;
        if (fuelRemaining <= 0f) { engineAlive = false; return; }

        // transform.up is forward for this plane's orientation
        rb.AddForce(transform.forward * throttle * maxThrust, ForceMode.Force); Debug.Log("Force: " + (-transform.forward * throttle * maxThrust));

        fuelRemaining -= (throttle * fuelBurnRate * Time.fixedDeltaTime) / 60f;
        fuelRemaining = Mathf.Max(fuelRemaining, 0f);
    }

    void ApplyAerodynamics()
    {
        float airspeed = rb.linearVelocity.magnitude;
        if (airspeed < 0.5f) return;
        if (isGrounded) return;

        // AoA calculated against transform.up (our actual forward)
        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
        float aoa = Mathf.Atan2(-localVel.z, localVel.y) * Mathf.Rad2Deg;
        aoaDegrees = aoa;

        float absAoa = Mathf.Abs(aoa);
        float stallFactor = Mathf.Clamp01(1f - Mathf.InverseLerp(stallAngle * 0.6f, stallAngle, absAoa));

        float speedFactor = Mathf.Clamp01((airspeed - minLiftSpeed) / minLiftSpeed);
        float weight = rb.mass * Mathf.Abs(Physics.gravity.y);
        float lift = weight * speedFactor * stallFactor * liftMultiplier;

        // Lift acts perpendicular to forward — which is -transform.forward in this orientation
        rb.AddForce(-transform.forward * lift, ForceMode.Force);

        float aoaDragFactor = 1f + (absAoa / stallAngle);
        float drag = dragMultiplier * airspeed * airspeed * aoaDragFactor * 0.04f;
        rb.AddForce(-rb.linearVelocity.normalized * drag, ForceMode.Force);
    }

    void ApplyControlSurfaces()
    {
        float airspeed = rb.linearVelocity.magnitude;
        float effectiveness = Mathf.Clamp01(airspeed / 35f);

        // Pitch W/S — rotates around right axis
        float pitch = 0f;
        if (Input.GetKey(KeyCode.W)) pitch = -1f;
        if (Input.GetKey(KeyCode.S)) pitch = 1f;
        rb.AddTorque(transform.right * pitch * pitchTorque * effectiveness, ForceMode.Force);

        // Roll Q/E — rotates around up axis (our actual forward)
        float roll = 0f;
        if (Input.GetKey(KeyCode.Q)) roll = 1f;
        if (Input.GetKey(KeyCode.E)) roll = -1f;
        rb.AddTorque(transform.up * roll * rollTorque * effectiveness, ForceMode.Force);

        // Yaw A/D — rotates around forward axis (which is -transform.forward here)
        float yaw = 0f;
        if (Input.GetKey(KeyCode.A)) yaw = -1f;
        if (Input.GetKey(KeyCode.D)) yaw = 1f;
        rb.AddTorque(-transform.forward * yaw * yawTorque * effectiveness, ForceMode.Force);

        // Adverse yaw
        rb.AddTorque(-transform.forward * (-roll * 0.3f) * yawTorque * effectiveness, ForceMode.Force);

        // Banked turn
        float bankAmount = Vector3.Dot(transform.right, Vector3.up);
        rb.AddTorque(transform.forward * bankAmount * 500f, ForceMode.Force);
    }

    void ApplyBrakes()
    {
        if (!isGrounded) return;
        if (!Input.GetKey(KeyCode.B)) return;

        Vector3 brakeDir = -rb.linearVelocity;
        brakeDir.y = 0f;
        rb.AddForce(brakeDir.normalized * brakeForce, ForceMode.Force);
    }

    bool CheckGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 2f, groundMask);
    }

    void UpdateReadouts()
    {
        throttlePct = throttle * 100f;
        airspeedKnots = rb.linearVelocity.magnitude * 1.944f;
        altitudeFeet = transform.position.y * 3.281f;
    }
}