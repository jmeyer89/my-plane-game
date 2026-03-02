using UnityEngine;
using TMPro;

public class PlaneCollision : MonoBehaviour
{
    public float crashSpeedThreshold = 40f;  // was 15f
    public GameObject explosionEffect;
    public TextMeshProUGUI crashText;
    private PlaneController planeController;
    private bool crashed = false;

    void Start()
    {
        planeController = GetComponent<PlaneController>();
        if (crashText != null)
            crashText.gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (crashed) return;

        // Get the impact direction
        Vector3 impactNormal = collision.contacts[0].normal;

        // How much of the velocity is INTO the surface (not along it)
        float impactSpeed = Vector3.Dot(collision.relativeVelocity, impactNormal);

        // Only crash on hard impacts, not rolling/sliding
        if (impactSpeed > crashSpeedThreshold)
        {
            crashed = true;
            CrashPlane();
        }
    }

    void CrashPlane()
    {
        if (planeController != null)
            planeController.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearDamping = 2f;
        rb.angularDamping = 1f;

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (crashText != null)
        {
            crashText.gameObject.SetActive(true);
            crashText.text = "Yawful";
        }

        Debug.Log("CRASH - Yawful");
    }
}