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
        if (e == null) return;                       // <-- defensive null‑check

        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[QuantumListener] InventoryManager not found – cannot add item.");
            return;
        }

        Debug.Log($"[QuantumListener] EVENT RECEIVED! ItemID: {e.ItemId} Qty: {e.Quantity} Player: {e.Player}");

        bool isLocal = game.PlayerIsLocal(e.Player);
        Debug.Log($"[QuantumListener] Is Local Player? {isLocal}");

        if (!isLocal)
        {
            Debug.LogWarning("[QuantumListener] Ignored because Player is not local.");
            return;
        }

        string itemIdString = GetItemIdString(e.ItemId);
        if (string.IsNullOrEmpty(itemIdString))
        {
            Debug.LogError($"[QuantumListener] Unable to map ItemId {e.ItemId} – aborting pickup.");
            return;
        }

        Debug.Log($"[QuantumListener] Mapped ItemId {e.ItemId} -> '{itemIdString}'");
        InventoryManager.Instance.PickupItem(itemIdString, e.Quantity);
    }

    // Mapping table: int -> string
    private string GetItemIdString(int itemId)
    {
        switch (itemId)
        {
            case 1: return "apple";
            case 2:
                return "coins" +
                    "";
            case 3: return "Potion";
            default:
                Debug.LogWarning($"[QuantumListener] Unknown ItemId: {itemId}");
                return null;
        }
    }
}
