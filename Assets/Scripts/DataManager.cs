using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; // ✅ Needed for Scene Tracking

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private string googleSheetURL = "https://script.google.com/macros/s/AKfycbzJ3qQhNlCcSvjEBpyxJmotFGTF5bL92MW_htVo7oPAyswKX-WaCW0SJCJ9cQzgHeF0/exec"; 

    [Header("Player Settings")]
    [SerializeField] private int playerID; // ✅ Visible in Inspector
    private int currentPhase; // ✅ Track game phase (1 or 2)

    private Dictionary<string, string> placements = new Dictionary<string, string>(); // ✅ Stores placement data

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            StartCoroutine(InitializePlayerID()); // ✅ Initialize Player ID
        }
        else
        {
            Destroy(gameObject);
        }

        currentPhase = SceneManager.GetActiveScene().buildIndex + 1; // ✅ Detects phase (Scene 1 → Phase 1, Scene 2 → Phase 2)
    }

    // ✅ Store placements for both Phase 1 and Phase 2
    public void StorePlacement(string zoneID, string itemName)
{
    if (!placements.ContainsKey(zoneID))
    {
        placements.Add(zoneID, itemName);
    }
    else
    {
        placements[zoneID] = itemName;
    }

    int phase = SceneManager.GetActiveScene().buildIndex + 1; // ✅ Get phase dynamically
    SendDataOnline(zoneID, itemName); // ✅ Log placement to Google Sheets
}

    // ✅ Returns all placements (for Phase 2 recall comparison)
    public Dictionary<string, string> GetAllPlacements()
    {
        return new Dictionary<string, string>(placements);
    }

    // ✅ Retrieves stored placement for a specific zone
    public string GetPlacementForZone(string zoneID)
    {
        return placements.ContainsKey(zoneID) ? placements[zoneID] : null;
    }

    // ✅ Returns the current player ID stored in DataManager
    public int GetPlayerID()
    {
        return playerID;
    }

    // ✅ Initializes and syncs Player ID at game start
    private IEnumerator InitializePlayerID()
    {
        if (!PlayerPrefs.HasKey("PlayerID"))
        {
            PlayerPrefs.SetInt("PlayerID", 1); // ✅ Set default PlayerID to 1
            PlayerPrefs.Save();
        }

        playerID = PlayerPrefs.GetInt("PlayerID");

        // ✅ Fetch latest Player ID from Google Sheets and sync
        yield return StartCoroutine(SyncPlayerIDWithGoogleSheets());

        // ✅ Increment Player ID after syncing (if necessary)
        IncrementPlayerID();
    }

    // ✅ Increments Player ID only after syncing with Google Sheets
    public void IncrementPlayerID()
    {
        playerID++; // ✅ Increase Player ID
        PlayerPrefs.SetInt("PlayerID", playerID);
        PlayerPrefs.Save();
        Debug.Log($"✅ New Player ID Assigned: {playerID}");
    }

    // ✅ Sends placement data to Google Sheets
    public void SendDataOnline(string zoneID, string itemName)
    {
        if (string.IsNullOrEmpty(zoneID) || string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("❌ Invalid data! Skipping upload to Google Sheets.");
            return; // ✅ Prevents sending "Missing Player ID"
        }

        int phase = SceneManager.GetActiveScene().buildIndex + 1; // ✅ Get phase dynamically

        string condition = MovementConditionManager.Instance != null 
    ? MovementConditionManager.Instance.GetCondition().ToString()
    : "Unknown";

StartCoroutine(SendToGoogleSheets(condition, playerID, phase, zoneID, itemName));


    }

    // ✅ Coroutine to send data to Google Sheets
private IEnumerator SendToGoogleSheets(string condition, int playerID, int phase, string zoneID, string itemName)
{
    string fullURL = googleSheetURL + 
    "?condition=" + UnityWebRequest.EscapeURL(condition) +
    "&playerID=" + playerID +
    "&phase=" + phase +
    "&zoneID=" + UnityWebRequest.EscapeURL(zoneID) +
    "&itemName=" + UnityWebRequest.EscapeURL(itemName);


    UnityWebRequest request = UnityWebRequest.Get(fullURL);
    yield return request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        Debug.Log($"✅ Data Sent Successfully (Player ID: {playerID}, Phase: {phase})");
    }
    else
    {
        Debug.LogError("❌ Error sending data: " + request.error);
    }
}

    // ✅ Fetch latest Player ID from Google Sheets to keep IDs in sync
    private IEnumerator SyncPlayerIDWithGoogleSheets()
    {
        string fetchURL = googleSheetURL + "?fetchPlayerID=true"; // ✅ Request latest PlayerID
        UnityWebRequest request = UnityWebRequest.Get(fetchURL);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (int.TryParse(request.downloadHandler.text, out int latestID))
            {
                if (latestID >= playerID) // ✅ Ensure Player ID only increases
                {
                    playerID = latestID; // ✅ Sync Player ID
                    PlayerPrefs.SetInt("PlayerID", playerID);
                    PlayerPrefs.Save();
                    Debug.Log($"✅ Player ID synced from Google Sheets: {playerID}");
                }
            }
            else
            {
                Debug.LogError("❌ Failed to parse PlayerID from response.");
            }
        }
        else
        {
           Debug.LogError("❌ Error fetching PlayerID: " + request.error);
        }
    }
}