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
                    Log.Info($"[ItemUsage] Player {playerIdx} healed {input->HealthRestore} HP. New HP: {playerInfo->CurrentHealth}");
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
                    Log.Info($"[ItemUsage] Player {playerIdx} recovered {input->KiRestore} Ki. New Ki: {playerInfo->Ki}");
                }
            }
        }
    }
}
