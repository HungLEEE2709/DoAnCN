using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class EnemyController : SystemMainThreadFilter<EnemyController.Filter>
    {

        public struct Filter
        {
            public EntityRef Entity;
            public EnemyInfo* Info;
        }
        private static readonly FP AttackWindup = FP.FromFloat_UNSAFE(0.8f);
        private static readonly FP AttackInterval = FP.FromFloat_UNSAFE(0.7f);

        public override void Update(Frame f, ref Filter filter)
        {

            EnemyInfo* info = filter.Info;
            if (!f.Unsafe.TryGetPointer<Transform2D>(filter.Entity, out var transform))
            {
                if (f.Number % 120 == 0) Log.Error($"Enemy {filter.Entity} missing Transform2D!");
                return;
            }
            
            if (!f.Unsafe.TryGetPointer<PhysicsBody2D>(filter.Entity, out var body))
            {
                if (f.Number % 120 == 0) Log.Error($"Enemy {filter.Entity} missing PhysicsBody2D!");
                return;
            }

            // Debug Log to check if System is running
            if (f.Number % 120 == 0) Log.Debug($"EnemyController Update Running for Entity {filter.Entity}");

            //INITIALIZATION
            if (!info->IsInitialized)
            {
                info->SpawnPosition = transform->Position;
                info->IsInitialized = true;
            }

            //DEATH 
            if (info->CurrentHealth <= FP._0 && !info->IsDead)
            {
                info->CurrentHealth = FP._0;
                info->IsDead = true;
                info->IsAttacking = false;
                UnityEngine.Debug.Log($"[EnemyController] Enemy {info->EnemyID} DIED!");

                // REWARD LOGIC
                EntityRef killerEnt = info->PlayerEntity;
                if (f.Exists(killerEnt) && f.Has<PlayerInfo>(killerEnt))
                {
                    // Reward Player with TiemNang and SucManh
                    var killerInfo = f.Get<PlayerInfo>(killerEnt);
                    killerInfo.TiemNang += 100;
                    killerInfo.SucManh += 100;
                    f.Set(killerEnt, killerInfo);

                    UnityEngine.Debug.Log($"Enemy Died! Killer {killerEnt} got 100 TN + 100 SM. Total TN: {killerInfo.TiemNang}, SM: {killerInfo.SucManh}");

                    // SPAWN GOLD ITEM
                    UnityEngine.Debug.Log("Attempting to load GoldItemEntityPrototype...");
                    var goldProto = UnityEngine.Resources.Load<EntityPrototype>("Entities/Item/GoldItemEntityPrototypeEntityPrototype");
                    if (goldProto == null)
                    {
                        UnityEngine.Debug.LogWarning("Failed to load 'Entities/Item/GoldItemEntityPrototypeEntityPrototype'. Trying 'Entities/Item/GoldItemEntityPrototype'...");
                        goldProto = UnityEngine.Resources.Load<EntityPrototype>("Entities/Item/GoldItemEntityPrototype");
                    }

                    if (goldProto != null)
                    {
                        var goldEnt = f.Create(goldProto);
                        UnityEngine.Debug.Log($"Created Gold Entity: {goldEnt}");
                        
                        if (f.Has<Transform2D>(goldEnt))
                        {
                            var t = f.Get<Transform2D>(goldEnt);
                            t.Position = transform->Position;
                            f.Set(goldEnt, t);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"Gold Entity {goldEnt} does not have Transform2D component!");
                        }

                        if (f.Has<ItemInfo>(goldEnt))
                        {
                            var item = f.Get<ItemInfo>(goldEnt);
                            item.ItemId = 0; // 0 = Gold
                            item.Quantity = f.Global->RngSession.Next(50, 150); // Random Gold 50-150
                            f.Set(goldEnt, item);
                            UnityEngine.Debug.Log($"Set ItemInfo for {goldEnt}: Qty={item.Quantity}");
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"Gold Entity {goldEnt} does not have ItemInfo component!");
                        }
                        
                        UnityEngine.Debug.Log($"Spawned Gold Item at {transform->Position}");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Could not load GoldItemEntityPrototype! Checked both paths.");
                    }
                }
            }
            
            if (info->CurrentHealth < info->Health && !info->IsDead)
            {
                 // Log.Debug($"[EnemyController] Enemy {info->EnemyID} HP: {info->CurrentHealth}");
            }

            if (info->IsDead)
            {
                info->IsAttacking = false;
                body->Velocity = FPVector2.Zero;

                // Respawn Logic
                info->RespawnTimer += f.DeltaTime;
                
                // Log every second
                if (f.Number % 60 == 0) 
                    Log.Debug($"Enemy {info->EnemyID} Dead. Timer: {info->RespawnTimer}");

                if (info->RespawnTimer >= 10)
                {
                    info->IsDead = false;
                    
                    // Safety check: If Max Health is 0, set to default 100
                    if (info->Health <= FP._0) info->Health = FP.FromFloat_UNSAFE(100);
                    
                    info->CurrentHealth = info->Health; // Reset HP to Max
                    transform->Position = info->SpawnPosition; // Reset Position
                    info->RespawnTimer = FP._0;
                    Log.Debug($"Enemy {info->EnemyID} RESPAWNED at {info->SpawnPosition}. HP: {info->CurrentHealth}");
                }
                return;
            }

            //GET PLAYER
            EntityRef playerEnt = info->PlayerEntity;
            if (!f.Exists(playerEnt))
                return;

            PlayerInfo* playerPtr = f.Unsafe.GetPointer<PlayerInfo>(playerEnt);
            Transform2D* playerTr = f.Unsafe.GetPointer<Transform2D>(playerEnt);

            FPVector2 enemyPos = transform->Position;
            FPVector2 playerPos = playerTr->Position;

            FP dist = FPVector2.Distance(enemyPos, playerPos);
            info->Time += f.DeltaTime;

            //PLAYER TRONG TẦM PHÁT HIỆN 
            if (dist < info->DetectionRange)
            {

                // ======================
                // TRONG TẦM CHÉM
                // ======================
                if (dist < info->AttackRange)
                {

                    // không chạy nữa, đứng chém
                    body->Velocity = FPVector2.Zero;

                    // nếu chưa bắt đầu chém và đã hết cooldown -> bắt đầu cú chém mới
                    if (!info->IsAttacking && info->AttackCooldown <= FP._0)
                    {
                        info->IsAttacking = true;   // bật animator Attack
                        info->AttackCooldown = FP._0; // dùng như timer đếm thời gian vung chém
                    }

                    if (info->IsAttacking)
                    {
                        info->AttackCooldown += f.DeltaTime;
                        if (info->AttackCooldown >= AttackWindup)
                        {

                            PlayerInfo p = *playerPtr;
                            if (p.CurrentHealth > FP._0)
                            {
                                p.CurrentHealth -= info->Damage;
                                if (p.CurrentHealth < FP._0)
                                    p.CurrentHealth = FP._0;
                                f.Set(playerEnt, p);
                            }
                            info->IsAttacking = false;         
                            info->AttackCooldown = AttackInterval; 
                        }
                    }
                    else
                    {
                        // không trong cú chém, chỉ đang chờ cooldown
                        if (info->AttackCooldown > FP._0)
                        {
                            info->AttackCooldown -= f.DeltaTime;
                        }
                    }
                }
                // THẤY PLAYER NHƯNG CHƯA TỚI TẦM CHÉM
                else
                {
                    info->IsAttacking = false; // animator tắt Attack

                    // giảm cooldown nếu còn
                    if (info->AttackCooldown > FP._0)
                        info->AttackCooldown -= f.DeltaTime;

                    FPVector2 dir = (playerPos - enemyPos).Normalized;
                    body->Velocity = dir * FP._1_25;
                }

                return;
            }
            info->IsAttacking = false; 
            if (info->Time > info->ChangeDirectionTime)
            {
                info->ChangeDirectionTime =
                    info->Time + f.Global->RngSession.Next(FP._1, FP._2);

                info->Direction = new FPVector2(
                    f.Global->RngSession.Next(-FP._1, FP._1),
                    f.Global->RngSession.Next(-FP._1, FP._1)
                ).Normalized;
            }

            FP distToSpawn = FPVector2.Distance(enemyPos, info->SpawnPosition);
            if (distToSpawn > info->Radius)
            {
                info->Direction = (info->SpawnPosition - enemyPos).Normalized;
            }

            body->Velocity = info->Direction * FP._0_75;
        }
    }
}
