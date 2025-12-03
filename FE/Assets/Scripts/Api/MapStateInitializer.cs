using UnityEngine;
using Quantum;
using System.Collections;
using System.Collections.Generic;
using Photon.Deterministic;

public class MapStateInitializer : MonoBehaviour
{
    IEnumerator Start()
    {
        // Wait for Quantum to start
        while (QuantumRunner.Default == null || QuantumRunner.Default.Game == null)
        {
            yield return null;
        }

        // Wait a bit for entities to spawn
        yield return new WaitForSeconds(0.5f);

        ApplyMapState();
    }

    unsafe void ApplyMapState()
    {
        string json = PlayerPrefs.GetString("MapState", "");
        Debug.Log($"[MapStateInitializer] Loaded JSON: {json}"); // LOG JSON

        if (string.IsNullOrEmpty(json)) return;

        MapStateResponse response = JsonUtility.FromJson<MapStateResponse>(json);
        if (response == null || !response.success) 
        {
            Debug.LogError("[MapStateInitializer] Parse failed or success=false");
            return;
        }

        Frame f = QuantumRunner.Default.Game.Frames.Verified;

        // 1. Restore Player Position
        if (response.PlayerPosition != null)
        {
            var players = f.GetComponentIterator<PlayerInfo>();
            foreach (var entry in players)
            {
                if (f.Unsafe.TryGetPointer<Transform2D>(entry.Entity, out var t))
                {
                    FPVector2 newPos = new FPVector2(FP.FromFloat_UNSAFE(response.PlayerPosition.x), FP.FromFloat_UNSAFE(response.PlayerPosition.y));
                    
                    // Check for valid position (not zero if that's suspicious, but 0,0 is valid. Let's just log)
                    Debug.Log($"[MapStateInitializer] Teleporting Player to: {newPos}");
                    
                    t->Position = newPos;
                    t->Teleport(f, newPos); 
                    break; 
                }
            }
        }

        // 2. Restore Enemies
        if (response.Enemies != null)
        {
            var enemies = f.GetComponentIterator<EnemyInfo>();
            foreach (var entry in enemies)
            {
                var info = entry.Component;
                
                // Find matching state by EnemyID
                foreach (var state in response.Enemies)
                {
                    if (state.id == info.EnemyID)
                    {
                        info.CurrentHealth = FP.FromFloat_UNSAFE(state.hp);
                        info.IsDead = state.isDead;
                        
                        // If dead, ensure health is 0
                        if (info.IsDead) info.CurrentHealth = FP._0;

                        f.Set(entry.Entity, info);
                        Debug.Log($"Restored Enemy ID {info.EnemyID}: HP={state.hp}, Dead={state.isDead}");
                        break;
                    }
                }
            }
        }
    }
}



