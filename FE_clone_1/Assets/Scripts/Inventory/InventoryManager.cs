using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject slotsHolder;
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;
    [SerializeField] private SlotClass[] items;
    [SerializeField] private SlotClass[] startingItems;

    [Header("Moving Items")]
    [SerializeField] private SlotClass movingSlot;
    [SerializeField] private SlotClass originalSlot;
    [SerializeField] private SlotClass tempSlot;

    [Header("UI")]
    public Image itemCursor;
    public string userId;

    private GameObject[] slots;
    public bool isMoving;

    void Start()
    {
        // 🔥 LẤY USER ID TỰ ĐỘNG TỪ LOGIN (đúng key LoginManager lưu)
        userId = PlayerPrefs.GetString("idUser", "");

        Debug.Log("====================================================");
        Debug.Log("📌 [InventoryManager] STARTED in scene: " + gameObject.scene.name);
        Debug.Log("📌 userId (from PlayerPrefs) = " + userId);
        Debug.Log("📌 slotsHolder = " + slotsHolder);
        Debug.Log("====================================================");

        if (slotsHolder == null)
        {
            Debug.LogError("❌ slotsHolder = NULL → Bạn chưa gán SlotsHolder trong Inspector!");
            return;
        }

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ USER ID RỖNG → Bạn chưa login, hoặc login chưa lưu PlayerPrefs!");
            return;
        }

        // 1) LẤY TẤT CẢ SLOT TRONG UI
        int count = slotsHolder.transform.childCount;
        Debug.Log("📦 Tổng số UI Slots tìm thấy: " + count);

        slots = new GameObject[count];
        items = new SlotClass[count];

        for (int i = 0; i < count; i++)
        {
            slots[i] = slotsHolder.transform.GetChild(i).gameObject;
            items[i] = new SlotClass();
        }

        // 2) SLOT TẠM CHO KÉO THẢ
        originalSlot = new SlotClass();
        movingSlot = new SlotClass();
        tempSlot = new SlotClass();

        if (InventoryAPI.Instance == null)
        {
            Debug.LogError("❌ InventoryAPI.Instance = NULL!");
            return;
        }

        string url = "http://localhost:5000/api/inventory/" + userId;
        Debug.Log("🌐 Gửi LoadInventory tới URL: " + url);

        // 3) GỌI API
        StartCoroutine(InventoryAPI.Instance.LoadInventory(userId, (inv) =>
        {
            Debug.Log("📥 API callback triggered");

            if (inv == null)
            {
                Debug.LogError("❌ API returned NULL Inventory!");
                return;
            }

            if (inv.slots == null)
            {
                Debug.LogError("❌ inv.slots = NULL!");
                return;
            }

            // 4) Đổ dữ liệu vào UI
            for (int i = 0; i < items.Length; i++)
            {
                if (i >= inv.slots.Length) break;

                string id = inv.slots[i].itemId;

                if (string.IsNullOrEmpty(id))
                {
                    items[i].RemoveItem();
                    continue;
                }

                var item = ItemDatabase.Get(id);
                if (item == null)
                {
                    Debug.LogWarning("⚠ Item không tồn tại: " + id);
                    items[i].RemoveItem();
                    continue;
                }

                items[i].AddItem(item, inv.slots[i].quantity);
            }

            RefreshUI();
            Debug.Log("<color=lime>🎉 INVENTORY UI ĐÃ CẬP NHẬT!</color>");
        }));
    }



    private void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Image icon = slots[i].transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI qty = slots[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            if (items[i].GetItem() == null)
            {
                icon.enabled = false;
                icon.sprite = null;
                qty.text = "";
                continue;
            }

            icon.enabled = true;
            icon.sprite = items[i].GetItem().itemIcon;

            qty.text = items[i].GetItem().isStackable ?
                        items[i].GetQuantity().ToString() : "";
        }
    }
}
