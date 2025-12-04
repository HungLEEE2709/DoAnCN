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
    }
}
