using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using System.Collections;
using TMPro;

public class WalkInPlace : LocomotionProvider
{
    /* [Header("References")]
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

    private bool syncedOnce = false;

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

    private void Start()
    {
        var wrapper = xrOrigin?.GetComponent<CharacterControllerDriverWrapper>();
        if (wrapper != null)
        {
            wrapper.ForceUpdateCharacterController();
            wrapper.ApplyPersistentYOffset();
        }

        SnapToGroundIfNeeded(); // Immediate
        StartCoroutine(SnapToGroundAgain()); // Optional backup
    }

    private IEnumerator SnapToGroundAgain()
    {
        yield return new WaitForSeconds(0.2f);
        SnapToGroundIfNeeded();
    }

    private void OnEnable()
    {
        toggleMovementAction.action.Enable();

        var wrapper = xrOrigin?.GetComponent<CharacterControllerDriverWrapper>();
        if (wrapper != null)
            wrapper.ApplyPersistentYOffset();
    }

    private void OnDisable()
    {
        toggleMovementAction.action.Disable();
    }

    private void Update()
    {
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
                        walkingStatusUI.SetActive(false);
                }
            }
        }

        if (!CanBeginLocomotion() || xrOrigin == null || !movementEnabled)
        {
            moveAmount = Mathf.Lerp(moveAmount, 0f, Time.deltaTime * smoothing * 2f);

            if (uiText != null && !tutorialFinished)
                uiText.color = Color.Lerp(uiText.color, targetColor, Time.deltaTime * fadeSpeed);

            return;
        }

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

        if (isBobActive)
            moveAmount = Mathf.Lerp(moveAmount, stepSpeed, Time.deltaTime * smoothing);
        else
            moveAmount = Mathf.Lerp(moveAmount, 0f, Time.deltaTime * smoothing * 2f);

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
                    var wrapper = xrOrigin.GetComponent<CharacterControllerDriverWrapper>();
                    if (wrapper != null)
                    {
                        wrapper.ForceUpdateCharacterController();
                        wrapper.ApplyPersistentYOffset();
                    }

                    Vector3 motion = forward * moveAmount * Time.deltaTime;

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
                        SnapToGroundIfNeeded();
                }

                EndLocomotion();
            }
        }

        if (uiText != null && !tutorialFinished)
            uiText.color = Color.Lerp(uiText.color, targetColor, Time.deltaTime * fadeSpeed);
    }

    private void SnapToGroundIfNeeded()
    {
        if (xrOrigin == null || xrOrigin.Camera == null) return;

        CharacterController characterController = xrOrigin.GetComponent<CharacterController>();
        if (characterController == null) return;

        if (Physics.Raycast(xrOrigin.Camera.transform.position, Vector3.down, out RaycastHit hit, 2f, LayerMask.GetMask("Ground")))
        {
            float camHeight = xrOrigin.Camera.transform.position.y;
            float targetY = hit.point.y + characterController.height / 2f;
            float offset = camHeight - targetY;

            if (Mathf.Abs(offset) > 0.05f)
            {
                characterController.Move(Vector3.down * offset);
                Debug.Log("📏 Snapped to ground via SnapToGroundIfNeeded()");
            }
        }
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
        }r
        else
        {
            uiText.text = $"⏸️ Walking Disabled\n{tutorialStep}";
            targetColor = new Color(uiText.color.r, uiText.color.g, uiText.color.b, 1f);
        }
    }

private void LateUpdate()
{
    if (system != null && system.xrOrigin != null)
    {
        var wrapper = system.xrOrigin.GetComponent<CharacterControllerDriverWrapper>();
        if (wrapper != null)
        {
            wrapper.ApplyPersistentYOffset(force: true); // ✅ Force it every frame
        }
    }
}
*/
}