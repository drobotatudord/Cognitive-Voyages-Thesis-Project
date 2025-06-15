using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // ✅ Needed for Scene Tracking

public class RecallPlacementZone : MonoBehaviour
{
    public string zoneID;
    public Transform snapPoint;
    public TMP_Text itemNameDisplay;

    public GameObject highlightEffect;    // Visual feedback for placement
    public float blinkSpeed = 1.0f;        // Speed of the blinking effect

    private bool isOccupied = false;
    private string placedItemName;
    private bool isBlinking = false;

    private bool shouldHideNameWhileInside = false;

    private void Awake()
    {
        if (string.IsNullOrEmpty(zoneID))
        {
            zoneID = gameObject.name;
        }
        UpdateDisplayText();
        UpdateHighlight();
    }

    private void Update()
{
    // ✅ Make sure the text always faces the player
    if (itemNameDisplay != null && Camera.main != null)
    {
        itemNameDisplay.transform.LookAt(Camera.main.transform);
        itemNameDisplay.transform.Rotate(0, 180, 0); // ✅ Flip text for proper readability
    }

    if (isBlinking && highlightEffect != null)
        {
            float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1.0f);
            Color color = highlightEffect.GetComponent<Renderer>().material.color;
            color.a = alpha;
            highlightEffect.GetComponent<Renderer>().material.color = color;
        }
}


private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        Debug.Log($"✅ Player entered {zoneID}, enabling inventory.");
        RecallPlacementManager.Instance.SetCurrentPlacementZone(this);

        if (!isOccupied)
        {
            shouldHideNameWhileInside = true;
            UpdateDisplayText(); // 👈 Refresh label to hide it
            RecallInventoryManager.Instance.ToggleInventory(true);
        }
    }
}

    private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player"))
    {
        Debug.Log($"⛔ Player exited {zoneID}, disabling inventory.");
        RecallPlacementManager.Instance.ClearCurrentPlacementZone();
        RecallInventoryManager.Instance.ToggleInventory(false);

        shouldHideNameWhileInside = false;
        UpdateDisplayText(); // 👈 Show label again if needed
    }
}


   public void PlaceItem(string itemName)
{
    if (isOccupied)
    {
        Debug.Log($"🚫 Zone {zoneID} is already occupied.");
        RecallInventoryManager.Instance.ToggleInventory(false); // ✅ Just close inventory if occupied
        return;
    }

    // ✅ Only mark zone as occupied AFTER placement is registered
    if (!RecallPlacementManager.Instance.RegisterPlacement(zoneID, itemName))
    {
        Debug.LogWarning($"❌ Placement registration failed for {zoneID}");
        return;
    }

    placedItemName = itemName;
    isOccupied = true;

    GameObject itemPrefab = RecallInventoryManager.Instance.GetItemPrefab(itemName);
    if (itemPrefab != null)
    {
        GameObject placedObject = Instantiate(itemPrefab, snapPoint.position + Vector3.up * 0.18f, snapPoint.rotation);
        ItemController itemController = placedObject.GetComponent<ItemController>();

        if (itemController != null)
        {
            itemController.MoveToSnap(snapPoint);
        }
    }

    UpdateDisplayText();
    UpdateHighlight();
    RecallInventoryManager.Instance.DisableItemButton(itemName);
    RecallInventoryManager.Instance.ToggleInventory(false);
}


    public void ForcePlayerEnter()
{
    Debug.Log($"🔁 [Teleport] Entered {zoneID}");
    RecallPlacementManager.Instance.SetCurrentPlacementZone(this);

    if (!isOccupied)
    {
        shouldHideNameWhileInside = true;
        UpdateDisplayText();
        RecallInventoryManager.Instance.ToggleInventory(true);
    }
}

public void ForcePlayerExit()
{
    Debug.Log($"🔁 [Teleport] Exited {zoneID}");
    RecallPlacementManager.Instance.ClearCurrentPlacementZone();
    RecallInventoryManager.Instance.ToggleInventory(false);

    shouldHideNameWhileInside = false;
    UpdateDisplayText();
}


    private void UpdateHighlight()
{
    if (highlightEffect != null)
    {
        bool shouldHighlight = !isOccupied; // ✅ Turns OFF when an item is placed
        highlightEffect.SetActive(shouldHighlight);
    }
}


    private void UpdateDisplayText()
{
    if (itemNameDisplay != null)
    {
        if (shouldHideNameWhileInside && !isOccupied)
        {
            itemNameDisplay.text = " ";
        }
        else
        {
            itemNameDisplay.text = isOccupied ? placedItemName : "Place Item";
        }
    }
}

public bool IsOccupied()
{
    return isOccupied;
}

}