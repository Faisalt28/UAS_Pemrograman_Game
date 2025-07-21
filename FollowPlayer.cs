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

        // Rotasi hanya jika TIDAK menyentuh analog
        if (!IsTouchingUIWithTag("IgnoreCamera"))
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // Tetap follow player, rotasi kamera mengarah ke player
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = player.position + Vector3.up * height + (rotation * Vector3.back * distance);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);
        transform.LookAt(player.position + Vector3.up * 1.5f);
    }

    // Cek apakah touch/mouse sedang di atas UI dengan tag tertentu
    bool IsTouchingUIWithTag(string tag)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            return IsPointerOverUIWithTag(touch.position, tag);
        }
#else
        if (Input.GetMouseButton(0))
        {
            return IsPointerOverUIWithTag(Input.mousePosition, tag);
        }
#endif
        return false;
    }

    // Cek jika pointer berada di atas UI dengan tag
    bool IsPointerOverUIWithTag(Vector2 screenPosition, string tag)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            if (result.gameObject.CompareTag(tag))
                return true;
        }
        return false;
    }
}
