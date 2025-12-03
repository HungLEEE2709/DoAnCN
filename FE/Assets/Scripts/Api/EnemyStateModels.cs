using System.Collections.Generic;

[System.Serializable]
public class MapStatePayload
{
    public string idUser;
    public PlayerPositionData PlayerPosition;
    public List<EnemyStateData> Enemies;
    public int Vang;
    public int TiemNang;
    public int SucManh;
    public int MaxHp;
    public int MaxKi;
    public int Dame;
}

[System.Serializable]
public class MapStateResponse
{
    public bool success;
    public PlayerPositionData PlayerPosition;
    public List<EnemyStateData> Enemies;
    public int Vang;
    public int TiemNang;
    public int SucManh;
    public int MaxHp;
    public int MaxKi;
    public int Dame;
}

[System.Serializable]
public class PlayerPositionData
{
    public float x;
    public float y;
}

[System.Serializable]
public class EnemyStateData
{
    public int id;
    public float x;
    public float y;
    public float hp;
    public bool isDead;
}
