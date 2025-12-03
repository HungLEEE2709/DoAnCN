[System.Serializable]
public class PlayerStatsData
{
    public string idUser;
    public string UserName;
    public int SucManh;
    public int Hp;
    public int MaxHp;
    public int Ki;
    public int MaxKi;
    public int Dame;
    public int Vang;
    public int TiemNang;
    public string CharacterName;
    public string PrefabKey;
}

[System.Serializable]
public class PlayerResponse
{
    public bool success;
    public PlayerStatsData player;
}
