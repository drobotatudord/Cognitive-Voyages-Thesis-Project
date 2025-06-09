using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonItemHandler : MonoBehaviour
{
    private InventoryManager inventoryManager;
    public int itemIndex;
    private GameObject currentItem;
    private Image buttonImage; // ✅ Reference to the button's image

    public void Initialize(InventoryManager manager, int index, Sprite itemSprite)
    {
        inventoryManager = manager;
        itemIndex = index;

        Button button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SpawnItem);

        buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = itemSprite; // ✅ Set the button's sprite
        }
    }

    public void SpawnItem()
{
    if (inventoryManager != null && itemIndex >= 0 && itemIndex < inventoryManager.inventoryItems.Count)
    {
        if (inventoryManager.currentItem != null)
        {
            Destroy(inventoryManager.currentItem);
        }

        // ✅ Use the shuffled inventory list
        GameObject itemPrefab = inventoryManager.inventoryItems[itemIndex]; 
        if (itemPrefab != null)
        {
            inventoryManager.currentItem = Instantiate(itemPrefab, inventoryManager.rightHandTransform.position, inventoryManager.rightHandTransform.rotation);
            inventoryManager.currentItem.transform.SetParent(inventoryManager.rightHandTransform);

            InventoryManager.Instance.TriggerHapticFeedback(0.8f, 0.3f);

            Debug.Log($"✅ Spawned item: {itemPrefab.name} at index {itemIndex}");
        }
        else
        {
            Debug.LogWarning("❌ Item prefab is null.");
        }
    }
    else
    {
        Debug.LogWarning("❌ InventoryManager is not assigned, or itemIndex is invalid.");
    }
}

    public void PlaceItemOnPedestal(PlacementZone placementZone)
    {
        if (currentItem != null && placementZone != null && !placementZone.IsOccupied())
        {
            placementZone.TryPlaceItem(currentItem.GetComponent<ItemController>());
            currentItem = null;

           InventoryManager.Instance.TriggerHapticFeedback(0.8f, 0.3f);
        }
    }

    public int GetItemIndex()
    {
        return itemIndex;
    }
}