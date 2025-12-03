namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public unsafe class PlayerSpawner : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
    {
        public void OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime)
        {
            string key = PlayerPrefs.GetString("PrefabKey", "");
            Debug.Log("PrefabKey từ PlayerPrefs: " + key);

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("PrefabKey trống!");
                return;
            }

            EntityPrototype proto = null;
            string loadPath = "";

            switch (key)
            {
                case "RYU DAIKI":
                case "ryu":
                    loadPath = "Entities/RYU DAIKIEntityPrototype";
                    break;

                case "LUNA BLADE":
                case "luna":
                    loadPath = "Entities/LUNA BLADEEntityPrototype";
                    break;

                case "GRIMJAW":
                case "grim":
                    loadPath = "Entities/GRIMJAWEntityPrototype";
                    break;

                case "ZIKK FANG":
                case "zikk":
                    loadPath = "Entities/ZIKK FANGEntityPrototype";
                    break;

                case "ELDRIA":
                case "eldria":
                    loadPath = "Entities/ELDRIAEntityPrototype";
                    break;

                case "MOROK":
                case "morok":
                    loadPath = "Entities/MOROKEntityPrototype";
                    break;

                default:
                    Debug.LogError("Không tìm thấy prototype cho key: " + key);
                    return;
            }

            proto = Resources.Load<EntityPrototype>(loadPath);

            if (proto == null)
            {
                Debug.LogError("Không load được prototype từ path: " + loadPath);
                return;
            }

            var spawnedPlayer = frame.Create(proto);

            if (!spawnedPlayer.IsValid)
            {
                Debug.LogError("Spawn thất bại cho: " + key);
                return;
            }

            // ⭐⭐⭐ THÊM ĐOẠN NÀY ⭐⭐⭐
            // GÁN PlayerEntity cho tất cả enemy trong map
            var enemies = frame.GetComponentIterator<EnemyInfo>();
            foreach (var it in enemies)
            {
                var eInfo = it.Component;
                eInfo.PlayerEntity = spawnedPlayer;
                frame.Set(it.Entity, eInfo);
            }
            // ⭐⭐⭐ KẾT THÚC ĐOẠN THÊM ⭐⭐⭐

            // Đặt vị trí spawn lệch theo PlayerRef
            if (frame.Has<Transform2D>(spawnedPlayer))
            {
                var t = frame.Get<Transform2D>(spawnedPlayer);

                int index = (int)player - 1;
                FP x = (FP)(index * 2);
                t.Position = new FPVector2(x, FP._0);

                frame.Set(spawnedPlayer, t);
            }

            if (frame.Has<PlayerInfo>(spawnedPlayer))
            {
                var playerInfo = frame.Get<PlayerInfo>(spawnedPlayer);
                playerInfo.PlayerRef = player;
                frame.Set(spawnedPlayer, playerInfo);
            }
            else
            {
                Debug.LogWarning("Prototype chưa có PlayerInfo component!");
            }

            Debug.Log($"Spawn thành công nhân vật: {key} cho PlayerRef {player}");
        }

        public void OnPlayerRemoved(Frame frame, PlayerRef player)
        {
            var players = frame.GetComponentIterator<PlayerInfo>();
            foreach (var item in players)
            {
                if (item.Component.PlayerRef == player)
                {
                    frame.Destroy(item.Entity);
                    Debug.Log("Destroy entity của player: " + player);
                }
            }
        }
    }
}
