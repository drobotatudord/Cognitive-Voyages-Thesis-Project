using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // ✅ Needed for Scene Tracking

public class PlacementZone : MonoBehaviour
{
    [Header("Placement Zone Settings")]

    private PlacementManager placementManager;

    private bool shouldHideNameWhileInside = false;


    public string zoneID;
    public string placedItemName; // ✅ Display item name in Inspector
    public ItemController placedItem;    // Reference to the placed item

    public Transform snapPoint;
    public GameObject highlightEffect;    // Visual feedback for placement
    public float blinkSpeed = 1.0f;        // Speed of the blinking effect

    private bool isBlinking = false;
    private bool playerInZone = false;

    public TMP_Text itemNameDisplay; // ✅ Reference to the TextMeshPro object

    [Header("Meshes to Toggle")]
public GameObject[] meshesToToggle;


    private void Awake()
    {
        placementManager = FindObjectOfType<PlacementManager>();
        placementManager.placementZones.Add(this); // Automatically register the zone

        if (string.IsNullOrEmpty(zoneID))
        {
            zoneID = gameObject.name;
        }

        if (itemNameDisplay == null) // ✅ Auto-assign if not linked manually
        {
            itemNameDisplay = GetComponentInChildren<TMP_Text>();
        }

        UpdateItemNameDisplay(); // ✅ Ensure display is clear on start
        UpdateHighlight();
    }

    private void Update()
    {

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
    if (other.CompareTag("Player") && !playerInZone)
    {
        playerInZone = true;
        shouldHideNameWhileInside = true;
        UpdateInventoryVisibility();
        UpdateItemNameDisplay(); // 👈 Refresh label
    }
}

private void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player") && playerInZone)
    {
        playerInZone = false;
        shouldHideNameWhileInside = false;
        UpdateInventoryVisibility();
        UpdateItemNameDisplay(); // 👈 Restore default label

        if (InventoryManager.Instance.currentItem != null)
        {
            Destroy(InventoryManager.Instance.currentItem);
            InventoryManager.Instance.currentItem = null;
            InventoryManager.Instance.EnableAllButtons();
        }
    }
}


    public bool IsOccupied()
    {
        return placedItem != null;
    }

    public bool TryPlaceItem(ItemController item)
    {
        if (IsOccupied())
        {
            Debug.Log($"PlacementZone {zoneID} is already occupied!");
            return false;
        }

        SetPlacedItem(item);
        SnapItemToZone(item);
        UpdateInventoryVisibility();
        return true;
    }

    private void ToggleMeshes(bool active)
{
    foreach (GameObject mesh in meshesToToggle)
    {
        if (mesh != null)
        {
            mesh.SetActive(active);
        }
    }
}


    public void SetPlacedItem(ItemController item)
    {
        placedItem = item;
        placedItemName = item.name.Replace("(Clone)", ""); // ✅ Clean name

        // ✅ Log placement to DataManager
        // ✅ Make sure to send the Player ID and phase when sending data
        int playerID = DataManager.Instance.GetPlayerID();
        int phase = SceneManager.GetActiveScene().buildIndex + 1; // ✅ Get current phase dynamically
        DataManager.Instance.SendDataOnline(zoneID, placedItemName);


        // ✅ Disable the corresponding button
        InventoryManager.Instance.DisableButton(placedItemName);
        InventoryManager.Instance.MarkItemAsUsed(placedItemName);

            if (phase == 1)
    {
        PlacementManager.Instance.RegisterPlacement(zoneID, item);
    }

        UpdateHighlight();
        ToggleMeshes(false); // Turn off meshes when item is placed
        UpdateItemNameDisplay();
        UpdateInventoryVisibility();
    }

    public void RemoveItem()
    {

        string itemName = placedItem.name.Replace("(Clone)", "");

        PlacementManager.Instance.DeregisterPlacement(zoneID);

        if (placedItem != null)
        {

            // ✅ Re-enable the button when item is reset
            InventoryManager.Instance.EnableButton(itemName);
            InventoryManager.Instance.MarkItemAsAvailable(itemName);

            Destroy(placedItem.gameObject);
            placedItem = null;

            placedItemName = "";
            UpdateItemNameDisplay();


            UpdateInventoryVisibility();
            UpdateHighlight();
            ToggleMeshes(true); // Turn meshes back on when item is removed
        }
    }

private void UpdateItemNameDisplay()
{
    if (itemNameDisplay != null)
    {
        if (shouldHideNameWhileInside && !IsOccupied())
        {
            itemNameDisplay.text = " "; // 👈 Empty while inside and nothing placed
        }
        else
        {
            itemNameDisplay.text = string.IsNullOrEmpty(placedItemName)
                ? "Place item" // 👈 Default if outside and nothing placed
                : placedItemName.Replace("(Clone)", "");
        }
    }
}



private void SnapItemToZone(ItemController item)
{
    if (snapPoint != null)
    {
        item.MoveToSnap(snapPoint); // ✅ Calls animation method
        item.transform.rotation = snapPoint.rotation; // ✅ Keeps correct rotation
    }
    else
    {
        item.transform.position = transform.position;
        item.transform.rotation = Quaternion.identity;
    }

    Rigidbody rb = item.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = true; // ✅ Ensure item stays in place
    }
}

    public ItemController GetPlacedItem()
    {
        return placedItem;
    }

    private void UpdateHighlight()
    {
        if (highlightEffect != null)
        {
            bool shouldHighlight = placedItem == null;
            highlightEffect.SetActive(shouldHighlight);
            isBlinking = shouldHighlight;
        }
    }

    public void ResetZone()
    {
        RemoveItem();
        InventoryManager.Instance.TriggerHapticFeedback(0.8f, 0.3f);
    }

private void UpdateInventoryVisibility()
{
    if (playerInZone && !IsOccupied())
    {
        InventoryManager.Instance.InitializeInventoryOnce(); // ✅ Only generates inventory once
        //InventoryManager.Instance.GenerateRandomizedInventory();

        InventoryManager.Instance.ToggleInventory(true);
    }
    else
    {
        InventoryManager.Instance.ToggleInventory(false);
    }
}

public void ForcePlayerEnter()
{
    if (!playerInZone)
    {
        playerInZone = true;
        shouldHideNameWhileInside = true;
        UpdateInventoryVisibility();
        UpdateItemNameDisplay();
    }
}

public void ForcePlayerExit()
{
    if (playerInZone)
    {
        playerInZone = false;
        shouldHideNameWhileInside = false;
        UpdateInventoryVisibility();
        UpdateItemNameDisplay();

        if (InventoryManager.Instance.currentItem != null)
        {
            Destroy(InventoryManager.Instance.currentItem);
            InventoryManager.Instance.currentItem = null;
            InventoryManager.Instance.EnableAllButtons();
        }
    }
}


    public bool IsPlayerInZone()
    {
        return playerInZone;
    }

}