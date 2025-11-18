using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement; 

[System.Serializable]
public class RegisterPayload
{
    public string username;
    public string email;
    public string password;
}

public class RegisterManager : MonoBehaviour
{
    
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;     
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    // Endpoint Đăng ký
    public string registerUrl = "http://localhost:5000/api/users/register";

    // Tên Scene Login để quay lại
    public string StartUi = "StartUi"; 

    public void OnConfirmRegister()
    {
        StartCoroutine(RegisterCoroutine());
    }

    public void OnBackToStart()
    {
        SceneManager.LoadScene(StartUi);
    }


    // --- CHỨC NĂNG ĐĂNG KÝ (Register) ---
    IEnumerator RegisterCoroutine()
    {
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(emailInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            statusText.text = "Vui lòng điền đầy đủ thông tin.";
            yield break; 
        }

        // 2. Chuẩn bị dữ liệu
        RegisterPayload data = new RegisterPayload
        {
            username = usernameInput.text,
            email = emailInput.text,
            password = passwordInput.text
        };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest req = new UnityWebRequest(registerUrl, "POST"))
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            // 3. Xử lý kết quả
            if (req.result == UnityWebRequest.Result.Success)
            {
                statusText.text = "Đăng ký thành công ✅. Đang chuyển về màn hình chính...";
                // Tùy chọn: Tự động chuyển về màn hình Login sau 2 giây
                yield return new WaitForSeconds(2);
                SceneManager.LoadScene(StartUi);
            }
            else
            {
                statusText.text = "Lỗi Đăng ký: " + req.downloadHandler.text;
            }
        }
    }
}