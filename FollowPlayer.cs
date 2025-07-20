using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("Target")]
    public Transform player;        // Objek yang diikuti (player)

    [Header("Camera Settings")]
    public float distance = 3f;     // Jarak kamera dari player (mundur)
    public float height = 2f;       // Ketinggian kamera dari player
    public float sensitivity = 2f;  // Sensitivitas rotasi kamera
    public float smoothSpeed = 10f; // Kehalusan pergerakan kamera

    [Header("Clamp Pitch")]
    public float minPitch = -20f;   // Batas rotasi ke bawah
    public float maxPitch = 60f;    // Batas rotasi ke atas

    private float yaw = 0f;         // Rotasi horizontal (mouse X)
    private float pitch = 10f;      // Rotasi vertikal (mouse Y)

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (player == null) return;

        // Input mouse
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        // Zoom kamera pakai scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * 5f, 2f, 10f);

        // Hitung rotasi kamera
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Hitung posisi target kamera (di atas + di belakang player)
        Vector3 desiredPosition = player.position + Vector3.up * height + (rotation * Vector3.back * distance);

        // Smooth pergerakan kamera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        // Kamera selalu melihat ke player (bagian atas tubuh)
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}
