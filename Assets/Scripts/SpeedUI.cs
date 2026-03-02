using UnityEngine;

public class SpeedUI : MonoBehaviour
{
    private PlaneController planeController;
    private Rigidbody rb;

    private GUIStyle labelStyle;
    private GUIStyle boxStyle;

    const float MS_TO_MPH = 2.237f;
    const float MS_TO_KTS = 1.944f;
    const float M_TO_FT = 3.281f;
    const float MS_TO_FPM = 196.85f; // m/s -> ft/min

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        planeController = GetComponent<PlaneController>();
    }

    void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = Color.green;

            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.5f));
        }

        float speedMps = rb ? rb.linearVelocity.magnitude : 0f;
        float speedMph = speedMps * MS_TO_MPH;
        float speedKts = speedMps * MS_TO_KTS;

        float altitudeFeet = transform.position.y * M_TO_FT;

        float throttlePct = planeController != null ? planeController.GetThrottle() * 100f : 0f;

        float verticalSpeedFpm = rb ? rb.linearVelocity.y * MS_TO_FPM : 0f;

        float boxX = 20f;
        float boxY = Screen.height - 200f;
        float boxW = 260f;
        float boxH = 180f;

        GUI.Box(new Rect(boxX, boxY, boxW, boxH), "", boxStyle);

        GUI.Label(new Rect(boxX + 10, boxY + 10, boxW, 25), $"AIRSPEED:  {speedMph:0} mph  ({speedKts:0} kt)", labelStyle);
        GUI.Label(new Rect(boxX + 10, boxY + 40, boxW, 25), $"ALTITUDE:  {altitudeFeet:0} ft", labelStyle);
        GUI.Label(new Rect(boxX + 10, boxY + 70, boxW, 25), $"THROTTLE:  {throttlePct:0}%", labelStyle);
        GUI.Label(new Rect(boxX + 10, boxY + 100, boxW, 25), $"VERT SPD:  {verticalSpeedFpm:0} fpm", labelStyle);

        GUI.Label(new Rect(boxX + 10, boxY + 130, 40, 25), "THR", labelStyle);
        GUI.Box(new Rect(boxX + 55, boxY + 133, 180, 15), "", boxStyle);
        GUI.DrawTexture(new Rect(boxX + 55, boxY + 133, 180f * (throttlePct / 100f), 15), MakeTexture(2, 2, Color.green));
    }

    private Texture2D MakeTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}