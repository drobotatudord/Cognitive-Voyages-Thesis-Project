using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

public class RecallPlacementManager : MonoBehaviour
{
    public static RecallPlacementManager Instance;

    private Dictionary<string, string> phase1Placements = new Dictionary<string, string>();
    private Dictionary<string, string> phase2Placements = new Dictionary<string, string>();

    private RecallPlacementZone currentPlacementZone; // ✅ Track the active placement zone

    public TMP_Text itemCounterText; // ✅ UI text to display item count

    private int placedItemCount = 0; // ✅ Track number of placed items
    private bool isFading = false;

    [Header("Fade UI")]
public CanvasGroup fadeCanvasGroup;
public TMPro.TMP_Text instructionText;
public UnityEngine.UI.Image fadePanelImage; // optional


    public GameObject objectToDisable1; // Assign in inspector
public GameObject objectToDisable2;
public GameObject objectToDisable3;
public MonoBehaviour scriptToDisable;
public MonoBehaviour scriptToDisable2;

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


private IEnumerator FadeAndCompletePhase2()
{
    float fadeDuration = 4f;
    float t = 0f;

    // ✅ 1. Freeze XR rig movement and physics
    var xrRig = GameObject.FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRRig>();
    if (xrRig != null)
    {
        var moveProvider = xrRig.GetComponent<UnityEngine.XR.Interaction.Toolkit.ContinuousMoveProviderBase>();
        var turnProvider = xrRig.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousTurnProvider>();

        if (moveProvider != null) moveProvider.enabled = false;
        if (turnProvider != null) turnProvider.enabled = false;

        var rigBody = xrRig.GetComponent<Rigidbody>();
        if (rigBody != null)
        {
            rigBody.velocity = Vector3.zero;
            rigBody.angularVelocity = Vector3.zero;
            rigBody.isKinematic = true; // optional: freeze completely
        }
    }

    // ✅ 2. Enable UI elements
    fadeCanvasGroup.gameObject.SetActive(true);
    instructionText.gameObject.SetActive(true);
    if (fadePanelImage != null)
        fadePanelImage.gameObject.SetActive(true);

    // ✅ 3. Fade in UI
    while (t < fadeDuration)
    {
        float alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
        fadeCanvasGroup.alpha = alpha;
        instructionText.alpha = alpha;

        if (fadePanelImage != null)
        {
            var color = fadePanelImage.color;
            color.a = alpha;
            fadePanelImage.color = color;
        }

        t += Time.deltaTime;
        yield return null;
    }

    // ✅ 4. Finalize fade
    fadeCanvasGroup.alpha = 1f;
    instructionText.alpha = 1f;

    if (fadePanelImage != null)
    {
        var finalColor = fadePanelImage.color;
        finalColor.a = 1f;
        fadePanelImage.color = finalColor;
    }

    // ✅ 5. Wait and proceed
    yield return new WaitForSeconds(15f);
    Debug.Log("✅ Phase 2 fade complete!");
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
        return false;
    }

    placedItemCount++;
    UpdateItemCounter();
    phase2Placements[zoneID] = itemName;
    DataManager.Instance.StorePlacement(zoneID, itemName);

    if (placedItemCount >= 12 && !isFading)
    {
        isFading = true;
         if (objectToDisable1 != null) objectToDisable1.SetActive(false);
    if (objectToDisable2 != null) objectToDisable2.SetActive(false);
    if (objectToDisable2 != null) objectToDisable3.SetActive(false);
    if (scriptToDisable != null) scriptToDisable.enabled = false;
    if (scriptToDisable != null) scriptToDisable2.enabled = false;
        Debug.Log("✅ All 12 items placed in Phase 2!");
        StartCoroutine(FadeAndCompletePhase2());
    }

    return true;
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