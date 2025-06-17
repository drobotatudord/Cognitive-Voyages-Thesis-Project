using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class HeadBobMovement : LocomotionProvider

{
    [Header("References")]
    public Transform xrCamera; // Usually the Main Camera
    public XROrigin xrOrigin;  // Assign XR Origin in the inspector

    [Header("Movement Settings")]
    public float spacingOffset = 2f;
    public float minPeak = 0.05f;
    public float maxPeak = 0.4f;
    public float accelerationMultiplier = 1f;
    public float decelerationMultiplier = 1f;
    public float multiplier = 1f;
    public float maxSpeed = 2.5f;
    public float timeTillMaxSpeed = 0.3f;

    private float camInitialYPos;
    private Vector3 camRot;
    private float speedTimer;
    private float targetSpeed;
    private float moveSpeed;
    private Vector3 lastCameraPosition;

    private readonly int queueSize = 5;
    private readonly int filterSize = 4;
    private Queue<float> filteredData = new Queue<float>();
    private readonly List<float> tempInputList = new();

    private CharacterController characterController;
    private Vector3 smoothedDirection = Vector3.zero;

    private float baseCameraLocalY;

    private float lastSyncedHeight = -1f;

    private bool allowMovement = false;


void Start()
{
    StartCoroutine(InitializeWhenReady());
}

private IEnumerator InitializeWhenReady()
{
    // ✅ Safeguard against missing references
    if (xrCamera == null || xrOrigin == null)
    {
        Debug.LogError("❌ Missing XR references. Please assign xrCamera and xrOrigin in the inspector.");
        yield break;
    }

    yield return new WaitForSeconds(0.5f);

    while (xrCamera.localPosition.y < 0.2f)
    {
        yield return null;
    }

    baseCameraLocalY = xrCamera.localPosition.y;
    lastCameraPosition = xrCamera.localPosition;

    // Assign CharacterController
    characterController = xrOrigin.GetComponent<CharacterController>();
    if (characterController == null)
    {
        Debug.LogError("❌ CharacterController not found on XR Origin. Please add one.");
        yield break;
    }

    // Set consistent height offset
    if (xrOrigin.CameraFloorOffsetObject != null)
    {
        Vector3 offset = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
        offset.y = -0.90f;
        xrOrigin.CameraFloorOffsetObject.transform.localPosition = offset;
        Debug.Log("📏 XR Origin Y offset set to -0.90f for consistency (delayed)");
    }

    allowMovement = true;
}

    void Update()
    {
        SetMoveDirection();
        GetHeadFrameInput();


        float camHeight = xrCamera.localPosition.y;
    if (Mathf.Abs(camHeight - lastSyncedHeight) > 0.1f)
    {
        SyncColliderToCamera();
        lastSyncedHeight = camHeight;
    }

        if (tempInputList.Count >= filterSize)
        {
            float inputSum = 0;
            foreach (var val in tempInputList)
                inputSum += val;

            AddSetToList(inputSum);
            tempInputList.Clear();
        }

        speedTimer = Mathf.Clamp(speedTimer, 0, timeTillMaxSpeed);
        SetMoveSpeed();
    }

void FixedUpdate()
{
    if (!allowMovement || !BeginLocomotion()) return;

    if (moveSpeed > 0.01f && characterController != null)
    {
        Vector3 desiredDirection = xrCamera.forward;
        desiredDirection.y = Mathf.Lerp(desiredDirection.y, 0f, 0.9f);
        desiredDirection.Normalize();

        smoothedDirection = Vector3.Lerp(smoothedDirection, desiredDirection, Time.fixedDeltaTime * 5f);
        characterController.Move(smoothedDirection * moveSpeed * Time.fixedDeltaTime);

        if (!characterController.isGrounded)
            characterController.Move(Vector3.down * 0.05f);
    }

    EndLocomotion();
}

void SyncColliderToCamera()
{
    if (characterController == null)
    {
        Debug.LogWarning("⚠️ characterController is null in SyncColliderToCamera.");
        return;
    }

    float cameraHeight = xrCamera.localPosition.y;
    float newHeight = Mathf.Clamp(cameraHeight, 1f, 2.5f);
    characterController.height = newHeight;
    characterController.center = new Vector3(0, newHeight / 2f, 0);
}


    void GetHeadFrameInput()
    {
        Vector2 range = ReturnBobbingRange();
        float camY = xrCamera.localPosition.y;

        if (camY <= range.x && camY >= range.y)
        {
            tempInputList.Add(camY);
        }
    }

    void AddSetToList(float filteredValue)
    {
        filteredData.Enqueue(filteredValue);

        if (filteredData.Count >= queueSize)
        {
            float[] data = filteredData.ToArray();
            float high = Mathf.Max(data);
            float low = Mathf.Min(data);
            float delta = high - low;

            if (delta >= minPeak && delta <= maxPeak)
            {
                speedTimer += Time.deltaTime * accelerationMultiplier;
                targetSpeed = Mathf.Clamp(delta * multiplier, 0, maxSpeed);
            }
            else
            {
                speedTimer -= Time.deltaTime * decelerationMultiplier;
            }

            filteredData.Dequeue();
        }
    }

    void SetMoveSpeed()
    {
        moveSpeed = targetSpeed * (speedTimer / timeTillMaxSpeed);
    }

Vector2 ReturnBobbingRange()
{
    float offset = CalculateDegreeAdjustmentValue();
    return new Vector2(
        baseCameraLocalY + offset + spacingOffset,
        baseCameraLocalY + offset - spacingOffset
    );
}


    float CalculateDegreeAdjustmentValue()
    {
        float xAngle = xrCamera.localRotation.eulerAngles.x;
        xAngle = (xAngle > 180) ? xAngle - 360 : xAngle;

        float value = 0f;
        if (xAngle >= 0 && xAngle < 90)
            value = 0.06f * Mathf.Sin(xAngle * Mathf.Deg2Rad);
        else if (xAngle > -90 && xAngle < 0)
            value = 0.13f * Mathf.Sin(xAngle * Mathf.Deg2Rad);

        return value;
    }

    Vector3 GetMoveDirection()
    {
        Vector3 forward = xrCamera.forward;
        forward.y = 0;
        return forward.normalized;
    }

    void SetMoveDirection()
    {
        camRot = xrCamera.localRotation.eulerAngles;
        if (camRot.x > 180)
            camRot.x -= 360;
        camRot.x = -camRot.x;
    }

void LateUpdate()
{
    if (xrOrigin != null && xrOrigin.CameraFloorOffsetObject != null)
    {
        Vector3 offset = xrOrigin.CameraFloorOffsetObject.transform.localPosition;
        if (Mathf.Abs(offset.y + 0.90f) > 0.001f)
        {
            offset.y = -0.90f;
            xrOrigin.CameraFloorOffsetObject.transform.localPosition = offset;
            Debug.Log("🔁 Reapplied Y offset in LateUpdate");
        }
    }
}
}