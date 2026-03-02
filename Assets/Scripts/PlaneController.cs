using UnityEngine;

public class PlaneController : MonoBehaviour
{
    [Header("Aircraft Specifications")]
    public float mass = 950f;
    public float maxThrust = 15000f;
    public float wingArea = 16.2f;

    [Header("Aerodynamic Coefficients")]
    public float CL0 = 0.3f;
    public float CLaPerDeg = 0.08f;
    public float CLmax = 1.4f;
    public float CLmin = -0.5f;
    public float CD0 = 0.02f;
    public float CDi = 0.03f;

    [Header("Control Surfaces")]
    public float pitchRate = 50f;
    public float rollRate = 60f;
    public float yawRate = 25f;

    [Header("Ground Handling")]
    public float groundDrag = 0.001f;
    public float brakeForce = 8000f;
    public float wheelHeight = 1.5f;
    public float minFlySpeed = 25f;

    [Header("Input")]
    public float throttle = 0f;

    private const float airDensity = 1.225f;
    private const float gravity = 9.81f;

    private Rigidbody rb;
    private bool isGrounded = false;
    private float currentAoA = 0f;
    private float currentCL = 0f;
    private float currentLift = 0f;

    public float GetThrottle() { return throttle; }
    public float GetAoA() { return currentAoA; }
    public float GetLift() { return currentLift; }
    public float GetSpeed() { return rb.linearVelocity.magnitude; }
    public float GetAltitude() { return transform.position.y; }
    public Vector3 GetVelocity() { return rb.linearVelocity; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.useGravity = true;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.5f;
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        rb.linearDamping = 0f;

        CheckGrounded();

        float speed = rb.linearVelocity.magnitude;

        CalculateAoA(speed);

        // ONLY apply lift if going fast enough
        if (speed > minFlySpeed)
        {
            ApplyLift(speed);
        }

        if (speed > 1f)
        {
            ApplyDrag(speed);
        }

        ApplyThrust();

        if (isGrounded)
        {
            ApplyGroundForces();
        }

        float speed_mph = speed * 2.237f;
        Debug.Log($"SPD:{speed_mph:F0}mph AOA:{currentAoA:F1} CL:{currentCL:F2} LIFT:{currentLift:F0} GND:{isGrounded}");
    }

    void HandleInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            throttle = Mathf.Clamp01(throttle + Time.deltaTime * 2f);
        if (Input.GetKey(KeyCode.LeftControl))
            throttle = Mathf.Clamp01(throttle - Time.deltaTime * 2f);

        float pitch = 0f;
        if (Input.GetKey(KeyCode.W)) pitch = 1f;
        if (Input.GetKey(KeyCode.S)) pitch = -1f;

        float roll = 0f;
        if (Input.GetKey(KeyCode.Q)) roll = 1f;
        if (Input.GetKey(KeyCode.E)) roll = -1f;

        float yaw = 0f;
        if (Input.GetKey(KeyCode.A)) yaw = -1f;
        if (Input.GetKey(KeyCode.D)) yaw = 1f;

        Vector3 rotation = new Vector3(
            pitch * pitchRate * Time.deltaTime,
            yaw * yawRate * Time.deltaTime,
            roll * rollRate * Time.deltaTime
        );

        transform.Rotate(rotation, Space.Self);
    }

    void CalculateAoA(float speed)
    {
        if (speed < 1f)
        {
            currentAoA = 0f;
            return;
        }

        // Get velocity direction
        Vector3 velDir = rb.linearVelocity.normalized;

        // AoA is angle between velocity and forward, measured in pitch plane
        // Use Dot product for a simpler, more reliable calculation
        float forwardDot = Vector3.Dot(velDir, transform.forward);
        float upDot = Vector3.Dot(velDir, transform.up);

        // Calculate AoA using atan2 for proper quadrant handling
        currentAoA = Mathf.Atan2(-upDot, forwardDot) * Mathf.Rad2Deg;

        // Clamp to reasonable range
        currentAoA = Mathf.Clamp(currentAoA, -30f, 30f);
    }

    void ApplyLift(float speed)
    {
        currentCL = CL0 + CLaPerDeg * currentAoA;
        currentCL = Mathf.Clamp(currentCL, CLmin, CLmax);

        float dynamicPressure = 0.5f * airDensity * speed * speed;
        currentLift = dynamicPressure * wingArea * currentCL;

        // Lift acts perpendicular to velocity, in the "up" direction of the plane
        // Use transform.up for simplicity - this is good enough for most flight
        Vector3 liftForce = transform.up * currentLift;

        rb.AddForce(liftForce);
    }

    void ApplyDrag(float speed)
    {
        float CD = CD0 + CDi * currentCL * currentCL;
        float dynamicPressure = 0.5f * airDensity * speed * speed;
        float dragMagnitude = dynamicPressure * wingArea * CD;
        rb.AddForce(-rb.linearVelocity.normalized * dragMagnitude);
    }

    void ApplyThrust()
    {
        rb.AddForce(transform.forward * maxThrust * throttle);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, wheelHeight + 0.1f);
    }

    void ApplyGroundForces()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Vector3 friction = -rb.linearVelocity.normalized * groundDrag * mass * gravity;
            rb.AddForce(friction);

            if (Input.GetKey(KeyCode.B))
            {
                rb.AddForce(-rb.linearVelocity.normalized * brakeForce);
            }
        }

        Vector3 euler = transform.eulerAngles;
        if (euler.x > 180) euler.x -= 360;
        if (euler.z > 180) euler.z -= 360;
        euler.x = Mathf.Clamp(euler.x, -5f, 15f);
        euler.z = Mathf.Lerp(euler.z, 0f, Time.fixedDeltaTime * 5f);
        transform.eulerAngles = euler;
    }
}