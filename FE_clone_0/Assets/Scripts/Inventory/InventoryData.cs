[System.Serializable]
public class SlotData
{
    public string itemId;
    public int quantity;
}

[System.Serializable]
public class InventoryContent
{
    public string userId;
    public SlotData[] slots;
}

[System.Serializable]
public class InventoryResponse
{
    public bool success;
    public InventoryContent inventory;
}
