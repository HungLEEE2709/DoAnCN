using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI potentialText;
    public TextMeshProUGUI strengthText;
    public Button addStrengthButton;

    [Header("API Config")]
    [System.NonSerialized] public string apiGetPlayer = GameConfig.BaseUrl + "/api/playerInfo/chosen/";
    [System.NonSerialized] public string apiAddPotential = GameConfig.BaseUrl + "/api/playerInfo/add-potential";

    private string userId;
    private int currentPotential;
    private int currentStrength;

    void Start()
    {
        userId = PlayerPrefs.GetString("idUser", "");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID not found!");
            return;
        }

        addStrengthButton.onClick.AddListener(OnAddStrengthClick);
        
        // Load initial data
        StartCoroutine(FetchPlayerData());
    }

    IEnumerator FetchPlayerData()
    {
        string url = apiGetPlayer + userId;
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching player data: " + req.error);
            yield break;
        }

        PlayerInfoResponse response = JsonUtility.FromJson<PlayerInfoResponse>(req.downloadHandler.text);
        if (response != null && response.success && response.player != null)
        {
            UpdateUI(response.player);
        }
    }

    void UpdateUI(PlayerData player)
    {
        currentPotential = player.TiemNang;
        currentStrength = player.SucManh;

        if (potentialText != null) potentialText.text = "Tiềm Năng: " + currentPotential;
        if (strengthText != null) strengthText.text = "Sức Mạnh: " + currentStrength;

        // Disable button if no potential points
        if (addStrengthButton != null)
        {
            addStrengthButton.interactable = currentPotential > 0;
        }
    }

    void OnAddStrengthClick()
    {
        if (currentPotential <= 0) return;
        StartCoroutine(AddPotentialRoutine(1));
    }

    IEnumerator AddPotentialRoutine(int amount)
    {
        // Disable button while processing
        addStrengthButton.interactable = false;

        AddPotentialPayload payload = new AddPotentialPayload { idUser = userId, amount = amount };
        string json = JsonUtility.ToJson(payload);

        UnityWebRequest req = UnityWebRequest.PostWwwForm(apiAddPotential, "");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error adding potential: " + req.downloadHandler.text);
            addStrengthButton.interactable = true; // Re-enable on error
            yield break;
        }

        // Parse response to update UI with new values
        // The API returns { success: true, player: { ... } }
        PlayerInfoResponse response = JsonUtility.FromJson<PlayerInfoResponse>(req.downloadHandler.text);
        if (response != null && response.success && response.player != null)
        {
            UpdateUI(response.player);
        }
        else
        {
            addStrengthButton.interactable = true;
        }
    }

    [System.Serializable]
    private class AddPotentialPayload
    {
        public string idUser;
        public int amount;
    }
}
