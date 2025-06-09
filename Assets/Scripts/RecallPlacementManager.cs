using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RecallPlacementManager : MonoBehaviour
{
    public static RecallPlacementManager Instance;

    private Dictionary<string, string> phase1Placements = new Dictionary<string, string>();
    private Dictionary<string, string> phase2Placements = new Dictionary<string, string>();

    private RecallPlacementZone currentPlacementZone; // ✅ Track the active placement zone

    public TMP_Text itemCounterText; // ✅ UI text to display item count

     private int placedItemCount = 0; // ✅ Track number of placed items

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (DataManager.Instance != null) // ✅ Check if DataManager exists
        {
            phase1Placements = DataManager.Instance.GetAllPlacements();
        }
        else
        {
            Debug.LogError("❌ DataManager Instance is null! Ensure DataManager is in the scene.");
        }
    }

    /// ✅ Set the current placement zone (Called when player enters a zone)
    public void SetCurrentPlacementZone(RecallPlacementZone zone)
    {
        currentPlacementZone = zone;
    }

    /// ✅ Get the currently active placement zone
    public RecallPlacementZone GetCurrentPlacementZone()
    {
        return currentPlacementZone;
    }

    /// ✅ Clears the placement zone when the player leaves
    public void ClearCurrentPlacementZone()
    {
        currentPlacementZone = null;
    }

    /// ✅ Registers the player's placement in Phase 2
    public bool RegisterPlacement(string zoneID, string itemName)
    {
        if (phase2Placements.ContainsKey(zoneID))
        {
            Debug.Log($"🚫 {zoneID} is already occupied!");
            return false; // ✅ Prevent reusing same slot
        }
        placedItemCount++; // ✅ Increment item count
        UpdateItemCounter(); // ✅ Update UI
        phase2Placements[zoneID] = itemName;
        DataManager.Instance.StorePlacement(zoneID, itemName); // ✅ Log placement in Phase 2

        // ✅ If all items placed, check accuracy
       // if (phase2Placements.Count == phase1Placements.Count)
        //{
         //   CheckAccuracy();
       // }

        return true; // ✅ Placement successful
    }

    /// ✅ Compares Phase 2 placements with Phase 1 for accuracy

     private void UpdateItemCounter()
    {
        if (itemCounterText != null)
        {
            itemCounterText.text = $"{placedItemCount} / 12";
        }
    }
}