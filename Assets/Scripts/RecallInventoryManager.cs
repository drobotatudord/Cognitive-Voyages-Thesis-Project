using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecallInventoryManager : MonoBehaviour
{
    public static RecallInventoryManager Instance;

    private bool inventoryInitialized = false;

    public List<GameObject> itemPrefabs;
    public GameObject buttonPrefab;
    public Transform inventoryContainer;
    public Vector2 buttonSpacing = new Vector2(120, 120);

    public GameObject inventoryUItotalItemsPlaced; // ✅ NEW: Link this in the Inspector

    public Dictionary<string, Button> inventoryButtons = new Dictionary<string, Button>();

    private bool inventoryVisible = false;
    private HashSet<string> disabledButtons = new HashSet<string>();

   private void Awake()
{
    Instance = this;

    InitializeInventoryOnce();     // ✅ Shuffle only once
    GenerateInventory();           // ✅ Now uses shuffled list
    inventoryContainer.gameObject.SetActive(false);

    if (inventoryUItotalItemsPlaced != null)
    {
        inventoryUItotalItemsPlaced.SetActive(false);
    }
}


private void GenerateInventory()
{
    foreach (Transform child in inventoryContainer)
    {
        Destroy(child.gameObject);
    }

    inventoryButtons.Clear();

    for (int i = 0; i < itemPrefabs.Count; i++)
    {
        string itemName = itemPrefabs[i].name;

        GameObject buttonObj = Instantiate(buttonPrefab, inventoryContainer);
        Button button = buttonObj.GetComponent<Button>();
        inventoryButtons[itemName] = button;

        RectTransform rect = buttonObj.GetComponent<RectTransform>();

        // ✅ Use prefab’s scale — don’t override it
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        int row = i / 4;
        int col = i % 4;

        float spacingX = 1.0f;
        float spacingY = 1.0f;

        float startX = -1.5f; // centers 4 columns
        float startY = 2.8f;  // moves everything up by 1 row (based on spacingY = 1.0f)


        rect.localPosition = new Vector3(
            startX + col * spacingX,
            startY - row * spacingY,
            0f
        );

        TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = itemName;
        }

        button.onClick.AddListener(() => OnItemSelected(itemName));

        if (disabledButtons.Contains(itemName))
        {
            button.interactable = false;
        }
    }
}


public void InitializeInventoryOnce()
{
    if (!inventoryInitialized)
    {
        ShuffleList(itemPrefabs); // ✅ Randomize item order once
        inventoryInitialized = true;
    }
}


    public void OnItemSelected(string itemName)
    {
        RecallPlacementZone currentZone = RecallPlacementManager.Instance.GetCurrentPlacementZone();
        if (currentZone != null)
        {
            currentZone.PlaceItem(itemName);
            DisableItemButton(itemName);
        }
    }

    public void DisableItemButton(string itemName)
    {
        if (inventoryButtons.ContainsKey(itemName))
        {
            inventoryButtons[itemName].interactable = false;
            disabledButtons.Add(itemName);
        }
    }

    public void ToggleInventory(bool show)
    {
        if (inventoryVisible == show) return;

        Debug.Log(show ? "✅ Inventory Opened" : "⛔ Inventory Closed");
        inventoryContainer.gameObject.SetActive(show);

        if (inventoryUItotalItemsPlaced != null)
        {
            inventoryUItotalItemsPlaced.SetActive(show); // ✅ Show/hide total items placed
        }

        inventoryVisible = show;
    }

    private void ShuffleList<T>(List<T> list)
{
    for (int i = list.Count - 1; i > 0; i--)
    {
        int randomIndex = Random.Range(0, i + 1);
        (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
    }
}


    public GameObject GetItemPrefab(string itemName)
    {
        return itemPrefabs.Find(prefab => prefab.name == itemName);
    }
}