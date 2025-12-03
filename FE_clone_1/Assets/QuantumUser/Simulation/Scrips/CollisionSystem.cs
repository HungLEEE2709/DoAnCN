using Photon.Deterministic;

namespace Quantum
{
    public unsafe class CollisionSystem :
        SystemSignalsOnly,
        ISignalOnCollisionEnter2D
    {

        public void OnCollisionEnter2D(Frame f, CollisionInfo2D info)
        {
            EntityRef a = info.Entity;
            EntityRef b = info.Other;

            // ======================
            // PLAYER → ENEMY (Attack)
            // ======================
            if (f.TryGet<PlayerInfo>(a, out var playerA) &&
                f.TryGet<EnemyInfo>(b, out var enemyB))
            {
                HandlePlayerAttack(f, a, ref playerA, b, ref enemyB);
            }

            if (f.TryGet<PlayerInfo>(b, out var playerB) &&
                f.TryGet<EnemyInfo>(a, out var enemyA))
            {
                HandlePlayerAttack(f, b, ref playerB, a, ref enemyA);
            }
        }

        private void HandlePlayerAttack(Frame f,
            EntityRef playerEnt, ref PlayerInfo player,
            EntityRef enemyEnt, ref EnemyInfo enemy)
        {

            if (!player.IsAttacking)
                return;

            if (enemy.IsDead)
                return;

            enemy.CurrentHealth -= player.Damage;

            if (enemy.CurrentHealth <= FP._0)
            {
                enemy.CurrentHealth = FP._0;
                enemy.IsDead = true;
                enemy.IsAttacking = false;
            }

            f.Set(enemyEnt, enemy);
        }
    }
}
