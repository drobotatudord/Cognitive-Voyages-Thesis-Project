using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using TMPro;

public class WalkInPlace : LocomotionProvider
{
    [Header("References")]
    public Transform xrCamera;
    public GameObject walkingStatusUI;
    public InputActionProperty toggleMovementAction; // Assigned to Left Thumbstick Click

    [Header("Movement Settings")]
    public float sensitivity = 0.01f;
    public float requiredBobIntensity = 0.02f;
    public float stepSpeed = 1.5f;
    public float smoothing = 5f;
    public float deadzone = 0.005f;
    public float activationTime = 0.3f;

    [Header("UI Fade Settings")]
    [SerializeField] private float fadeSpeed = 3f;
    private Color targetColor;

    [Header("Tutorial Settings")]
    public int maxToggles = 3;
    private int toggleCount = 0;
    private bool tutorialFinished = false;

    private float lastY;
    private float velocityY;
    private float moveAmount;
    private float bobTimer;
    private bool isBobActive;
    private bool movementEnabled = false;

    private XROrigin xrOrigin;
    private TMP_Text uiText;

    protected override void Awake()
    {
        base.Awake();

        if (xrCamera == null)
            xrCamera = Camera.main.transform;

        xrOrigin = system.xrOrigin;
        lastY = xrCamera.localPosition.y;

        if (walkingStatusUI != null)
            uiText = walkingStatusUI.GetComponentInChildren<TMP_Text>();

        if (uiText != null)
        {
            targetColor = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 1f);
            UpdateUI(); // Set initial text
        }
    }

    private void OnEnable()
    {
        toggleMovementAction.action.Enable();
    }

    private void OnDisable()
    {
        toggleMovementAction.action.Disable();
    }

    void Update()
    {
        // ✅ Allow toggling walk mode anytime
        if (toggleMovementAction.action.WasPressedThisFrame())
        {
            movementEnabled = !movementEnabled;

            if (!tutorialFinished)
            {
                toggleCount++;
                UpdateUI();

                if (toggleCount >= maxToggles)
                {
                    tutorialFinished = true;

                    if (walkingStatusUI != null)
                        walkingStatusUI.SetActive(false); // ⛔ Hide UI permanently
                }
            }
        }

        // 🛑 Skip walking logic if needed
        if (!CanBeginLocomotion() || xrOrigin == null || !movementEnabled)
        {
            moveAmount = Mathf.Lerp(moveAmount, 0f, Time.deltaTime * smoothing * 2f);

            // ✨ Fade UI only if tutorial is active
            if (uiText != null && !tutorialFinished)
                uiText.color = Color.Lerp(uiText.color, targetColor, Time.deltaTime * fadeSpeed);

            return;
        }

        // 👣 Head bob detection
        float currentY = xrCamera.localPosition.y;
        velocityY = (currentY - lastY) / Time.deltaTime;
        float bobPower = Mathf.Abs(velocityY);
        lastY = currentY;

        if (bobPower > requiredBobIntensity)
        {
            bobTimer += Time.deltaTime;
            isBobActive = bobTimer >= activationTime;
        }
        else
        {
            bobTimer = 0f;
            isBobActive = false;
        }

        // 🚶 Smooth movement
        if (isBobActive)
            moveAmount = Mathf.Lerp(moveAmount, stepSpeed, Time.deltaTime * smoothing);
        else
            moveAmount = Mathf.Lerp(moveAmount, 0f, Time.deltaTime * smoothing * 2f);

        // 🎮 Apply movement
        if (moveAmount > deadzone)
        {
            if (BeginLocomotion())
            {
                Vector3 forward = xrCamera.forward;
                forward.y = 0;
                forward.Normalize();

                CharacterController characterController = xrOrigin.GetComponent<CharacterController>();
                if (characterController != null)
{
    Vector3 motion = forward * moveAmount * Time.deltaTime;

// 🚫 Check if player is about to walk into an unclimbable object
RaycastHit forwardHit;
Vector3 rayOrigin = xrCamera.position;
Vector3 rayDirection = forward;

if (Physics.Raycast(rayOrigin, rayDirection, out forwardHit, 0.5f))
{
    if (forwardHit.collider.CompareTag("Unclimbable"))
    {
        Debug.Log("⛔ Blocked by Unclimbable object");
        EndLocomotion();
        return;
    }
}

    characterController.Move(motion);

    if (!characterController.isGrounded)
    {
        RaycastHit hit;
        if (Physics.Raycast(xrCamera.position, Vector3.down, out hit, 1f))
        {
            if (hit.collider.CompareTag("Ground")) // Optional tag check
            {
                characterController.Move(Vector3.down * 0.05f);
            }
        }
    }
}


                EndLocomotion();
            }
        }

        // 🖼 Smooth UI fade while active
        if (uiText != null && !tutorialFinished)
            uiText.color = Color.Lerp(uiText.color, targetColor, Time.deltaTime * fadeSpeed);
    }

    private void UpdateUI()
    {
        if (uiText == null || tutorialFinished) return;

        string tutorialStep = toggleCount switch
        {
            0 => "Step 1: Press the left thumbstick to START walking",
            1 => "Step 2: Press again to STOP walking",
            2 => "Step 3: Press again to START walking again",
            _ => ""
        };

        if (movementEnabled)
        {
            uiText.text = $"🚶 Walking Enabled\n{tutorialStep}";
            targetColor = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 125f / 255f);
        }
        else
        {
            uiText.text = $"⏸️ Walking Disabled\n{tutorialStep}";
            targetColor = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 1f);
        }
    }

private bool syncedOnce = false;

void LateUpdate()
{
    if (!syncedOnce && system != null && system.xrOrigin != null)
    {
        var wrapper = system.xrOrigin.GetComponent<CharacterControllerDriverWrapper>();
        if (wrapper != null)
        {
            wrapper.ForceUpdateCharacterController();
            syncedOnce = true;
        }
    }
}


}