using UnityEngine;
using UnityEngine.Networking;
using Quantum;
using System.Collections;
using System.Collections.Generic;

public class MapStateSaver : MonoBehaviour
{
    private string saveUrl = GameConfig.BaseUrl + "/api/mapstate/save";

    private void OnApplicationQuit()
    {
        SaveMapState();
    }

    public unsafe void SaveMapState()
    {
        if (QuantumRunner.Default == null || QuantumRunner.Default.Game == null) return;

        Frame f = QuantumRunner.Default.Game.Frames.Verified;
        string userId = PlayerPrefs.GetString("idUser", "");

        if (string.IsNullOrEmpty(userId)) return;

        // 1. Get Player Position
        PlayerPositionData playerPosData = new PlayerPositionData();
        var players = f.GetComponentIterator<PlayerInfo>();
        foreach (var entry in players)
        {
             // Assuming local player is the one we want to save, or just the first one found for now
             // Ideally we check if it's the local player
             if (f.Unsafe.TryGetPointer<Transform2D>(entry.Entity, out var t))
             {
                 playerPosData.x = t->Position.X.AsFloat;
                 playerPosData.y = t->Position.Y.AsFloat;
                 break; 
             }
        }

        // 2. Get Enemies
        List<EnemyStateData> enemyList = new List<EnemyStateData>();
        var enemies = f.GetComponentIterator<EnemyInfo>();
        foreach (var entry in enemies)
        {
            var info = entry.Component;
            enemyList.Add(new EnemyStateData
            {
                id = info.EnemyID,
                x = info.SpawnPosition.X.AsFloat,
                y = info.SpawnPosition.Y.AsFloat,
                hp = info.CurrentHealth.AsFloat,
                isDead = info.IsDead
            });
        }

        // 3. Send Payload
        MapStatePayload payload = new MapStatePayload
        {
            idUser = userId,
            PlayerPosition = playerPosData,
            Enemies = enemyList,
            MaxHp = 0,
            MaxKi = 0,
            Dame = 0,
            Vang = 0,
            TiemNang = 0,
            SucManh = 0
        };

        // Get Stats from PlayerInfo
        foreach (var entry in players)
        {
             var pInfo = entry.Component;
             // Assuming this is the correct player (should check PlayerRef if possible, but for single player it's fine)
             payload.MaxHp = (int)pInfo.MaxHealth.AsFloat;
             payload.MaxKi = (int)pInfo.MaxKi.AsFloat;
             payload.Dame = (int)pInfo.Damage.AsFloat;
             payload.Vang = pInfo.Vang;
             payload.TiemNang = pInfo.TiemNang;
             payload.SucManh = pInfo.SucManh;
             break; 
        }

        string json = JsonUtility.ToJson(payload);
        Debug.Log($"[MapStateSaver] Sending Payload: {json}"); // LOG PAYLOAD
        StartCoroutine(PostRequest(json));
    }

    IEnumerator PostRequest(string json)
    {
        UnityWebRequest req = new UnityWebRequest(saveUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Saved Map State!");
        }
        else
        {
            Debug.LogError("Save Map Error: " + req.error);
        }
    }
}
