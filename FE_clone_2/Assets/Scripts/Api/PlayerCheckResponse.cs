using System;

[Serializable]
public class PlayerInfoData
{
    public string _id;
    public string idUser;
    public string UserName;
    public int SucManh;
    public int Hp;
    public int Ki;
    public int Dame;
    public string Planet;
    public string CharacterName;
    public bool CharacterChosen;
    public string PrefabKey;
}

[Serializable]
public class PlayerCheckResponse
{
    public bool success;
    public bool created;
    public PlayerInfoData player; 
}
