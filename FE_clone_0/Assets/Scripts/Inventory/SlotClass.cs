using UnityEngine;

[System.Serializable]
public class SlotClass
{
    [SerializeField] private ItemClass item;
    [SerializeField] private int quantity = 0;

    public SlotClass()
    {
        item = null;
        quantity = 0;
    }

    public SlotClass(ItemClass _item, int _quantity)
    {
        item = _item;
        quantity = _quantity;
    }

    public ItemClass GetItem()
    {
        if (quantity <= 0) return null;
        return item;
    }

    public int GetQuantity() { return quantity; }

    public void AddQuantity(int _quantity)
    {
        quantity += _quantity;
    }

    public void SubQuantity(int _quantity)
    {
        quantity -= _quantity;
        if (quantity <= 0) RemoveItem();
    }

    public void SetQuantity(int _quantity)
    {
        quantity = _quantity;
        if (quantity <= 0) RemoveItem();
    }

    public void AddItem(ItemClass newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;
    }

    public void RemoveItem()
    {
        item = null;
        quantity = 0;
    }
}
