using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleLift : MonoBehaviour
{
    public float minLiftSpeed = 30f;
    public float maxLiftSpeed = 120f;
    public float liftMultiplier = .9f;
    public float pitchSensitivity = 0.25f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("SimpleLift NEW running");
    }

    void FixedUpdate()
    {
        // TEST: press L to force upward lift
        if (Input.GetKey(KeyCode.L))
        {
            rb.AddForce(Vector3.up * 8000f, ForceMode.Force);
        }

        float speed = rb.linearVelocity.magnitude;
        if (speed < minLiftSpeed) return;

        float tSpeed = Mathf.InverseLerp(minLiftSpeed, maxLiftSpeed, speed);

        // nose-up factor
        float pitchUpSigned = Vector3.Dot(transform.forward, Vector3.up);

        float pitchFactor = Mathf.Clamp01((pitchUpSigned + pitchSensitivity) / (1f + pitchSensitivity));
        pitchFactor = Mathf.Max(0.5f, pitchFactor);

        float weight = rb.mass * Physics.gravity.magnitude;

        float lift = weight * tSpeed * pitchFactor * liftMultiplier;

        rb.AddForce(Vector3.up * lift, ForceMode.Force);
    }
}