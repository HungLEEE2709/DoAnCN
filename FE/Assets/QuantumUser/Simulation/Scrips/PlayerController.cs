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
            public PhysicsBody2D* Body;
            public PlayerInfo* Info;
        }

        public override void Update(Frame frame, ref Filter f)
        {
            var input = frame.GetPlayerInput(f.Info->PlayerRef);

            // MOVE
            f.Body->Velocity = input->Direction * f.Info->Speed;

            // ATTACK START
            if (input->Attack.WasPressed)
            {
                // thời gian attack (20 frame ~ 1/3 giây nếu 60fps)
                f.Info->AttackTimer = 20;
                f.Info->IsAttacking = true;
                Log.Debug("PLAYER ATTACK START");

                // AoE Attack Logic
                Transform2D* transform = frame.Unsafe.GetPointer<Transform2D>(f.Entity);
                if (transform != null)
                {
                    FP radius = FP._2; // Attack Radius
                    // Create a circle shape
                    Shape2D shape = Shape2D.CreateCircle(radius);
                    
                    // Perform OverlapShape
                    var hits = frame.Physics2D.OverlapShape(transform->Position, FP._0, shape);
                    
                    for (int i = 0; i < hits.Count; i++)
                    {
                        var hitEntity = hits[i].Entity;
                        if (hitEntity == f.Entity) continue; // Skip self

                        if (frame.TryGet<EnemyInfo>(hitEntity, out var enemy))
                        {
                            if (enemy.IsDead) continue;

                            // Apply Damage
                            enemy.CurrentHealth -= f.Info->Damage;
                            Log.Debug($"AoE Hit Enemy! HP Left: {enemy.CurrentHealth}");

                            if (enemy.CurrentHealth <= FP._0)
                            {
                                enemy.CurrentHealth = FP._0;
                                enemy.IsDead = true;
                                enemy.IsAttacking = false;
                                Log.Debug("Enemy Died by AoE!");

                                // Reward
                                f.Info->SucManh += (enemy.RewardSucManh > 0 ? enemy.RewardSucManh : 10);
                                f.Info->TiemNang += (enemy.RewardTiemNang > 0 ? enemy.RewardTiemNang : 1);
                            }
                            
                            // Update Enemy Component
                            frame.Set(hitEntity, enemy);
                        }
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
