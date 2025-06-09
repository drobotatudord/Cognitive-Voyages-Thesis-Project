using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public void ResetItem(ItemController item)
{
    if (item != null && item.GetCurrentZone() != null)
    {
        // ✅ Only reset if the player is in the correct placement zone
        if (item.GetCurrentZone().IsPlayerInZone())
        {
            item.ResetPlacement();
        }
        else
        {
            Debug.Log("You must be in the correct placement zone to reset this item!");
        }
    }
}
}