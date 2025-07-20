using UnityEngine;

public class AreaManager1 : MonoBehaviour
{
    private bool destroyed = false;

    public void NotifyAreaDestroyed()
    {
        if (destroyed) return;
        destroyed = true;

        Debug.Log("Area 1 hancur.");
        GameManager.Instance.AreaDestroyed();
    }
}
