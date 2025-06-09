using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class MovementActivator : MonoBehaviour
{
    public GameObject[] walkingInPlaceObjects;
    public MonoBehaviour[] walkingInPlaceScripts;

    public GameObject[] teleportationObjects;
    public MonoBehaviour[] teleportationScripts;

void Start()
{
    StartCoroutine(DelayedActivate());
}

private IEnumerator DelayedActivate()
{
    yield return new WaitForSeconds(0.2f); // Let Unity init everything

    var condition = MovementConditionManager.Instance.GetCondition();
    bool useTeleport = condition == MovementCondition.Teleportation;

    foreach (var obj in walkingInPlaceObjects)
        if (obj != null) obj.SetActive(!useTeleport);

    foreach (var script in walkingInPlaceScripts)
        if (script != null) script.enabled = !useTeleport;

    // ✅ Do NOT touch teleport scripts/objects unless it's NOT teleportation
    if (!useTeleport)
    {
        foreach (var obj in teleportationObjects)
            if (obj != null) obj.SetActive(false);

        foreach (var script in teleportationScripts)
            if (script != null) script.enabled = false;
    }

    Debug.Log($"🔁 [Delayed Toggle] Movement mode: {(useTeleport ? "Teleportation" : "Walking In Place")}");
}


}