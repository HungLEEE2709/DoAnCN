using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Quantum;
using Photon.Deterministic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("References")]
    [SerializeField] private GameObject slotsHolder;

    private GameObject[] slots;
    private SlotClass[] items;

    [Header("Moving Items")]
    [SerializeField] private SlotClass movingSlot;
    [SerializeField] private SlotClass originalSlot;
    [SerializeField] private SlotClass tempSlot;

    [Header("UI")]
    public Image itemCursor;
    public string userId;

    private bool isMoving;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // LẤY USER ID TỪ LOGIN
        userId = PlayerPrefs.GetString("idUser", "");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ USER ID RỖNG → Bạn chưa login!");
            return;
        }

        if (slotsHolder == null)
        {
            Debug.LogError("❌ slotsHolder = NULL");
            return;
        }

        // LẤY UI SLOTS
        int count = slotsHolder.transform.childCount;

        slots = new GameObject[count];
        items = new SlotClass[count];

        for (int i = 0; i < count; i++)
        {
            slots[i] = slotsHolder.transform.GetChild(i).gameObject;
            items[i] = new SlotClass();
        }

        // SLOT TẠM CHO KÉO THẢ
        originalSlot = new SlotClass();
        movingSlot = new SlotClass();
        tempSlot = new SlotClass();

        // LOAD INVENTORY TỪ API
        StartCoroutine(InventoryAPI.Instance.LoadInventory(userId, OnInventoryLoaded));
    }

    void OnInventoryLoaded(InventoryContent inv)
    {
        if (inv == null)
        {
            Debug.LogError("❌ API trả về NULL");
            return;
        }

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
                items[i].RemoveItem();
                continue;
            }

            items[i].AddItem(item, inv.slots[i].quantity);
        }

        RefreshUI();
        Debug.Log("<color=lime>🎉 INVENTORY LOADED!</color>");
    }

    // ===========================
    // USE ITEM (INPUT POLLING)
    // ===========================
    private int _pendingItemId;
    private int _pendingHealthRestore;
    private int _pendingKiRestore;

    private void OnEnable()
    {
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => OnPollInput(callback));
    }

    private void OnPollInput(CallbackPollInput callback)
    {
        if (_pendingItemId != 0)
        {
            Quantum.Input i = new Quantum.Input();
            i.UseItemId = _pendingItemId;
            i.HealthRestore = _pendingHealthRestore;
            i.KiRestore = _pendingKiRestore;
            
            callback.SetInput(i, DeterministicInputFlags.Command);
            
            // Reset sau khi đã gửi
            _pendingItemId = 0;
            _pendingHealthRestore = 0;
            _pendingKiRestore = 0;
        }
    }

    public void UseItem(SlotClass slot)
    {
        if (slot == null || slot.GetItem() == null) return;

        ConsumableClass consumable = slot.GetItem().GetConsumable();
        if (consumable != null)
        {
            // Queue Input cho Quantum
            _pendingItemId = consumable.itemId.GetHashCode();
            _pendingHealthRestore = consumable.healthRecovery;
            _pendingKiRestore = consumable.kiRecovery;
            Debug.Log($"<color=lime>[Inventory] Đã dùng {consumable.itemName} (Queue Input)</color>");

            // Phát âm thanh
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayItemUseSound();
            }

            // Giảm số lượng
            slot.SubQuantity(1);
            if (slot.GetQuantity() <= 0)
            {
                slot.RemoveItem();
            }

            // Lưu inventory
            StartCoroutine(InventoryAPI.Instance.SaveInventory(userId, items));
            RefreshUI();
        }
    }

    // ===========================
    // UPDATE (KÉO THẢ & USE ITEM)
    // ===========================
    private void Update()
    {
        // ... (Logic kéo thả cũ) ...
        if (UnityEngine.Input.GetMouseButtonDown(0))
        {
            if (isMoving) EndMove();
            else BeginMove();
        }

        if (UnityEngine.Input.GetMouseButtonDown(1))
        {
            if (!isMoving) BeginSplit();
        }

        if (isMoving)
        {
            itemCursor.enabled = true;
            itemCursor.transform.position = UnityEngine.Input.mousePosition;
            itemCursor.sprite = movingSlot.GetItem().itemIcon;
        }
        else
        {
            itemCursor.enabled = false;
            itemCursor.sprite = null;
        }

        // DETECT USE ITEM (Phím E)
        if (UnityEngine.Input.GetKeyDown(KeyCode.E))
        {
            SlotClass slot = GetClosestSlot();
            if (slot != null && slot.GetItem() != null)
            {
                UseItem(slot);
            }
        }
    }

    // ===========================
    // UI UPDATE
    // ===========================
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

    private SlotClass GetClosestSlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            RectTransform rect = slots[i].GetComponent<RectTransform>();

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, UnityEngine.Input.mousePosition))
                return items[i];
        }

        return null;
    }

    private void BeginMove()
    {
        originalSlot = GetClosestSlot();

        if (originalSlot == null || originalSlot.GetItem() == null)
            return;

        movingSlot.AddItem(originalSlot.GetItem(), originalSlot.GetQuantity());
        originalSlot.RemoveItem();

        isMoving = true;
        RefreshUI();
    }

    private void BeginSplit()
    {
        originalSlot = GetClosestSlot();

        if (originalSlot == null || originalSlot.GetItem() == null) return;
        if (originalSlot.GetQuantity() <= 1) return;

        int half = Mathf.CeilToInt(originalSlot.GetQuantity() / 2f);

        movingSlot.AddItem(originalSlot.GetItem(), half);
        originalSlot.SubQuantity(half);

        isMoving = true;
        RefreshUI();
    }

    private void EndMove()
    {
        originalSlot = GetClosestSlot();

        if (originalSlot == null)
        {
            AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
        }
        else
        {
            if (originalSlot.GetItem() != null)
            {
                // STACK
                if (originalSlot.GetItem() == movingSlot.GetItem() &&
                    originalSlot.GetItem().isStackable)
                {
                    int max = originalSlot.GetItem().maxStackQuantity;
                    int sum = originalSlot.GetQuantity() + movingSlot.GetQuantity();

                    if (sum > max)
                    {
                        originalSlot.SetQuantity(max);
                        movingSlot.SetQuantity(sum - max);
                        isMoving = true;
                        RefreshUI();
                        return;
                    }

                    originalSlot.AddQuantity(movingSlot.GetQuantity());
                    movingSlot.RemoveItem();
                }
                else
                {
                    tempSlot.AddItem(originalSlot.GetItem(), originalSlot.GetQuantity());
                    originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                    movingSlot.AddItem(tempSlot.GetItem(), tempSlot.GetQuantity());
                    tempSlot.RemoveItem();
                }
            }
            else
            {
                originalSlot.AddItem(movingSlot.GetItem(), movingSlot.GetQuantity());
                movingSlot.RemoveItem();
            }
        }

        isMoving = false;
        RefreshUI();
    }

    // ===========================
    // ADD / REMOVE
    // ===========================
    public void PickupItem(string itemId, int quantity)
    {
        var item = ItemDatabase.Get(itemId);

        if (item == null)
        {
            Debug.LogError($"[Inventory] Không tìm thấy item với id = {itemId}");
            return;
        }

        AddItem(item, quantity);
        StartCoroutine(InventoryAPI.Instance.SaveInventory(userId, items));
    }

    private void AddItem(ItemClass item, int quantity)
    {
        SlotClass slot = ContainsItem(item);

        if (slot != null && slot.GetItem().isStackable)
        {
            slot.AddQuantity(quantity);
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].GetItem() == null)
                {
                    items[i].AddItem(item, quantity);
                    break;
                }
            }
        }

        RefreshUI();
    }

    private SlotClass ContainsItem(ItemClass item)
    {
        for (int i = 0; i < items.Length; i++)
            if (items[i].GetItem() == item)
                return items[i];

        return null;
    }
}
