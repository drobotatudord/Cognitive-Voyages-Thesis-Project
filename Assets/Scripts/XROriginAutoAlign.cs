using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class XROriginAutoAlign : MonoBehaviour
{
    private XROrigin xrOrigin;

    private IEnumerator Start()
    {
        yield return null; // Wait one frame for XR system to initialize

        xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null && xrOrigin.Camera != null)
        {
            Debug.Log("📍 Auto-aligning XR Origin to headset start position...");
            xrOrigin.MoveCameraToWorldLocation(xrOrigin.Camera.transform.position);
        }
        else
        {
            Debug.LogWarning("⚠️ XR Origin or Camera not found for auto-align.");
        }
    }

    [ContextMenu("Reset XR Origin")]
    void ResetXROrigin()
    {
        if (xrOrigin != null && xrOrigin.Camera != null)
        {
            Vector3 currentCameraPosition = xrOrigin.Camera.transform.position;
            xrOrigin.MoveCameraToWorldLocation(currentCameraPosition);
            Debug.Log("📍 XR Origin reset to match current HMD position.");
        }
        else
        {
            Debug.LogWarning("⚠️ Cannot reset XR Origin — reference not found.");
        }
    }
}