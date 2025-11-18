using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AuthPayload
{
    public string username;
    public string password;
}

[System.Serializable]
public class LoginResponse
{
    public string message;
    public string token;
    public UserData user;
}

[System.Serializable]
public class UserData
{
    public int idUser;
    public string UserName;
}

[System.Serializable]
public class PlayerInfoData
{
    public int idUser;
    public string UserName;
    public string Planet;
    public string CharacterName;
    public bool CharacterChosen;
}

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    public string baseUrl = "http://localhost:5000/api/auth";
    public string playerUrl = "http://localhost:5000/api/player";

    public string StartUI = "StartUI";
    public string RegisterUI = "RegisterUI";
    public string CharacterSelect = "CharacterSelect";
    public string GameScene = "GameScene";

    void GoToStartUI() => SceneManager.LoadScene(StartUI);
    public void OnRegister() => SceneManager.LoadScene(RegisterUI);
    public void OnLogin() => StartCoroutine(LoginCoroutine());

    IEnumerator LoginCoroutine()
    {
        AuthPayload data = new AuthPayload
        {
            username = usernameInput.text,
            password = passwordInput.text
        };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = new UnityWebRequest(baseUrl + "/login", "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string res = req.downloadHandler.text;

                // Parse login response
                LoginResponse loginData = JsonUtility.FromJson<LoginResponse>(res);
                if (loginData == null || loginData.user == null)
                {
                    statusText.text = "❌ Dữ liệu trả về không hợp lệ!";
                    yield break;
                }

                string token = loginData.token;

                // Lưu token & idUser
                PlayerPrefs.SetString("jwt_token", token);
                PlayerPrefs.SetInt("idUser", loginData.user.idUser);
                PlayerPrefs.SetString("UserName", loginData.user.UserName);
                PlayerPrefs.Save();

                // ⬇ Thêm phần GET PlayerInfo tại đây
                yield return StartCoroutine(GetPlayerInfo(loginData.user.idUser));
            }
            else statusText.text = "Lỗi Đăng nhập: " + req.downloadHandler.text;
        }
    }

    IEnumerator GetPlayerInfo(int idUser)
    {
        using (UnityWebRequest req = UnityWebRequest.Get($"{playerUrl}/{idUser}"))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                string json = req.downloadHandler.text;
                PlayerInfoData info = JsonUtility.FromJson<PlayerInfoData>(json);

                if (info == null)
                {
                    statusText.text = "❌ Không thể đọc dữ liệu nhân vật!";
                    yield break;
                }

                // Lưu vào PlayerPrefs
                PlayerPrefs.SetString($"CharacterName_{idUser}", info.CharacterName);
                PlayerPrefs.SetString($"Planet_{idUser}", info.Planet);
                PlayerPrefs.SetInt("CharacterChosen", info.CharacterChosen ? 1 : 0);
                PlayerPrefs.Save();

                if (info.CharacterChosen)
                {
                    SceneManager.LoadScene(GameScene); 
                }
                else
                {
                    SceneManager.LoadScene(CharacterSelect); 
                }
            }
            else
            {
                statusText.text = "Lỗi tải PlayerInfo: " + req.error;
            }
        }
    }

    string ExtractToken(string json)
    {
        int idx = json.IndexOf("\"token\":\"");
        if (idx == -1) return null;
        int start = idx + 9;
        int end = json.IndexOf("\"", start);
        if (end == -1) return null;
        return json.Substring(start, end - start);
    }
}
