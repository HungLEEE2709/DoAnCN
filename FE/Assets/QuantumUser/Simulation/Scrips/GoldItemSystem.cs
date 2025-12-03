using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class GoldItemSystem : SystemSignalsOnly, ISignalOnTriggerEnter2D
    {
        public void OnTriggerEnter2D(Frame f, TriggerInfo2D info)
        {
            // Check if one entity is Player and other is Item
            EntityRef playerEnt = EntityRef.None;
            EntityRef itemEnt = EntityRef.None;

            if (f.Has<PlayerInfo>(info.Entity)) playerEnt = info.Entity;
            else if (f.Has<PlayerInfo>(info.Other)) playerEnt = info.Other;

            if (f.Has<ItemInfo>(info.Entity)) itemEnt = info.Entity;
            else if (f.Has<ItemInfo>(info.Other)) itemEnt = info.Other;

            if (playerEnt != EntityRef.None && itemEnt != EntityRef.None)
            {
                var player = f.Get<PlayerInfo>(playerEnt);
                var item = f.Get<ItemInfo>(itemEnt);

                // Check if Item is Gold (ID = 0)
                if (item.ItemId == 0)
                {
                    // Add Gold
                    player.Vang += item.Quantity;
                    f.Set(playerEnt, player);

                    // Log
                    UnityEngine.Debug.Log($"[GoldItemSystem] Player picked up {item.Quantity} Gold! Total: {player.Vang}");

                    // Destroy Item
                    f.Destroy(itemEnt);
                }
            }
        }
    }
}
