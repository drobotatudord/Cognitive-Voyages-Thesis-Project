using System.Collections.Generic;
using UnityEngine;

public class RecallManager : MonoBehaviour
{
    public static RecallManager Instance;

    private Dictionary<string, string> phase1Placements = new Dictionary<string, string>();
    private Dictionary<string, string> playerRecalls = new Dictionary<string, string>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        phase1Placements = DataManager.Instance.GetAllPlacements(); // ✅ Correct method call
    }

    public void RegisterRecall(string zoneID, string itemName)
    {
        if (!playerRecalls.ContainsKey(zoneID))
        {
            playerRecalls.Add(zoneID, itemName);
        }
        else
        {
            playerRecalls[zoneID] = itemName; // ✅ Update recall if already placed
        }
    }

    public void CheckRecallAccuracy()
    {
        int correctCount = 0;
        foreach (var zone in phase1Placements.Keys)
        {
            if (playerRecalls.ContainsKey(zone) && playerRecalls[zone] == phase1Placements[zone])
            {
                correctCount++;
            }
        }
        Debug.Log($"Player recalled {correctCount}/{phase1Placements.Count} correctly.");
    }
}