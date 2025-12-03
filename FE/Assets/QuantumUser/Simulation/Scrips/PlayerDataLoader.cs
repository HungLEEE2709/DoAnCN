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

        PlayerInfoResponse response = JsonUtility.FromJson<PlayerInfoResponse>(json);

        if (response == null || response.player == null)
        {
            Debug.LogError("Không có nhân vật đang chọn → chuyển CreatePlayer");
            onDone(false);
            yield break;
        }

        PlayerData data = response.player;

        // Lưu Prefab + UserName
        PlayerPrefs.SetString("PrefabKey", data.PrefabKey);
        PlayerPrefs.SetString("PlayerName", data.UserName);

        // Save Current & Max Stats
        PlayerPrefs.SetInt("CurrentHp", data.Hp);
        PlayerPrefs.SetInt("MaxHp", data.MaxHp);
        PlayerPrefs.SetInt("CurrentKi", data.Ki);
        PlayerPrefs.SetInt("MaxKi", data.MaxKi);
        
        // Save Dame, Vang, TiemNang, SucManh
        PlayerPrefs.SetInt("Dame", data.Dame);
        PlayerPrefs.SetInt("Vang", data.Vang);
        PlayerPrefs.SetInt("TiemNang", data.TiemNang);
        PlayerPrefs.SetInt("SucManh", data.SucManh);

        PlayerPrefs.Save();

        Debug.Log(">>> PrefabKey = " + data.PrefabKey);

        // Load Map State
        string mapUrl = "http://localhost:5000/api/mapstate/load/" + userId;
        UnityWebRequest mapReq = UnityWebRequest.Get(mapUrl);
        yield return mapReq.SendWebRequest();

        if (mapReq.result == UnityWebRequest.Result.Success)
        {
            string mapJson = mapReq.downloadHandler.text;
            Debug.Log("Map State: " + mapJson);
            PlayerPrefs.SetString("MapState", mapJson);
        }

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
    public int Hp;
    public int MaxHp;
    public int Ki;
    public int MaxKi;
    public int Dame;
    public int Vang;
    public int TiemNang;
    public int SucManh;
}
