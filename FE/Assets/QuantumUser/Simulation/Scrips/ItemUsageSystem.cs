using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class ItemUsageSystem : SystemMainThread
    {
        public override void Update(Frame f)
        {
            for (int i = 0; i < f.PlayerCount; i++)
            {
                var input = f.GetPlayerInput(i);
                
                // Check if an item is being used (ItemId > 0)
                if (input->UseItemId > 0)
                {
                    Log.Info($"[ItemUsageSystem] 🎯 Detected UseItemId: {input->UseItemId} | HealthRestore: {input->HealthRestore} | KiRestore: {input->KiRestore}");
                    ProcessUseItem(f, i, input);
                }
            }
        }

        private void ProcessUseItem(Frame f, int playerIdx, Input* input)
        {
            // Find player entity
            EntityRef playerEntity = EntityRef.None;
            var filter = f.Filter<PlayerInfo>();
            while (filter.Next(out var entity, out var info))
            {
                if (info.PlayerRef == playerIdx)
                {
                    playerEntity = entity;
                    break;
                }
            }

            if (playerEntity == EntityRef.None)
            {
                Log.Error($"[ItemUsage] Could not find PlayerEntity for PlayerRef {playerIdx}");
                return;
            }

            Log.Info($"[ItemUsageSystem] ✅ Found PlayerEntity for PlayerRef {playerIdx}");

            PlayerInfo* playerInfo = f.Unsafe.GetPointer<PlayerInfo>(playerEntity);
            bool wasHealed = false;

            // ========== HỒI MÁU ==========
            if (input->HealthRestore > 0)
            {
                FP oldHealth = playerInfo->CurrentHealth;
                playerInfo->CurrentHealth += FP.FromFloat_UNSAFE(input->HealthRestore);

                // Clamp về MaxHealth
                if (playerInfo->CurrentHealth > playerInfo->MaxHealth)
                {
                    playerInfo->CurrentHealth = playerInfo->MaxHealth;
                }

                if (playerInfo->CurrentHealth > oldHealth)
                {
                    wasHealed = true;
                    Log.Info($"[ItemUsage] ✅ Player {playerIdx} healed {input->HealthRestore} HP. Old HP: {oldHealth} → New HP: {playerInfo->CurrentHealth}");
                }
                else
                {
                    Log.Warn($"[ItemUsage] ⚠️ Player {playerIdx} HP is already at MAX! Current HP: {playerInfo->CurrentHealth} / {playerInfo->MaxHealth}");
                }
            }

            // ========== HỒI KI ==========
            if (input->KiRestore > 0)
            {
                FP oldKi = playerInfo->Ki;
                playerInfo->Ki += FP.FromFloat_UNSAFE(input->KiRestore);

                // Clamp về MaxKi
                if (playerInfo->Ki > playerInfo->MaxKi)
                {
                    playerInfo->Ki = playerInfo->MaxKi;
                }

                if (playerInfo->Ki > oldKi)
                {
                    wasHealed = true;
                    Log.Info($"[ItemUsage] ✅ Player {playerIdx} recovered {input->KiRestore} Ki. Old Ki: {oldKi} → New Ki: {playerInfo->Ki}");
                }
                else
                {
                    Log.Warn($"[ItemUsage] ⚠️ Player {playerIdx} Ki is already at MAX! Current Ki: {playerInfo->Ki} / {playerInfo->MaxKi}");
                }
            }
            
            // ⚠️ IMPORTANT: Clear the input to prevent processing the same item multiple times
            // This is necessary when using Repeatable input flags
            input->UseItemId = 0;
            input->HealthRestore = 0;
            input->KiRestore = 0;
            Log.Info($"[ItemUsageSystem] 🔄 Input consumed and cleared for PlayerRef {playerIdx}");
        }
    }
}
