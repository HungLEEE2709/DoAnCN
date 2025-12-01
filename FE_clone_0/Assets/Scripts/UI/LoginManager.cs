using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text;

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
    public string _id;
    public string username;
    public string email;
}

public class LoginManager : MonoBehaviour
{

    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    public string baseUrl = "http://localhost:5000/api/users";

    public string StartScene = "Start";
    public string RegisterUI = "RegisterUI";

    public void OnRegister() => SceneManager.LoadScene(RegisterUI);
    public void OnLogin() => StartCoroutine(LoginCoroutine());

    IEnumerator LoginCoroutine()
    {

        statusText.text = "Đang đăng nhập...";

        AuthPayload payload = new AuthPayload
        {
            username = usernameInput.text,
            password = passwordInput.text
        };

        string jsonBody = JsonUtility.ToJson(payload);

        using (UnityWebRequest req = new UnityWebRequest(baseUrl + "/login", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "Lỗi đăng nhập: " + req.error;
                yield break;
            }

            LoginResponse loginData = null;

            try
            {
                loginData = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
            }
            catch
            {
                statusText.text = "JSON backend sai định dạng!";
                yield break;
            }

            if (loginData == null || loginData.user == null)
            {
                statusText.text = "Không thể đọc dữ liệu user!";
                yield break;
            }

            // 🔥 XÓA SẠCH dữ liệu tài khoản cũ
            PlayerPrefs.DeleteAll();

            // 🔥 LƯU thông tin user mới
            PlayerPrefs.SetString("jwt_token", loginData.token);
            PlayerPrefs.SetString("idUser", loginData.user._id);   // <- đúng key
            PlayerPrefs.SetString("UserName", loginData.user.username);
            PlayerPrefs.Save();

            // 🔥 Chuyển tới Start UI
            SceneManager.LoadScene(StartScene);
        }
    }
}
