using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public Text statusText;

    private static readonly HttpClient client = new HttpClient();
    private string apiBase = "http://localhost:5000/api/users"; 

    public async void OnLoginButton()
    {
        await HandleAuth("login");
    }

    public async void OnRegisterButton()
    {
        await HandleAuth("register");
    }

    private async Task HandleAuth(string endpoint)
    {
        var email = emailInput.text;
        var password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "⚠️ Nhập đủ email và mật khẩu!";
            return;
        }

        var json = $"{{\"email\":\"{email}\",\"password\":\"{password}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{apiBase}/{endpoint}", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                statusText.text = "✅ Thành công!";
                PlayerPrefs.SetString("authToken", result); 
                SceneManager.LoadScene("StartUI"); 
            }
            else
            {
                statusText.text = $"❌ Lỗi: {result}";
            }
        }
        catch (System.Exception ex)
        {
            statusText.text = $"🚫 Kết nối lỗi: {ex.Message}";
        }
    }
}
