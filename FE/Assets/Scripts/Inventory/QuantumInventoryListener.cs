using Quantum;
using UnityEngine;

public class QuantumInventoryListener : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("[QuantumListener] Script ENABLED! Listening for events...");
        QuantumEvent.Subscribe<EventItemPickedUp>(this, OnItemPickedUp);
    }

    private void OnDisable()
    {
        QuantumEvent.UnsubscribeListener(this);
    }

    private void OnItemPickedUp(EventItemPickedUp e)
    {
        // Kiểm tra xem event này có phải của local player không
        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        // Lấy player link với client này
        // Trong Quantum, thường check xem PlayerRef trong event có match với local player không
        // Tuy nhiên, logic đơn giản nhất là check xem PlayerRef có phải là LocalPlayer không
        
        // Cách đơn giản: Kiểm tra nếu InventoryManager có tồn tại
        if (InventoryManager.Instance == null) return;

        // TODO: Cần check xem e.Player có phải là local player không để tránh update inventory của người khác
        // Nhưng hiện tại InventoryManager đang dùng userId từ PlayerPrefs, tức là nó chỉ quản lý inventory của local user.
        // Nếu e.Player tương ứng với local user thì mới add.
        
        // Tạm thời add luôn để test logic, sau này cần map PlayerRef -> Local User
        Debug.Log($"[QuantumListener] EVENT RECEIVED! ItemID: {e.ItemId} Qty: {e.Quantity} Player: {e.Player}");
        
        bool isLocal = game.PlayerIsLocal(e.Player);
        Debug.Log($"[QuantumListener] Is Local Player? {isLocal}");

        // Kiểm tra xem PlayerRef của event có phải là local player không
        if (isLocal)
        {
             Debug.Log("[QuantumListener] Calling InventoryManager.PickupItem...");
             
             // Map int ItemId to string for ItemDatabase
             string itemIdString = GetItemIdString(e.ItemId);
             Debug.Log($"[QuantumListener] Mapped ItemId {e.ItemId} -> '{itemIdString}'");
             
             InventoryManager.Instance.PickupItem(itemIdString, e.Quantity);
        }
        else
        {
             Debug.LogWarning("[QuantumListener] Ignored because Player is not local.");
        }
    }

    // Mapping table: int -> string
    private string GetItemIdString(int itemId)
    {
        switch (itemId)
        {
            case 1: return "apple";
            case 2: return "sword";
            case 3: return "potion";
            // Thêm các item khác ở đây
            default:
                Debug.LogWarning($"[QuantumListener] Unknown ItemId: {itemId}");
                return itemId.ToString();
        }
    }
    }

