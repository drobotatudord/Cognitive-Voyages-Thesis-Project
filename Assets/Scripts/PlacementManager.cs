using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // ✅ Import TextMeshPro for UI updates
using UnityEngine.SceneManagement; // ✅ Include Scene Management

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [Header("Fade UI")]
    public CanvasGroup fadeCanvasGroup;
    public Image fadePanelImage; // ✅ Panel with background image

    public TMP_Text instructionText;

    private Dictionary<string, string> placements = new Dictionary<string, string>(); // ✅ Track by name

    public List<string> activePlacements = new List<string>(); // ✅ Inspector-friendly tracking

    public List<PlacementZone> placementZones = new List<PlacementZone>();

      [Header("UI Settings")]
    public TMP_Text itemCounterText; // ✅ UI text to display item count

    private int placedItemCount = 0; // ✅ Track number of placed items

    public GameObject objectToDisable1; // Assign in inspector
public GameObject objectToDisable2;
public GameObject objectToDisable3;
public MonoBehaviour scriptToDisable;
public MonoBehaviour scriptToDisable2;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterPlacement(string zoneID, ItemController item)
    {
        foreach (var zone in placementZones)
        {
            if (zone.zoneID == zoneID)
            {
                zone.placedItemName = item.name;
                UpdateActivePlacements();
                placedItemCount++; // ✅ Increment item count
                UpdateItemCounter(); // ✅ Update UI
                CheckAllZonesFilled(); // ✅ Check if all zones are now filled
                break;
            }
        }
    }

public void DeregisterPlacement(string zoneID)
{
    foreach (var zone in placementZones)
    {
        if (zone.zoneID == zoneID)
        {
            zone.placedItemName = "No item available";
            UpdateActivePlacements();

            if (placedItemCount > 0) // ✅ Ensure we don't decrease below 0
            {
                placedItemCount--; // ✅ Decrement item count correctly
                UpdateItemCounter(); // ✅ Update UI
            }
            break;
        }
    }
}

    public void UpdateActivePlacements()
    {
        activePlacements.Clear();
        foreach (var zone in placementZones)
        {
            string itemName = string.IsNullOrEmpty(zone.placedItemName) ? "No item available" : zone.placedItemName;
            activePlacements.Add($"{zone.zoneID}: {itemName}");
        }
    }

      // ✅ Update UI text to reflect the placed items
    private void UpdateItemCounter()
    {
        if (itemCounterText != null)
        {
            itemCounterText.text = $"{placedItemCount} / 12";
        }
    }


    public string GetItemInZone(string zoneID)
    {
        return placements.ContainsKey(zoneID) ? placements[zoneID] : null;
    }

    public Dictionary<string, string> GetAllPlacements()
    {
        return placements;
    }

    // ✅ Check if all 12 placement zones have an item
    public void CheckAllZonesFilled()
    {
        int filledZones = 0;
        foreach (var zone in placementZones)
        {
            if (zone.IsOccupied())
            {
                filledZones++;
            }
        }

        if (filledZones >= 12) // ✅ When all 12 zones are filled
        {
            Debug.Log("All 12 placement zones are filled! Moving to Scene 2...");

                // ✅ Disable game objects and script before fading
    if (objectToDisable1 != null) objectToDisable1.SetActive(false);
    if (objectToDisable2 != null) objectToDisable2.SetActive(false);
    if (objectToDisable2 != null) objectToDisable3.SetActive(false);
    if (scriptToDisable != null) scriptToDisable.enabled = false;
    if (scriptToDisable != null) scriptToDisable2.enabled = false;

            StartCoroutine(LoadNextScene()); // ✅ Load Scene 2
        }
    }

private IEnumerator LoadNextScene()
{
    float fadeDuration = 4f;
    float t = 0f;

    // ✅ Disable movement and rotation during fade
var xrRig = GameObject.FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRRig>();
if (xrRig != null)
{
    var moveProvider = xrRig.GetComponent<UnityEngine.XR.Interaction.Toolkit.ContinuousMoveProviderBase>();
    var turnProvider = xrRig.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousTurnProvider>();

    if (moveProvider != null) moveProvider.enabled = false;
    if (turnProvider != null) turnProvider.enabled = false;
}

// ✅ Optional: stop motion cold if Rigidbody is present
var rigBody = xrRig?.GetComponent<Rigidbody>();
if (rigBody != null)
{
    rigBody.velocity = Vector3.zero;
    rigBody.angularVelocity = Vector3.zero;
    rigBody.isKinematic = true; // optional: makes it totally frozen
}


    // Step 1: Enable UI elements
    fadeCanvasGroup.gameObject.SetActive(true);
    instructionText.gameObject.SetActive(true);
    if (fadePanelImage != null)
        fadePanelImage.gameObject.SetActive(true); // ✅ Make sure the panel is visible

    // Step 2: Fade CanvasGroup, Text, and Image alpha
    while (t < fadeDuration)
    {
        float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
        fadeCanvasGroup.alpha = alpha;
        instructionText.alpha = alpha;

        if (fadePanelImage != null)
        {
            Color color = fadePanelImage.color;
            color.a = alpha;
            fadePanelImage.color = color;
        }

        t += Time.deltaTime;
        yield return null;
    }

    // Finalize alpha
    fadeCanvasGroup.alpha = 1f;
    instructionText.alpha = 1f;

    if (fadePanelImage != null)
    {
        Color finalColor = fadePanelImage.color;
        finalColor.a = 1f;
        fadePanelImage.color = finalColor;
    }

    // Step 3: Wait before scene change
    yield return new WaitForSeconds(15f);

    // Step 4: Load the next scene
    SceneManager.LoadScene(1);
}
}