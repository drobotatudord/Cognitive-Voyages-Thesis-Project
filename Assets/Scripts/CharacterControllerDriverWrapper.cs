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

    void Start()
    {
        StartCoroutine(SnapToGroundOnce());
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
    if (heightInitialized || xrOrigin == null || characterController == null)
        return;

    float cameraHeight = xrOrigin.CameraInOriginSpaceHeight;
    float clampedHeight = Mathf.Clamp(cameraHeight, 0.9f, 1.8f);
    float newHeight = clampedHeight + 0.1f;

    characterController.height = newHeight;
    characterController.center = new Vector3(0, newHeight / 2f, 0);

    // ✅ Optional: snap to ground
    if (Physics.Raycast(xrOrigin.Camera.transform.position, Vector3.down, out RaycastHit hit, 2f))
    {
        float difference = hit.distance - clampedHeight * 0.5f;
        if (Mathf.Abs(difference) > 0.05f)
        {
            characterController.Move(new Vector3(0, difference, 0));
        }
    }

    // ✅ Optional: ceiling nudge
    if (Physics.Raycast(xrOrigin.Camera.transform.position, Vector3.up, out RaycastHit ceilingCheck, 0.3f))
    {
        characterController.Move(Vector3.down * 0.2f);
        Debug.Log("⬇️ Nudging down to avoid ceiling clip.");
    }

    heightInitialized = true;
}

}
