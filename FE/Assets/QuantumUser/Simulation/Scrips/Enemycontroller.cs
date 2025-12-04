using Photon.Deterministic;
using UnityEngine.Scripting;
using UnityEngine;

namespace Quantum
{
    [Preserve]
    public unsafe class EnemyController : SystemMainThreadFilter<EnemyController.Filter>
    {

        public struct Filter
        {
            public EntityRef Entity;
            public PhysicsBody2D* Body;
            public Transform2D* Transform;
            public EnemyInfo* Info;
        }

        // Thời gian animation attack (phải đủ để animation chạy hết)
        // Tăng lên để animation Goblin_Attack chạy đủ trước khi gây damage
        private static readonly FP AttackWindup = FP.FromFloat_UNSAFE(1.2f);
        
        // Thời gian cooldown giữa các cú chém (tránh spam attack)
        // Phải đủ lớn để animation kết thúc và có khoảng nghỉ
        private static readonly FP AttackInterval = FP.FromFloat_UNSAFE(1.5f);

        public override void Update(Frame f, ref Filter filter)
        {

            EnemyInfo* info = filter.Info;

            // ========== DEATH ==========
            if (info->CurrentHealth <= FP._0 && !info->IsDead)
            {
                info->CurrentHealth = FP._0;
                info->IsDead = true;
                info->IsAttacking = false;
                
                // ========== ITEM DROP ==========
                // Kiểm tra có Prototype và ID hợp lệ không
                if (info->DropItemPrototype.Id.IsValid && info->DropItemId > 0 && info->DropQuantity > 0)
                {
                    // Random drop chance (0-100)
                    int randomChance = f.Global->RngSession.Next(0, 100);
                    int dropChance = info->DropChance > 0 ? info->DropChance : 100; // Default 100% if not set
                    
                    if (randomChance < dropChance)
                    {
                        SpawnDroppedItem(f, info->DropItemPrototype, info->DropItemId, info->DropQuantity, filter.Transform->Position);
                    }
                    else
                    {
                        Log.Debug($"[EnemyDrop] No drop (rolled {randomChance} vs {dropChance}%)");
                    }
                }
            }

            if (info->IsDead)
            {
                info->IsAttacking = false;
                filter.Body->Velocity = FPVector2.Zero;
                return;
            }

            // ========== GET PLAYER ==========
            EntityRef playerEnt = info->PlayerEntity;
            if (!f.Exists(playerEnt))
                return;

            PlayerInfo* playerPtr = f.Unsafe.GetPointer<PlayerInfo>(playerEnt);
            Transform2D* playerTr = f.Unsafe.GetPointer<Transform2D>(playerEnt);

            FPVector2 enemyPos = filter.Transform->Position;
            FPVector2 playerPos = playerTr->Position;

            FP dist = FPVector2.Distance(enemyPos, playerPos);
            info->Time += f.DeltaTime;

            // ========== PLAYER TRONG TẦM PHÁT HIỆN ==========
            if (dist < info->DetectionRange)
            {

                // ======================
                // TRONG TẦM CHÉM
                // ======================
                if (dist < info->AttackRange)
                {

                    // không chạy nữa, đứng chém
                    filter.Body->Velocity = FPVector2.Zero;

                    // nếu chưa bắt đầu chém và đã hết cooldown -> bắt đầu cú chém mới
                    if (!info->IsAttacking && info->AttackCooldown <= FP._0)
                    {
                        info->IsAttacking = true;   // bật animator Attack
                        info->AttackCooldown = FP._0; // dùng như timer đếm thời gian vung chém
                    }

                    if (info->IsAttacking)
                    {
                        // đang trong 1 cú chém: tăng timer
                        info->AttackCooldown += f.DeltaTime;

                        // ĐỦ THỜI GIAN VUNG (Animator đã chém tới) -> TRỪ MÁU MỘT LẦN
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

                            // kết thúc cú chém hiện tại
                            info->IsAttacking = false;         // animator sẽ tắt Attack
                            info->AttackCooldown = AttackInterval; // chờ trước khi được phép chém lại
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
                // ======================
                // THẤY PLAYER NHƯNG CHƯA TỚI TẦM CHÉM
                // ======================
                else
                {
                    info->IsAttacking = false; // animator tắt Attack

                    // giảm cooldown nếu còn
                    if (info->AttackCooldown > FP._0)
                        info->AttackCooldown -= f.DeltaTime;

                    FPVector2 dir = (playerPos - enemyPos).Normalized;
                    filter.Body->Velocity = dir * FP._1_25;
                }

                return;
            }

            // ========== PATROL (KHÔNG ATTACK) ==========
            info->IsAttacking = false; // rất quan trọng

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

            filter.Body->Velocity = info->Direction * FP._0_75;
        }
        
        private void SpawnDroppedItem(Frame f, AssetRef<EntityPrototype> protoRef, int itemId, int quantity, FPVector2 position)
        {
            var proto = f.FindAsset(protoRef);
            if (proto == null)
            {
                Log.Error($"[EnemyDrop] ❌ FAILED to find asset for prototype ref: {protoRef}");
                return;
            }

            var itemEntity = f.Create(proto);
            
            if (itemEntity == EntityRef.None)
            {
                Log.Error($"[EnemyDrop] ❌ f.Create(proto) returned EntityRef.None! Prototype might be invalid.");
                return;
            }
            else
            {
                Log.Info($"[EnemyDrop] ✅ Entity CREATED! ID: {itemEntity}. Checking components...");
                
                // Check Transform
                if (f.Unsafe.TryGetPointer<Transform2D>(itemEntity, out var transformCheck))
                {
                     Log.Info($"[EnemyDrop] - Has Transform2D. Pos: {transformCheck->Position}");
                }
                else
                {
                     Log.Error($"[EnemyDrop] - MISSING Transform2D component!");
                }
                
                // Check ItemInfo
                if (f.Has<ItemInfo>(itemEntity))
                {
                     Log.Info($"[EnemyDrop] - Has ItemInfo component.");
                }
                else
                {
                     Log.Error($"[EnemyDrop] - MISSING ItemInfo component!");
                }
            }
            
            // Set Position
            if (f.Unsafe.TryGetPointer<Transform2D>(itemEntity, out var t))
            {
                t->Position = position;
                Log.Info($"[EnemyDrop] ✅ Set Position to {position}");
            }

            // Set Item Info
            if (f.Unsafe.TryGetPointer<ItemInfo>(itemEntity, out var itemInfo))
            {
                itemInfo->ItemId = itemId;
                itemInfo->Quantity = quantity;
                itemInfo->Collected = false;
                Log.Info($"[EnemyDrop] ✅ Set ItemInfo: ID={itemId}, Qty={quantity}");
            }
            
            Log.Info($"[EnemyDrop] 🎁 SUCCESSFULLY Spawned Item {itemId} x{quantity} at {position}");
        }
    }
}
