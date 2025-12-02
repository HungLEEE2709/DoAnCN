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
            public PhysicsBody2D* Body;
            public Transform2D* Transform;
            public EnemyInfo* Info;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            EnemyInfo* info = filter.Info;

            // Nếu HP <= 0 mà chưa set IsDead → set luôn
            if (info->CurrentHealth <= FP._0 && !info->IsDead)
            {
                info->CurrentHealth = FP._0;
                info->IsDead = true;
                info->IsAttacking = false;
            }

            // Nếu chết thì đứng im, không xử lý AI nữa
            if (info->IsDead)
            {
                filter.Body->Velocity = FPVector2.Zero;
                info->IsAttacking = false;
                return;
            }

            // ----------------------------
            // LẤY PLAYER TỪ EnemyInfo.PlayerEntity
            // (trong inspector bạn đã gán Player Entity rồi)
            // ----------------------------
            EntityRef playerEnt = info->PlayerEntity;
            if (!f.Exists(playerEnt))
                return;

            PlayerInfo* playerPtr = f.Unsafe.GetPointer<PlayerInfo>(playerEnt);
            Transform2D* playerTr = f.Unsafe.GetPointer<Transform2D>(playerEnt);

            // ----------------------------
            // TỌA ĐỘ
            // ----------------------------
            FPVector2 enemyPos = filter.Transform->Position;
            FPVector2 playerPos = playerTr->Position;

            FP dist = FPVector2.Distance(enemyPos, playerPos);

            // ----------------------------
            // UPDATE TIMER (cho patrol & cooldown)
            // ----------------------------
            info->Time += f.DeltaTime;

            // ----------------------------
            // 1) DETECTION → CHASE OR ATTACK
            // ----------------------------
            if (dist < info->DetectionRange)
            {
                // ===== ATTACK =====
                if (dist < info->AttackRange)
                {
                    info->IsAttacking = true;
                    filter.Body->Velocity = FPVector2.Zero;

                    // Attack cooldown 0.25s
                    if (info->Time > info->ChangeDirectionTime)
                    {
                        info->ChangeDirectionTime = info->Time + FP._0_25;

                        // QUAN TRỌNG: copy struct, chỉnh, rồi Set lại
                        PlayerInfo player = *playerPtr;

                        player.CurrentHealth -= info->Damage;
                        if (player.CurrentHealth < FP._0)
                            player.CurrentHealth = FP._0;

                        f.Set(playerEnt, player);   // ghi ngược về frame (UI/BE mới thấy)
                    }
                }
                else
                {
                    // ===== CHASE =====
                    info->IsAttacking = false;

                    FPVector2 dir = (playerPos - enemyPos).Normalized;
                    filter.Body->Velocity = dir * FP._1_25;
                }

                return;
            }

            // ----------------------------
            // 2) PATROL (KHÔNG THẤY PLAYER)
            // ----------------------------

            // Đổi hướng random
            if (info->Time > info->ChangeDirectionTime)
            {
                info->ChangeDirectionTime = info->Time + f.Global->RngSession.Next(FP._1, FP._2);

                info->Direction = new FPVector2(
                    f.Global->RngSession.Next(-FP._1, FP._1),
                    f.Global->RngSession.Next(-FP._1, FP._1)
                ).Normalized;
            }

            // Giữ trong bán kính
            FP distToSpawn = FPVector2.Distance(enemyPos, info->SpawnPosition);
            if (distToSpawn > info->Radius)
            {
                info->Direction = (info->SpawnPosition - enemyPos).Normalized;
            }

            // Move patrol
            info->IsAttacking = false;
            filter.Body->Velocity = info->Direction * FP._0_75;
        }
    }
}
