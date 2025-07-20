using UnityEngine;

public class AreaManager2 : MonoBehaviour
{
    private bool destroyed = false;

    public void NotifyAreaDestroyed()
    {
        if (destroyed) return;
        destroyed = true;

        Debug.Log("Area 2 hancur.");
        GameManager.Instance.AreaDestroyed();
    }
}
