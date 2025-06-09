using UnityEngine;

public class ResetPlayerPrefs : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.SetInt("PlayerID", 0);  // ✅ Reset Player ID back to 0
        PlayerPrefs.Save();                 // ✅ Ensure the changes are saved
        Debug.Log("✅ PlayerID reset to 0");
    }
}