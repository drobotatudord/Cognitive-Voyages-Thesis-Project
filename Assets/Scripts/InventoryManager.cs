using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private bool inventoryInitialized = false;
    public List<GameObject> inventoryItems;
    public List<Sprite> itemSprites; // ✅ List of sprites matching items
    public Transform rightHandTransform;
    public InputActionProperty placeAction;
    public InputActionProperty resetAction;

    public GameObject buttonPrefab;
    public Transform inventoryContainer;
    public GameObject inventoryUI;
     public GameObject inventoryUItotalItemsPlaced;


    public Vector2 buttonSpacing = new Vector2(120, 120);

    [HideInInspector] 
    public GameObject currentItem;  // ✅ Now accessible by ButtonItemHandler
    private int currentItemIndex = -1;
    private HashSet<string> usedItems = new HashSet<string>();
    private List<Button> inventoryButtons = new List<Button>();

    private void Awake()
    {
        Instance = this;
        GenerateRandomizedInventory();
        ToggleInventory(false);
    }

    private void Update()
    {
        if (placeAction.action.WasPressedThisFrame() && currentItem != null)
{
    var itemController = currentItem?.GetComponent<ItemController>();
    if (itemController == null)
    {
        Debug.LogWarning("❌ ItemController not found on currentItem.");
        return;
    }

    itemController.PlaceItem();
    if (itemController.IsPlaced()) // ✅ You'll need to add this method in ItemController
    {
       if (currentItem != null) // ✅ extra safeguard
    {
        string placedItemName = currentItem.name.Replace("(Clone)", "");
        usedItems.Add(placedItemName);
    }
        currentItem = null;
        EnableAllButtons();
    }
    else
    {
        // Placement failed, don't reset state yet
        Debug.LogWarning("⚠️ Placement failed — item still in hand.");
    }
}

        // ✅ Modified Reset Logic
        if (resetAction.action.WasPressedThisFrame())
        {
            PlacementZone[] zones = FindObjectsOfType<PlacementZone>();

            foreach (var zone in zones)
            {
                // ✅ Only reset the item if the player is in this specific zone
                if (zone.IsPlayerInZone() && zone.IsOccupied())
                {
                    zone.ResetZone();
                    break; // ✅ Exit the loop after resetting the item in the current zone
                }
            }
        }
    }

    public void ToggleInventory(bool isVisible)
    {
        inventoryUI.SetActive(isVisible);
        inventoryUItotalItemsPlaced.SetActive(isVisible);

        if (isVisible)
        {
            RefreshButtonStates(); // ✅ Ensure correct button states when inventory reopens
        }
    }

    public void InitializeInventoryOnce()
{
    if (!inventoryInitialized)
    {
        GenerateRandomizedInventory();
        inventoryInitialized = true;
    }
}

public void ResetInventoryInitialization()
{
    inventoryInitialized = false;
}

    private void RefreshButtonStates()
{
    for (int i = 0; i < inventoryItems.Count; i++)
    {
        string itemName = inventoryItems[i].name.Replace("(Clone)", "");
        Button button = inventoryButtons[i];
        bool isUsed = usedItems.Contains(itemName);

        button.interactable = !isUsed;

        // 🔷 Visually dim the button image
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            img.color = isUsed ? new Color(0.4f, 0.4f, 0.4f, 0.8f) : Color.white;
        }

        // 🔷 Dim the button text as well
        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = isUsed ? new Color(0.6f, 0.6f, 0.6f, 0.7f) : Color.white;
        }
    }
}


    public void GenerateRandomizedInventory()
    {
        // ✅ Clear previous inventory UI
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }
        inventoryButtons.Clear();

        // ✅ Shuffle items and sprites together to keep them aligned
        List<(GameObject, Sprite)> shuffledInventory = new List<(GameObject, Sprite)>();
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            shuffledInventory.Add((inventoryItems[i], itemSprites[i]));
        }

        ShuffleList(shuffledInventory); // ✅ Shuffle items + sprites as pairs

        // ✅ Create new randomized buttons
        for (int i = 0; i < shuffledInventory.Count; i++)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, inventoryContainer);
            Button button = buttonObj.GetComponent<Button>();
            inventoryButtons.Add(button);

int row = i / 4;
int col = i % 4;

float startX = -((3 * buttonSpacing.x) / 2); // centers 4 columns
float startY = ((2 * buttonSpacing.y));      // centers 3 rows (assumes 12 items max)

buttonObj.transform.localPosition = new Vector3(
    startX + col * buttonSpacing.x,
    startY - row * buttonSpacing.y,
    0f
);

            buttonObj.name = shuffledInventory[i].Item1.name;

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = shuffledInventory[i].Item1.name;
            }

            // ✅ Assign correct item & sprite after shuffle
            ButtonItemHandler buttonHandler = buttonObj.AddComponent<ButtonItemHandler>();
            buttonHandler.Initialize(this, i, shuffledInventory[i].Item2); // ✅ Use shuffled index

            // ✅ Ensure placed items remain disabled
           if (usedItems.Contains(shuffledInventory[i].Item1.name.Replace("(Clone)", "")))
{
    button.interactable = false;

    // 🔷 Visually dim the button image and text
    Image img = button.GetComponent<Image>();
    if (img != null)
        img.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);

    TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
    if (text != null)
        text.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
}

        }

        // ✅ Update inventoryItems to match shuffled order
        inventoryItems = shuffledInventory.ConvertAll(pair => pair.Item1);
        itemSprites = shuffledInventory.ConvertAll(pair => pair.Item2);

        Debug.Log("✅ Inventory randomized and updated.");
    }

    // ✅ Generic shuffle function
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    public void HandleFailedPlacement(GameObject item)
{
    if (item != null)
    Destroy(item);
    transform.SetParent(null);
    currentItem = null;
    EnableAllButtons();
}


    public void SpawnItem(int index)
    {
        string itemName = inventoryItems[index].name;

        if (usedItems.Contains(itemName))
        {
            Debug.Log($"{itemName} is already placed. You cannot spawn it again.");
            return;
        }

        if (currentItem != null && currentItemIndex != index)
        {
            inventoryButtons[currentItemIndex].interactable = true;
        }

        if (currentItem != null && currentItem.name == itemName + "(Clone)")
        {
            return;
        }

        if (currentItem != null)
        {
            Destroy(currentItem);
            currentItem = null;
            currentItemIndex = -1;
        }

        currentItem = Instantiate(inventoryItems[index], rightHandTransform);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        currentItem.transform.localScale = Vector3.one;



        XRGrabInteractable grabInteractable = currentItem.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = currentItem.AddComponent<XRGrabInteractable>();
        }

        Rigidbody rb = currentItem.GetComponent<Rigidbody>();
        if (rb == null) rb = currentItem.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        grabInteractable.interactionManager = FindObjectOfType<XRInteractionManager>();
        grabInteractable.selectEntered.AddListener((args) =>
        {
            InventoryManager.Instance.currentItem = currentItem;
        });

        grabInteractable.selectExited.AddListener((args) =>
        {
            InventoryManager.Instance.currentItem = null;
        });

        inventoryButtons[index].interactable = false;
        currentItemIndex = index;
    }

public void EnableAllButtons()
{
    for (int i = 0; i < inventoryButtons.Count; i++)
    {
        string itemName = inventoryItems[i].name.Replace("(Clone)", "");
        inventoryButtons[i].interactable = !usedItems.Contains(itemName);
    }
    currentItemIndex = -1;
}


    public void DisableButton(string itemName)
    {
        int index = inventoryItems.FindIndex(item => item.name == itemName);
        if (index != -1 && index < inventoryButtons.Count)
        {
            inventoryButtons[index].interactable = false;
            usedItems.Add(itemName);
        }
    }

    public void EnableButton(string itemName)
    {
        int index = inventoryItems.FindIndex(item => item.name == itemName);
        if (index != -1 && index < inventoryButtons.Count)
        {
            inventoryButtons[index].interactable = true;
            usedItems.Remove(itemName);
        }
    }

    public void MarkItemAsUsed(string itemName)
    {
        if (!usedItems.Contains(itemName))
        {
            usedItems.Add(itemName);
        }
        RefreshButtonStates();
    }

    public void MarkItemAsAvailable(string itemName)
    {
        usedItems.Remove(itemName);
        RefreshButtonStates();
    }

    public void TriggerHapticFeedback(float amplitude, float duration)
    {
        XRBaseController rightController = rightHandTransform.GetComponentInParent<XRBaseController>();
        if (rightController != null)
        {
            rightController.SendHapticImpulse(amplitude, duration);
        }
    }
}