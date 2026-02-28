using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimplePlaneController : MonoBehaviour
{
    public float thrustForce = 3200f;

    public float yawTorque = 3000f;     // A / D
    public float pitchTorque = 8000f;   // W / S
    public float rollTorque = 6000f;    // Q / E

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().maxAngularVelocity = 2f;
    }

    void FixedUpdate()
    {
        // Throttle (Space)
        if (Input.GetKey(KeyCode.Space))
            rb.AddForce(transform.forward * thrustForce, ForceMode.Force);

        // Yaw (A/D)
        float yaw = 0f;
        if (Input.GetKey(KeyCode.A)) yaw = -1f;
        if (Input.GetKey(KeyCode.D)) yaw = 1f;

        rb.AddTorque(transform.up * yaw * yawTorque, ForceMode.Force);

        // Pitch (W/S)
        float pitch = 0f;
        if (Input.GetKey(KeyCode.W)) pitch = -1f;   // swap if inverted
        if (Input.GetKey(KeyCode.S)) pitch = 1f;

        rb.AddTorque(transform.right * pitch * pitchTorque, ForceMode.Force);

        // Roll (Q/E)
        float roll = 0f;
        if (Input.GetKey(KeyCode.Q)) roll = 1f;
        if (Input.GetKey(KeyCode.E)) roll = -1f;

        rb.AddTorque(transform.forward * roll * rollTorque, ForceMode.Force);

        // Banked turn effect
        float bankAmount = Vector3.Dot(transform.right, Vector3.up);
        rb.AddTorque(-transform.up * bankAmount * 400f, ForceMode.Force);
    }
}