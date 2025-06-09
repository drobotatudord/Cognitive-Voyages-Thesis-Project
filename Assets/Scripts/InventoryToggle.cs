using UnityEngine;
using UnityEngine.InputSystem;


public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryUI;
    public InputActionProperty toggleAction;

    private void Update()
    {
        if (toggleAction.action.WasPressedThisFrame())
        {
            bool isActive = !inventoryUI.activeSelf;
            inventoryUI.SetActive(isActive);
        }
    }
}