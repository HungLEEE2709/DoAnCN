using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class InventoryAPI : MonoBehaviour
{
    public static InventoryAPI Instance;

    private void Awake()
    {
        Instance = this;
        Debug.Log("[API] InventoryAPI Initialized.");
    }

    // =======================================================
    //  LOAD INVENTORY  (server sẽ tự tạo nếu chưa tồn tại)
    // =======================================================
    public IEnumerator LoadInventory(string userId, System.Action<InventoryContent> callback)
    {
        string url = GameConfig.BaseUrl + "/api/inventory/" + userId;
        Debug.Log("[API] GET " + url);

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        // Lỗi mạng
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[API] Network Error: " + req.error);
            callback?.Invoke(null);
            yield break;
        }

        // Lỗi HTTP
        if (req.responseCode != 200)
        {
            Debug.LogError("[API] HTTP Error: " + req.responseCode);
            Debug.LogError(req.downloadHandler.text);
            callback?.Invoke(null);
            yield break;
        }

        // Parse JSON
        InventoryResponse res = null;

        try
        {
            res = JsonUtility.FromJson<InventoryResponse>(req.downloadHandler.text);
        }
        catch
        {
            Debug.LogError("[API] JSON Parse Error: " + req.downloadHandler.text);
            callback?.Invoke(null);
            yield break;
        }

        // Kiểm tra data
        if (res == null || res.inventory == null)
        {
            Debug.LogError("[API] Invalid Inventory JSON.");
            callback?.Invoke(null);
            yield break;
        }

        Debug.Log("[API] Inventory Loaded OK.");
        callback?.Invoke(res.inventory);
    }

    // =======================================================
    //  SAVE INVENTORY
    // =======================================================
    public IEnumerator SaveInventory(string userId, SlotClass[] items)
    {
        string url = GameConfig.BaseUrl + "/api/inventory/save";

        InventoryContent payload = new InventoryContent();
        payload.userId = userId;
        payload.slots = new SlotData[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            payload.slots[i] = new SlotData
            {
                itemId = items[i].GetItem() == null ? null : items[i].GetItem().itemId,
                quantity = items[i].GetQuantity()
            };
        }

        string json = JsonUtility.ToJson(payload);
        Debug.Log("[API] Saving Inventory JSON = " + json);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        // Lỗi mạng
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[API] Save ERROR: " + req.error);
            yield break;
        }

        // Lỗi server
        if (req.responseCode != 200)
        {
            Debug.LogError("[API] Save HTTP ERROR: " + req.responseCode);
            Debug.LogError(req.downloadHandler.text);
            yield break;
        }

        Debug.Log("[API] Inventory Saved Successfully!");
    }
}
