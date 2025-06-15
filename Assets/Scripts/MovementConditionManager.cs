using UnityEngine;

public enum MovementCondition
{
    Teleportation,
    WalkingInPlace
}

public class MovementConditionManager : MonoBehaviour
{
    public static MovementConditionManager Instance;
    public MovementCondition movementCondition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogError("❌ DataManager.Instance is null! Movement assignment failed.");
            return;
        }

        int playerID = DataManager.Instance.GetPlayerID();
        movementCondition = (playerID % 2 == 0)
            ? MovementCondition.Teleportation
            : MovementCondition.WalkingInPlace;

        Debug.Log($"✅ Movement condition set to: {movementCondition}");
    }

    public MovementCondition GetCondition()
    {
        return movementCondition;
    }

    // ✅ Add this to make "CurrentCondition" work
    public MovementCondition CurrentCondition => movementCondition;
}