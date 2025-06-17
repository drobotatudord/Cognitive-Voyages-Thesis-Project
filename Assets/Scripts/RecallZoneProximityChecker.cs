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

            if (zone != null)
            {
                // ✅ Always update zone reference (even if same as before)
                currentZone = zone;

                // ✅ Force inventory open if not occupied
                if (!zone.IsOccupied())
                {
                    zone.ForcePlayerEnter();
                }
            }
        }
        else
        {
            // ✅ Exit logic when no zone is nearby
            if (currentZone != null)
            {
                ExitZone(currentZone);
            }
        }
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