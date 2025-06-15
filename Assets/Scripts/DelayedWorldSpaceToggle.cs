using UnityEngine;

public class DelayedWorldSpaceToggle : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public bool enableWorldSpace = true;

    void Start()
    {
        if (lineRenderer != null)
        {
            Invoke(nameof(ApplyToggle), 1f); // Wait 1 second
        }
        else
        {
            Debug.LogWarning("❌ LineRenderer reference not assigned!");
        }
    }

    void ApplyToggle()
    {
        lineRenderer.useWorldSpace = enableWorldSpace;
        Debug.Log($"✅ LineRenderer.useWorldSpace set to {enableWorldSpace}");
    }
}