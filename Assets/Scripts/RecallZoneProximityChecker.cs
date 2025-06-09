using UnityEngine;

public class RecallZoneProximityChecker : MonoBehaviour
{
    public float checkRadius = 0.8f;
    public LayerMask zoneLayer;

    private RecallPlacementZone currentZone;

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius, zoneLayer);

        if (hits.Length > 0)
        {
            RecallPlacementZone zone = hits[0].GetComponent<RecallPlacementZone>();
            if (zone != null && zone != currentZone)
            {
                EnterZone(zone);
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

private void EnterZone(RecallPlacementZone zone)
{
    if (zone == null || zone == currentZone)
        return;

    // ✅ Do NOT trigger inventory if already placed
    if (zone.IsOccupied()) return;

    currentZone = zone;
    zone.ForcePlayerEnter();
}


    private void ExitZone(RecallPlacementZone zone)
    {
        zone.ForcePlayerExit();
        currentZone = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}