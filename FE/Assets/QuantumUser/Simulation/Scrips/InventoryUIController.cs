using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private GameObject inventoryRoot;   // KHÔNG PHẢI CANVAS! Chỉ root của inventory

    private bool isOpen = false;

    private void Start()
    {
        if (inventoryRoot != null)
            inventoryRoot.SetActive(false);
        else
            Debug.LogError("❌ InventoryRoot chưa gán!");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleInventory();
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryRoot.SetActive(isOpen);
    }
}
