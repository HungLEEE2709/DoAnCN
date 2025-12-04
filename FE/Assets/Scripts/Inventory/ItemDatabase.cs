using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public List<ItemClass> allItems;

    private Dictionary<string, ItemClass> dict;

    private void Awake()
    {
        Debug.Log("ItemDatabase Loaded " + allItems.Count + " items");
        Instance = this;
        dict = new Dictionary<string, ItemClass>();

        foreach (var item in allItems)
        {
            if (string.IsNullOrEmpty(item.itemId))
            {
                Debug.LogError("Item missing itemId: " + item.name);
                continue;
            }

            dict[item.itemId] = item;
        }

        Debug.Log("ItemDatabase Loaded " + dict.Count + " items");
    }

    public static ItemClass Get(string id)
    {
        // Null check để tránh crash
        if (Instance == null)
        {
            Debug.LogError("❌ ItemDatabase.Instance is NULL! Bạn cần thêm GameObject 'ItemDatabase' vào scene!");
            return null;
        }

        if (Instance.dict == null)
        {
            Debug.LogError("❌ ItemDatabase.dict is NULL! Awake() chưa được gọi!");
            return null;
        }

        if (Instance.dict.ContainsKey(id))
            return Instance.dict[id];

        Debug.LogWarning("⚠️ Item not found in database: " + id);
        return null;
    }
}
