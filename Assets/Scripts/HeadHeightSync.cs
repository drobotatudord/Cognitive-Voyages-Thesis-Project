using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils; // <-- This line is required for XROrigin

[RequireComponent(typeof(XROrigin))]
[RequireComponent(typeof(CharacterController))]
public class HeadHeightSync : MonoBehaviour
{
    public float minHeight = 1.0f;
    public float maxHeight = 2.0f;

    private XROrigin xrOrigin;
    private CharacterController characterController;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        characterController = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        UpdateCharacterHeight();
    }

    void UpdateCharacterHeight()
    {
        Transform head = xrOrigin.Camera.transform;
        float headLocalHeight = Mathf.Clamp(head.localPosition.y, minHeight, maxHeight);

        characterController.height = headLocalHeight;
        characterController.center = new Vector3(head.localPosition.x, headLocalHeight / 2f, head.localPosition.z);
    }
}