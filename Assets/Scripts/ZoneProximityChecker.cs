using System.Collections.Generic;
using UnityEngine;

public class ZoneProximityChecker : MonoBehaviour
{
    public float checkRadius = 0.8f; // adjust as needed
    public LayerMask zoneLayer;

    private PlacementZone currentZone;

void Update()
{
    Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius, zoneLayer);

    if (hits.Length > 0)
    {
        PlacementZone zone = hits[0].GetComponent<PlacementZone>();

        if (zone != null)
        {
            currentZone = zone;

            // Always force re-check to ensure inventory visibility after teleport
            if (!zone.IsOccupied())
            {
                zone.ForcePlayerEnter();
            }
        }
    }
    else
    {
        if (currentZone != null)
        {
            ExitZone(currentZone);
        }
    }
}


private void EnterZone(PlacementZone zone)
{
    if (zone == null || zone == currentZone)
        return;

    if (zone.IsOccupied()) return; // ✅ Prevent re-entering already-placed zones

    currentZone = zone;
    zone.ForcePlayerEnter(); // simulate trigger enter
}

    private void ExitZone(PlacementZone zone)
    {
        zone.ForcePlayerExit(); // simulate trigger exit
        currentZone = null;
    }

    void OnDrawGizmos()
{
    Gizmos.color = Color.red; // Or whatever color you like
    Gizmos.DrawWireSphere(transform.position, checkRadius);
}

}