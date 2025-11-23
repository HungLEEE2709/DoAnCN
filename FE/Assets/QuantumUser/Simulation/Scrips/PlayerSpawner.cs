namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public unsafe class PlayerSpawer : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
    {
        public void OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime)
        {
            // 1) Lấy key nhân vật FE đã lưu
            string key = PlayerPrefs.GetString("PrefabKey", "Ryu");

            // 2) Chọn prototype tương ứng
            EntityPrototype proto = null;

            switch (key)
            {
                case "Grim":
                    proto = frame.FindAsset<EntityPrototype>("GrimEntityPrototype");
                    break;

                case "Ryu":
                default:
                    proto = frame.FindAsset<EntityPrototype>("RyuEntityPrototype");
                    break;
            }


            var spawnedPlayer = frame.Create(proto);


            var playerInfo = frame.Get<PlayerInfo>(spawnedPlayer);
            playerInfo.PlayerRef = player;
            frame.Set(spawnedPlayer, playerInfo);
        }

        public void OnPlayerRemoved(Frame frame, PlayerRef player)
        {
            var players = frame.GetComponentIterator<PlayerInfo>();
            foreach (var item in players)
            {
                if (item.Component.PlayerRef == player)
                {
                    frame.Destroy(item.Entity);
                }
            }
        }
    }
}
