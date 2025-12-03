[System.Serializable]
public class PlayerStatsData
{
    public string idUser;
    public string UserName;
    public int SucManh;
    public int TiemNang;
    public int Hp;
    public int Ki;
    public int Dame;
    public string CharacterName;
    public string PrefabKey;
}

[System.Serializable]
public class PlayerResponse
{
    public bool success;
    public PlayerStatsData player;
}
