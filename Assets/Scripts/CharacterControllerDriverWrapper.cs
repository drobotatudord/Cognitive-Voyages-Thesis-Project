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

    void Awake()
    {
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
    }

private void Start()
{
    StartCoroutine(ForceHeightAfterDelay());
}

private IEnumerator ForceHeightAfterDelay()
{
    float timer = 0f;
    while ((xrOrigin == null || characterController == null || xrOrigin.Camera == null) && timer < 3f)
    {
        yield return new WaitForSeconds(0.1f);
        timer += 0.1f;

        // Try re-fetching references in case XR initialized late
        driver = GetComponent<CharacterControllerDriver>();
        BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var xrOriginField = typeof(CharacterControllerDriver).GetField("m_XROrigin", flags);
        var ccField = typeof(CharacterControllerDriver).GetField("m_CharacterController", flags);
        xrOrigin = xrOriginField?.GetValue(driver) as XROrigin;
        characterController = ccField?.GetValue(driver) as CharacterController;
    }

    if (xrOrigin == null || characterController == null || xrOrigin.Camera == null)
    {
        Debug.LogError("❌ Still missing references after delay.");
        yield break;
    }

    Debug.Log("⏳ Forcing character height after delay (post-wait)...");
    ForceUpdateCharacterController(); // ⬅️ keep this line

    // 🔻 ADD THIS RIGHT AFTER:
    if (xrOrigin.CameraFloorOffsetObject != null)
    {
        Vector3 offset = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
        offset.y -= 0.25f; // Slight lowering for all players
        xrOrigin.CameraFloorOffsetObject.transform.localPosition = offset;
        Debug.Log($"📉 Manually lowered CameraFloorOffsetObject to {offset.y}");
    }
}

    private IEnumerator SnapToGroundOnce()
    {
        yield return new WaitForSeconds(0.25f); // Let camera pose stabilize

        if (xrOrigin != null && xrOrigin.Camera != null && characterController != null)
        {
            Vector3 cameraPos = xrOrigin.Camera.transform.position;

            if (Physics.Raycast(cameraPos, Vector3.down, out RaycastHit hit, 2f))
            {
                float offset = hit.distance - (characterController.height / 2f);
                if (Mathf.Abs(offset) > 0.05f)
                {
                    characterController.Move(Vector3.down * offset);
                    Debug.Log("📏 Snapped to ground at start.");
                }
            }
        }
    }

    private bool heightInitialized = false;

public void ForceUpdateCharacterController()
{
    if (xrOrigin == null || characterController == null || xrOrigin.Camera == null)
    {
        Debug.LogWarning("❌ XR Origin or Character Controller is missing.");
        return;
    }

    float cameraHeight = xrOrigin.CameraInOriginSpaceHeight;
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

    // ✅ Avoid ceiling clip just in case
    if (Physics.Raycast(xrOrigin.Camera.transform.position, Vector3.up, out RaycastHit ceilingCheck, 0.3f))
    {
        characterController.Move(Vector3.down * 0.2f);
        Debug.Log("⬇️ Nudging down to avoid ceiling clip.");
    }

    heightInitialized = true;
}

}
