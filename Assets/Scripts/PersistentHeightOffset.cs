using UnityEngine;
using Unity.XR.CoreUtils;

[DefaultExecutionOrder(100)]
public class PersistentHeightOffset : MonoBehaviour
{
    [Tooltip("Offset applied to the CameraFloorOffsetObject Y position.")]
    public float heightOffset = -0.60f;

    private XROrigin xrOrigin;
    private bool applied = false;

    void Start()
    {
        xrOrigin = FindObjectOfType<XROrigin>();
        ApplyOffset();
    }

void LateUpdate()
{
    if (xrOrigin != null && xrOrigin.CameraFloorOffsetObject != null)
    {
        Vector3 current = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
        if (Mathf.Abs(current.y - heightOffset) > 0.001f)
        {
            current.y = heightOffset;
            xrOrigin.CameraFloorOffsetObject.transform.localPosition = current;
            Debug.Log($"🔁 Re-applied height offset {heightOffset}.");
        }
    }
}


    void ApplyOffset()
    {
        if (xrOrigin != null && xrOrigin.CameraFloorOffsetObject != null)
        {
            Vector3 current = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
            if (Mathf.Abs(current.y - heightOffset) > 0.001f)
            {
                current.y = heightOffset;
                xrOrigin.CameraFloorOffsetObject.transform.localPosition = current;
                Debug.Log($"✅ Height offset {heightOffset} applied to CameraFloorOffsetObject.");
            }
            applied = true;
        }
        else
        {
            Debug.LogWarning("⚠️ XR Origin or CameraFloorOffsetObject not found.");
        }
    }
}
