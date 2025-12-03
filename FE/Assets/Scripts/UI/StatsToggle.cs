using UnityEngine;

public class StatsToggle : MonoBehaviour
{
    public GameObject statsCanvas;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleStats();
        }
    }

    void ToggleStats()
    {
        if (statsCanvas != null)
        {
            statsCanvas.SetActive(!statsCanvas.activeSelf);
        }
    }
}
