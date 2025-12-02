using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class StartUIManager : MonoBehaviour
{
    [Header("Buttons")]
    public UnityEngine.UI.Button continueButton;
    public UnityEngine.UI.Button newGameButton;
    public UnityEngine.UI.Button changeAccountButton;

    [Header("UI")]
    public TMP_Text accountText;

    [Header("Scene Names")]
    public string quantumGameScene = "QuantumGameScene";
    public string createPlayerScene = "CreatePlayer";
    public string loginUIScene = "LoginUI";

    [Header("API")]
    public string apiCheck = "http://localhost:5000/api/player-info/check/";

    private void Start()
    {
        continueButton.onClick.AddListener(OnContinueClick);
        newGameButton.onClick.AddListener(OnNewGameClick);
        changeAccountButton.onClick.AddListener(OnChangeAccountClick);

        string username = PlayerPrefs.GetString("UserName", "");

        if (string.IsNullOrEmpty(username))
            accountText.text = "Chưa đăng nhập";
        else
            accountText.text = "Chơi tài khoản: " + username;
    }

    private void OnContinueClick()
    {
        StartCoroutine(CheckCharacterRoutine());
    }

    IEnumerator CheckCharacterRoutine()
    {
        string userId = PlayerPrefs.GetString("idUser", "");

        // Nếu không có user → quay lại Login
        if (string.IsNullOrEmpty(userId))
        {
            SceneManager.LoadScene(loginUIScene);
            yield break;
        }

        UnityWebRequest req = UnityWebRequest.Get(apiCheck + userId);
        yield return req.SendWebRequest();

        Debug.Log("API RESPONSE = " + req.downloadHandler.text);

        if (req.result != UnityWebRequest.Result.Success)
        {
            SceneManager.LoadScene(createPlayerScene);
            yield break;
        }

        PlayerCheckResponse data =
            JsonUtility.FromJson<PlayerCheckResponse>(req.downloadHandler.text);

        // Nếu không có nhân vật → vào create player
        if (data == null || data.created == false || data.player == null)
        {
            SceneManager.LoadScene(createPlayerScene);
        }
        else
        {
            // Có nhân vật hợp lệ → vào game
            SceneManager.LoadScene(quantumGameScene);
        }
    }

    private void OnNewGameClick()
    {
        // start new → luôn tạo nhân vật mới
        SceneManager.LoadScene(createPlayerScene);
    }

    private void OnChangeAccountClick()
    {
        // Xóa hết dữ liệu user cũ
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(loginUIScene);
    }
}
