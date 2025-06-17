using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Reflection;
using System.Collections;
using Unity.XR.CoreUtils;

[RequireComponent(typeof(CharacterControllerDriver))]
public class CharacterControllerDriverWrapper : MonoBehaviour
{
    private CharacterControllerDriver driver;
    private XROrigin xrOrigin;
    private CharacterController characterController;

private bool offsetApplied = false;

private float baseCameraHeight = -1f;

private bool heightInitialized = false;

private float GetCalibratedHeight()
{
    if (baseCameraHeight < 0f && xrOrigin != null && xrOrigin.Camera != null)
    {
        baseCameraHeight = xrOrigin.CameraInOriginSpaceHeight;
        Debug.Log($"📏 Calibrated Base Camera Height: {baseCameraHeight}");
    }
    return baseCameraHeight;
}

void Awake()
{
    ApplyPersistentYOffset(force: true); // ← EARLY APPLICATION
    driver = GetComponent<CharacterControllerDriver>();

    // Use reflection to access Unity's private fields
    BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

    var xrOriginField = typeof(CharacterControllerDriver).GetField("m_XROrigin", flags);
    var ccField = typeof(CharacterControllerDriver).GetField("m_CharacterController", flags);

    xrOrigin = xrOriginField?.GetValue(driver) as XROrigin;
    characterController = ccField?.GetValue(driver) as CharacterController;

    if (xrOrigin == null || characterController == null)
    {
        Debug.LogError("❌ Failed to access XROrigin or CharacterController via reflection.");
    }
    else
    {
        // ✅ Only set tracking mode if xrOrigin was found
        xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
    }
}

private void Start()
{
    StartCoroutine(InitializeXRCharacterController());
}

public void ApplyPersistentYOffset(bool force = false)
{
    if ((offsetApplied && !force) || xrOrigin == null || xrOrigin.CameraFloorOffsetObject == null)
        return;

    Vector3 offset = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
    offset.y = -0.90f;
    xrOrigin.CameraFloorOffsetObject.transform.localPosition = offset;

    offsetApplied = true;
    Debug.Log("📏 Persistent Y-offset (-0.30) applied to CameraFloorOffsetObject.");
}


public void ForceUpdateCharacterController()
{
    if (xrOrigin == null || characterController == null || xrOrigin.Camera == null)
    {
        Debug.LogWarning("❌ XR Origin or Character Controller is missing.");
        return;
    }

    float cameraHeight = Mathf.Clamp(GetCalibratedHeight(), 0.9f, 1.65f);

    float clampedHeight = Mathf.Clamp(cameraHeight, 0.9f, 1.65f);
    float newHeight = clampedHeight + 0.02f; // very close fit

    // ✅ Set height and center first
    characterController.height = newHeight;
    characterController.center = new Vector3(0, newHeight / 2f, 0);
    Debug.Log($"📏 Setting CharacterController height to {newHeight}, center to {characterController.center}");

    // ✅ Small downward nudge to feel more grounded
    characterController.Move(Vector3.down * 0.02f);

    // ✅ Snap to ground if slightly off
    if (Physics.Raycast(xrOrigin.Camera.transform.position, Vector3.down, out RaycastHit hit, 2f))
    {
        float difference = hit.distance - clampedHeight * 0.5f;
        if (Mathf.Abs(difference) > 0.05f)
        {
            characterController.Move(new Vector3(0, difference, 0));
        }
    }

    // 🔁 Force camera offset back to -0.90f again (teleport safe)
if (xrOrigin.CameraFloorOffsetObject != null)
{
    Vector3 offset = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
    offset.y = -0.90f;
    xrOrigin.CameraFloorOffsetObject.transform.localPosition = offset;
    Debug.Log("📏 Re-applied -0.90f offset after character controller sync.");
}


    heightInitialized = true;
}

private IEnumerator InitializeXRCharacterController()
{
    yield return new WaitUntil(() =>
        xrOrigin != null &&
        xrOrigin.Camera != null &&
        xrOrigin.CameraFloorOffsetObject != null &&
        characterController != null
    );

    yield return null;

    ApplyPersistentYOffset(force: true);       // ✅ Always apply
    ForceUpdateCharacterController();

    if (driver != null)
    {
        driver.enabled = false;
        Debug.Log("✅ CharacterControllerDriver disabled after setup.");
    }

    Debug.Log("✅ Character Controller initialized for all movement modes.");
}
}
