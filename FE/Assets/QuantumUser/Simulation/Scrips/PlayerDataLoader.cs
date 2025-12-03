using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PlayerDataLoader : MonoBehaviour
{
    public string apiGetPlayer = "http://localhost:5000/api/playerInfo/chosen/";

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

        // Lưu Prefab + UserName + Stats
        PlayerPrefs.SetString("PrefabKey", data.player.PrefabKey);
        PlayerPrefs.SetString("PlayerName", data.player.UserName);
        
        PlayerPrefs.SetInt("Player_MaxHp", data.player.MaxHp);
        PlayerPrefs.SetInt("Player_Hp", data.player.Hp);
        PlayerPrefs.SetInt("Player_MaxKi", data.player.MaxKi);
        PlayerPrefs.SetInt("Player_Ki", data.player.Ki);
        PlayerPrefs.SetInt("Player_Dame", data.player.Dame);
        PlayerPrefs.SetInt("Player_SucManh", data.player.SucManh);
        PlayerPrefs.SetInt("Player_TiemNang", data.player.TiemNang);
        
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
    public int SucManh;
    public int TiemNang;
    public int Hp;
    public int MaxHp;
    public int Ki;
    public int MaxKi;
    public int Dame;
}
