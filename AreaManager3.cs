using UnityEngine;

public class AreaManager3 : MonoBehaviour
{
    private bool destroyed = false;

    public void NotifyAreaDestroyed()
    {
        if (destroyed) return;
        destroyed = true;

        Debug.Log("Area 3 hancur.");
        GameManager.Instance.AreaDestroyed();
    }
}
