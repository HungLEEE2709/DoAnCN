namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public unsafe class PlayerSpawner : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
    {
        private static readonly FPVector3 SpawnPosition = new FPVector3(0, 0, 0);

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

            // Lấy đúng đường dẫn .qprototype trong Resources
            switch (key)
            {
                case "RYU DAIKI": loadPath = "Entities/RYU DAIKIEntityPrototype"; break;
                case "LUNA BLADE": loadPath = "Entities/LUNA BLADEEntityPrototype"; break;
                case "GRIMJAW": loadPath = "Entities/GRIMJAWEntityPrototype"; break;
                case "ZIKK FANG": loadPath = "Entities/ZIKK FANGEntityPrototype"; break;
                case "ELDRIA": loadPath = "Entities/ELDRIAEntityPrototype"; break;
                case "MOROK": loadPath = "Entities/MOROKEntityPrototype"; break;

                default:
                    Debug.LogError("Không tìm thấy prototype cho key: " + key);
                    return;
            }

            // Load prototype qua Unity Resources
            proto = Resources.Load<EntityPrototype>(loadPath);

            if (proto == null)
            {
                Debug.LogError("Không load được prototype từ path: " + loadPath);
                return;
            }

            // Spawn entity trong Simulation
            var spawnedPlayer = frame.Create(proto);

            if (!spawnedPlayer.IsValid)
            {
                Debug.LogError("Spawn thất bại cho: " + key);
                return;
            }

            // Set position
            if (frame.Has<Transform3D>(spawnedPlayer))
            {
                var transform = frame.Get<Transform3D>(spawnedPlayer);
                transform.Position = SpawnPosition;
                frame.Set(spawnedPlayer, transform);
            }

            // Ánh xạ playerRef
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

            Debug.Log("Spawn thành công nhân vật: " + key);
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
