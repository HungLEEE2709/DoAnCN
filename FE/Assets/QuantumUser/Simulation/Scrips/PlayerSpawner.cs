namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public unsafe class PlayerSpawner : SystemMainThread, ISignalOnPlayerAdded, ISignalOnPlayerRemoved
    {
        public override void OnInit(Frame f)
        {
            Debug.Log(">>> [PlayerSpawner] System Initialized! <<<");
        }

        public override void Update(Frame f) { }

        public void OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime)
        {
            string key = PlayerPrefs.GetString("PrefabKey", "ryu"); // Default to ryu
            Debug.Log($"[PlayerSpawner] OnPlayerAdded. PlayerRef: {player}, PrefabKey: '{key}'");

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("PrefabKey vẫn trống sau khi default! Force set 'ryu'");
                key = "ryu";
            }

            EntityPrototype proto = null;
            string loadPath = "";

            switch (key)
            {
                case "RYU DAIKI":
                case "ryu":
                    loadPath = "Entities/Character/RYU DAIKIEntityPrototype";
                    break;

                case "LUNA BLADE":
                case "luna":
                    loadPath = "Entities/Character/BLADEEntityPrototype";
                    break;

                case "GRIMJAW":
                case "grim":
                    loadPath = "Entities/Character/GRIMJAWEntityPrototype";
                    break;

                case "ZIKK FANG":
                case "zikk":
                    loadPath = "Entities/Character/ZIKK FANGEntityPrototype";
                    break;

                case "ELDRIA":
                case "eldria":
                    loadPath = "Entities/Character/ELDRIAEntityPrototype";
                    break;

                case "MOROK":
                case "morok":
                    loadPath = "Entities/Character/MOROKEntityPrototype";
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
                
                // Initialize Stats from PlayerPrefs
                int currentHp = PlayerPrefs.GetInt("CurrentHp", 100);
                int maxHp = PlayerPrefs.GetInt("MaxHp", 100);
                int currentKi = PlayerPrefs.GetInt("CurrentKi", 50);
                int maxKi = PlayerPrefs.GetInt("MaxKi", 50);

                playerInfo.CurrentHealth = FP.FromFloat_UNSAFE(currentHp);
                playerInfo.MaxHealth = FP.FromFloat_UNSAFE(maxHp);
                playerInfo.Ki = FP.FromFloat_UNSAFE(currentKi);
                playerInfo.MaxKi = FP.FromFloat_UNSAFE(maxKi);
                
                // Initialize Damage
                int damage = PlayerPrefs.GetInt("Dame", 10); 
                playerInfo.Damage = FP.FromFloat_UNSAFE(damage);
                
                // Initialize Gold & Potential & Power
                playerInfo.Vang = PlayerPrefs.GetInt("Vang", 0);
                playerInfo.TiemNang = PlayerPrefs.GetInt("TiemNang", 0);
                playerInfo.SucManh = PlayerPrefs.GetInt("SucManh", 0);
                
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
