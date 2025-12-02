using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "New Item")]
public abstract class ItemClass : ScriptableObject
{
    [Header("Identification")]
    public string itemId;       // <-- ID KHỚP VỚI BACKEND
    public string itemName;     // (tên hiển thị)

    [Header("Icon")]
    public Sprite itemIcon;

    [Header("Stack")]
    public bool isStackable = true;
    public int maxStackQuantity = 0;

    // Type getter
    public abstract ItemClass GetItem();
    public abstract ToolClass GetTool();
    public abstract MiscClass GetMisc();
    public abstract ConsumableClass GetConsumable();
}
