using UnityEngine.Networking;
using UnityEngine;

namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class PlayerController : SystemMainThreadFilter<PlayerController.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform2D* Transform;
            public PlayerInfo* Info;
        }

        public override void Update(Frame frame, ref Filter f)
        {
            var input = frame.GetPlayerInput(f.Info->PlayerRef);
            
            // Debug Log every 60 frames
            if (frame.Number % 60 == 0) Log.Debug($"[PlayerController] Update. Input Attack: {input->Attack.WasPressed}");

            // Manually get Body
            if (frame.Unsafe.TryGetPointer<PhysicsBody2D>(f.Entity, out var body))
            {
                // MOVE
                body->Velocity = input->Direction * f.Info->Speed;
            }

            // ATTACK START
            if (input->Attack.WasPressed)
            {
                // thời gian attack (20 frame ~ 1/3 giây nếu 60fps)
                f.Info->AttackTimer = 20;
                f.Info->IsAttacking = true;
                UnityEngine.Debug.Log($"[PlayerController] ATTACK START! Damage: {f.Info->Damage}");

                // HIT DETECTION
                var enemies = frame.GetComponentIterator<EnemyInfo>();
                FPVector2 playerPos = f.Transform->Position;
                FP range = FP.FromFloat_UNSAFE(2.0f); // Hardcoded range

                foreach (var enemy in enemies)
                {
                    var eInfo = enemy.Component;
                    if (eInfo.IsDead) continue;

                    FP dist = FPVector2.Distance(playerPos, eInfo.SpawnPosition); // Default
                    
                    // Try to get actual position
                    if (frame.Unsafe.TryGetPointer<Transform2D>(enemy.Entity, out var eTr))
                    {
                        dist = FPVector2.Distance(playerPos, eTr->Position);
                    }

                    UnityEngine.Debug.Log($"[PlayerController] Checking Enemy {eInfo.EnemyID}. Dist: {dist}. Range: {range}");

                    if (dist <= range)
                    {
                        eInfo.CurrentHealth -= f.Info->Damage;
                        UnityEngine.Debug.Log($"[PlayerController] HIT! OldHP: {eInfo.CurrentHealth + f.Info->Damage} -> NewHP: {eInfo.CurrentHealth}");
                        
                        if (eInfo.CurrentHealth <= FP._0)
                        {
                            eInfo.CurrentHealth = FP._0;
                            eInfo.IsDead = true;
                            eInfo.IsAttacking = false;
                            UnityEngine.Debug.Log($"[PlayerController] Enemy {eInfo.EnemyID} KILLED!");

                            // === REWARD LOGIC ===
                            f.Info->TiemNang += 100;
                            f.Info->SucManh += 100;
                            UnityEngine.Debug.Log($"[PlayerController] Reward 100 TN + 100 SM. Total TN: {f.Info->TiemNang}");

                            // === SPAWN GOLD ITEM ===
                            var goldProto = UnityEngine.Resources.Load<EntityPrototype>("Entities/Item/GoldItemEntityPrototypeEntityPrototype");
                            if (goldProto == null) goldProto = UnityEngine.Resources.Load<EntityPrototype>("Entities/Item/GoldItemEntityPrototype");

                            if (goldProto != null)
                            {
                                var goldEnt = frame.Create(goldProto);
                                
                                if (frame.Unsafe.TryGetPointer<Transform2D>(goldEnt, out var t))
                                {
                                    if (frame.Unsafe.TryGetPointer<Transform2D>(enemy.Entity, out var enemyTr))
                                    {
                                        t->Position = enemyTr->Position;
                                    }
                                    else
                                    {
                                        t->Position = eInfo.SpawnPosition;
                                    }
                                    UnityEngine.Debug.Log($"[PlayerController] Spawned Gold Item at {t->Position}");
                                }

                                if (frame.Unsafe.TryGetPointer<ItemInfo>(goldEnt, out var item))
                                {
                                    item->ItemId = 0; // Gold
                                    item->Quantity = frame.Global->RngSession.Next(50, 150);
                                }
                                
                                }
                            else
                            {
                                UnityEngine.Debug.LogError("[PlayerController] Could not load GoldItemEntityPrototype!");
                            }
                            
                            
                        }
                        frame.Set(enemy.Entity, eInfo);
                    }
                }
            }

            // ATTACK UPDATE
            if (f.Info->AttackTimer > 0)
            {
                f.Info->AttackTimer--;
            }
            else
            {
                f.Info->IsAttacking = false;
            }
        }
    }
}
