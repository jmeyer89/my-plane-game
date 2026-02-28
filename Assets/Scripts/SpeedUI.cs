using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpeedUI : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnGUI()
    {
        float speed = rb.linearVelocity.magnitude;
        GUI.Label(new Rect(10, 10, 300, 30), "Speed: " + speed.ToString("0.0"));
    }
}