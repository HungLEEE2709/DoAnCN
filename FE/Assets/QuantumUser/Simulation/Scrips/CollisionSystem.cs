using Photon.Deterministic;

namespace Quantum
{
    public unsafe class CollisionSystem :
        SystemSignalsOnly,
        ISignalOnCollisionEnter2D,
        ISignalOnTriggerEnter2D
    {
        public void OnTriggerEnter2D(Frame f, TriggerInfo2D info)
        {
            Log.Info($"[Quantum] OnTriggerEnter2D detected between {info.Entity} and {info.Other}");

            EntityRef a = info.Entity;
            EntityRef b = info.Other;

            // Player → Item
            if (f.TryGet<PlayerInfo>(a, out var playerA) &&
                f.TryGet<ItemInfo>(b, out var itemB))
            {
                HandlePickup(f, a, ref playerA, b, ref itemB);
            }

            // Item → Player
            if (f.TryGet<ItemInfo>(a, out var itemA) &&
                f.TryGet<PlayerInfo>(b, out var playerB))
            {
                HandlePickup(f, b, ref playerB, a, ref itemA);
            }
        }

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

        private void HandlePickup(Frame f, EntityRef playerEnt, ref PlayerInfo player, EntityRef itemEnt, ref ItemInfo item)
        {
            if (item.Collected) return;

            Log.Info($"[Quantum] HandlePickup called! ItemID: {item.ItemId}, Qty: {item.Quantity}");

            item.Collected = true;
            f.Set(itemEnt, item);

            // Gửi event về Unity
            f.Events.ItemPickedUp(player.PlayerRef, item.ItemId, item.Quantity);
            Log.Info($"[Quantum] Event ItemPickedUp SENT for Player: {player.PlayerRef}");

            f.Destroy(itemEnt);   // QUANTUM xoá entity
        }
    }
}
