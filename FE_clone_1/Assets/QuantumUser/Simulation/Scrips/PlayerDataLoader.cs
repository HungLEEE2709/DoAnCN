using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerDataLoader : MonoBehaviour
{
    // API LẤY NHÂN VẬT ĐANG ĐƯỢC CHỌN
    public string apiGetPlayer = "http://localhost:5000/api/player-info/chosen/";

    public IEnumerator LoadPlayerFromServer(System.Action<bool> onDone)
    {
        string userId = PlayerPrefs.GetString("idUser", "");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Không có idUser → quay lại Login");
            onDone(false);
            yield break;
        }

        string url = apiGetPlayer + userId;
        Debug.Log("Gọi API: " + url);

        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API lỗi: " + req.error);
            onDone(false);
            yield break;
        }

        string json = req.downloadHandler.text;
        Debug.Log("API trả về: " + json);

        PlayerInfoResponse data = JsonUtility.FromJson<PlayerInfoResponse>(json);

        if (data == null || data.player == null)
        {
            Debug.LogError("Không có nhân vật đang chọn → chuyển CreatePlayer");
            onDone(false);
            yield break;
        }

        // Lưu Prefab + UserName
        PlayerPrefs.SetString("PrefabKey", data.player.PrefabKey);
        PlayerPrefs.SetString("PlayerName", data.player.UserName);
        PlayerPrefs.Save();

        Debug.Log(">>> PrefabKey = " + data.player.PrefabKey);

        onDone(true);
    }
}

[System.Serializable]
public class PlayerInfoResponse
{
    public bool success;
    public PlayerData player;
}

[System.Serializable]
public class PlayerData
{
    public string UserName;
    public string PrefabKey;
}
