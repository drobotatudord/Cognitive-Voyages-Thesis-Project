using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // ✅ Needed for Scene Tracking

public class ItemController : MonoBehaviour
{
    private PlacementZone currentZone;
    private bool isPlaced = false;

    [Header("Placement Anchoring")]
    public Transform snapAnchor;

    public AudioSource audioSource; // ✅ Reference to Audio Source
    public AudioClip placementSound; // ✅ The sound to play when placed


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlacementZone"))
        {
            currentZone = other.GetComponent<PlacementZone>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlacementZone") && currentZone != null)
        {
            currentZone = null;
        }
    }

    public bool IsPlaced()
{
    return isPlaced;
}


public void PlaceItem()
{
    if (currentZone != null && !isPlaced)
    {
        if (currentZone.TryPlaceItem(this))
        {
            isPlaced = true;
            transform.SetParent(null);

            if (InventoryManager.Instance.currentItem == this.gameObject)
            {
                InventoryManager.Instance.currentItem = null;
            }

            InventoryManager.Instance.TriggerHapticFeedback(0.8f, 0.3f);
        }

        else
        {
             Debug.LogWarning("Item placement failed. Cleaning up via inventory manager.");
             InventoryManager.Instance.HandleFailedPlacement(this.gameObject); // 👈 Add this helper
        }
    }
}

public void ClearCurrentZone()
{
    currentZone = null;
}


public void ResetPlacement()
{
    if (isPlaced && currentZone != null && currentZone.IsPlayerInZone()) // ✅ Check if player is in the correct zone
    {
        currentZone.RemoveItem();
        Destroy(gameObject);
        transform.SetParent(null); // ✅ Detach from hand before deletion
    }
    else
    {
        Debug.Log("You must be in the correct placement zone to reset this item!");
    }
}


public void MoveToSnap(Transform snapPoint)
{
    if (snapPoint == null || snapAnchor == null)
    {
        Debug.LogWarning("❌ Snap point or SnapAnchor is missing.");
        return;
    }

    isPlaced = true;

    // STEP 1: Forcefully reset the rotation FIRST to match snapPoint (no visual mismatch)
    transform.rotation = snapPoint.rotation;

    // STEP 2: Calculate correct position based on new rotation
    Vector3 anchorOffset = transform.position - snapAnchor.position;
    Vector3 finalPosition = snapPoint.position + anchorOffset;

    StartCoroutine(AnimatePlacementToPosition(finalPosition));
}


private IEnumerator AnimatePlacementToPosition(Vector3 targetPos)
{
    float duration = 0.8f;
    float elapsedTime = 0f;
    Vector3 startPos = transform.position;

    while (elapsedTime < duration)
    {
        transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
        elapsedTime += Time.deltaTime;
        yield return null;
    }

    transform.position = targetPos;

    // ✅ Play sound when placement ends
    if (audioSource != null && placementSound != null)
    {
        audioSource.PlayOneShot(placementSound);
    }
}



public PlacementZone GetCurrentZone()
{
    return currentZone;
}
}