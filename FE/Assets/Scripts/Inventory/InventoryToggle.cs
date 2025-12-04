using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryCanvas;

    bool isOpen = false;

    void Start()
    {
        if (inventoryCanvas != null)
            inventoryCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;
            inventoryCanvas.SetActive(isOpen);

            if (AudioManager.Instance != null)
            {
                if (isOpen)
                    AudioManager.Instance.PlayInventoryOpen();
                else
                    AudioManager.Instance.PlayInventoryClose();
            }
        }
    }
}
