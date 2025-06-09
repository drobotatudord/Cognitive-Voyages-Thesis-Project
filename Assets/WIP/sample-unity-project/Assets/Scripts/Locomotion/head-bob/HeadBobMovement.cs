// Adapted head bob system using logic from Immersification into your XR Origin-based setup
// This integrates smoothly into your existing WalkInPlace-style architecture and supports stairs

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using System.Collections.Generic;

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

    void Start()
    {
        if (xrOrigin == null)
        {
            Debug.LogError("XR Origin is not assigned in HeadBobMovement.");
            return;
        }

        characterController = xrOrigin.Origin.GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController not found on XR Origin's Origin GameObject.");
            return;
        }

        camInitialYPos = xrCamera.localPosition.y;
        lastCameraPosition = xrCamera.localPosition;
    }

    void Update()
    {
        SetMoveDirection();
        GetHeadFrameInput();

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
        if (BeginLocomotion())
        {
            if (moveSpeed > 0.01f && characterController != null)
            {
                Vector3 desiredDirection = GetMoveDirection();
                smoothedDirection = Vector3.Lerp(smoothedDirection, desiredDirection, Time.fixedDeltaTime * 5f);
                characterController.Move(smoothedDirection * moveSpeed * Time.fixedDeltaTime);
            }
            EndLocomotion();
        }
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
            camInitialYPos + offset + spacingOffset,
            camInitialYPos + offset - spacingOffset
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
}