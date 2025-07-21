using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float distance = 3f;
    public float height = 2f;
    public float sensitivity = 0.5f;
    public float smoothSpeed = 10f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    private float yaw = 0f;
    private float pitch = 10f;
    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (player == null) return;

        // 🌀 Rotasi jika swipe/touch TIDAK di atas elemen UI (seperti analog/tombol)
        if (IsScreenTouched() && !IsPointerOverUI())
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // 📸 Posisi kamera
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = player.position + Vector3.up * height + (rotation * Vector3.back * distance);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }

    // Cek apakah ada sentuhan (Android/iOS atau klik di PC)
    bool IsScreenTouched()
    {
#if UNITY_ANDROID || UNITY_IOS
        return Input.touchCount > 0;
#else
        return Input.GetMouseButton(0);
#endif
    }

    // Cek apakah sentuhan/klik berada di atas elemen UI
    bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            eventData.position = Input.GetTouch(0).position;
#else
        eventData.position = Input.mousePosition;
#endif
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
