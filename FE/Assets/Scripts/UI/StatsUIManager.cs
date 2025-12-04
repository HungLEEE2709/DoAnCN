using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Input = UnityEngine.Input;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using Quantum;

public class StatsUIManager : MonoBehaviour
{
    [Header("Top Info")]
    public TextMeshProUGUI sucManhText;
    public TextMeshProUGUI tiemNangText;

    [Header("HP Row")]
    public Button hpRowBtn;       // The main button/row for HP
    public Button hpAddBtn;       // The + button for HP
    public TextMeshProUGUI hpAddBtnText; // Text inside the + button
    public TextMeshProUGUI hpValueText;

    [Header("KI Row")]
    public Button kiRowBtn;       // The main button/row for KI
    public Button kiAddBtn;       // The + button for KI
    public TextMeshProUGUI kiAddBtnText; // Text inside the + button
    public TextMeshProUGUI kiValueText;

    [Header("SD Row")]
    public Button sdRowBtn;       // The main button/row for SD
    public Button sdAddBtn;       // The + button for SD
    public TextMeshProUGUI sdAddBtnText; // Text inside the + button
    public TextMeshProUGUI sdValueText;

    [Header("API Config")]
    [System.NonSerialized] public string apiGetPlayer = GameConfig.BaseUrl + "/api/playerInfo/chosen/";
    [System.NonSerialized] public string apiAddPotential = GameConfig.BaseUrl + "/api/playerInfo/add-potential";

    private string userId;
    private int currentPotential;

    [Header("Main Panel")]
    public GameObject statsPanel; // Assign the Panel object here

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (statsPanel != null)
            {
                bool isActive = statsPanel.activeSelf;
                statsPanel.SetActive(!isActive);
                
                // If opening, refresh data
                if (!isActive)
                {
                    StartCoroutine(FetchPlayerData());
                }
            }
        }

        // Poll Quantum State for Real-time Updates (SucManh, TiemNang)
        if (QuantumRunner.Default != null && QuantumRunner.Default.Game != null)
        {
            var frame = QuantumRunner.Default.Game.Frames.Verified;
            if (frame != null)
            {
                // Find local player (simplified: assume Player 0 or check all)
                // Better: Filter by PlayerRef if we knew it. 
                // For now, we iterate to find the one matching our User (requires syncing PlayerRef <-> UserID, but let's just show the first one for single player or check local)
                
                var players = frame.GetComponentIterator<Quantum.PlayerInfo>();
                foreach (var p in players)
                {
                    // Assuming single local player or we show stats of the first valid player
                    // In a real multiplayer match, we need to know which Entity belongs to "me".
                    // Usually PlayerRef 0 is local in single player.
                    
                    // Update UI Text
                    if (sucManhText) sucManhText.text = "Sức Mạnh: " + p.Component.SucManh.AsInt;
                    if (tiemNangText) tiemNangText.text = "Tiềm Năng: " + p.Component.TiemNang.AsInt;
                    break; // Just take the first one found
                }
            }
        }
    }

    void Start()
    {
        userId = PlayerPrefs.GetString("idUser", "");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID not found!");
            return;
        }

        // Setup Listeners
        if (hpRowBtn) hpRowBtn.onClick.AddListener(() => ToggleAddButton("hp"));
        if (kiRowBtn) kiRowBtn.onClick.AddListener(() => ToggleAddButton("ki"));
        if (sdRowBtn) sdRowBtn.onClick.AddListener(() => ToggleAddButton("sd"));

        if (hpAddBtn) hpAddBtn.onClick.AddListener(() => OnAddPoint("hp"));
        if (kiAddBtn) kiAddBtn.onClick.AddListener(() => OnAddPoint("ki"));
        if (sdAddBtn) sdAddBtn.onClick.AddListener(() => OnAddPoint("sd"));

        // Hide all add buttons initially
        HideAllAddButtons();

        // Load data initially
        StartCoroutine(FetchPlayerData());
        
        // Ensure panel is hidden on start (optional, or set in Inspector)
        if (statsPanel != null) statsPanel.SetActive(false);
    }

    void HideAllAddButtons()
    {
        if (hpAddBtn) hpAddBtn.gameObject.SetActive(false);
        if (kiAddBtn) kiAddBtn.gameObject.SetActive(false);
        if (sdAddBtn) sdAddBtn.gameObject.SetActive(false);
    }

    void ToggleAddButton(string type)
    {
        // If the user clicks the row, we show the corresponding + button
        // and hide others.
        HideAllAddButtons();

        switch (type)
        {
            case "hp":
                if (hpAddBtn) hpAddBtn.gameObject.SetActive(true);
                break;
            case "ki":
                if (kiAddBtn) kiAddBtn.gameObject.SetActive(true);
                break;
            case "sd":
                if (sdAddBtn) sdAddBtn.gameObject.SetActive(true);
                break;
        }
    }

    void OnAddPoint(string type)
    {
        if (currentPotential <= 0)
        {
            Debug.Log("Không đủ tiềm năng!");
            return;
        }
        StartCoroutine(AddPotentialRoutine(1, type));
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

    IEnumerator AddPotentialRoutine(int amount, string type)
    {
        AddPotentialPayload payload = new AddPotentialPayload 
        { 
            idUser = userId, 
            amount = amount,
            statType = type
        };
        string json = JsonUtility.ToJson(payload);

        UnityWebRequest req = UnityWebRequest.PostWwwForm(apiAddPotential, "");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error adding potential: " + req.downloadHandler.text);
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

        if (sucManhText) sucManhText.text = "Sức Mạnh: " + player.SucManh;
        if (tiemNangText) tiemNangText.text = "Tiềm Năng: " + player.TiemNang;

        if (hpValueText) hpValueText.text = player.MaxHp.ToString();
        if (kiValueText) kiValueText.text = player.MaxKi.ToString();
        if (sdValueText) sdValueText.text = player.Dame.ToString();

        // Calculate Costs
        int hpCost = Mathf.FloorToInt(player.MaxHp * 1.02f);
        int kiCost = Mathf.FloorToInt(player.MaxKi * 1.02f);
        int sdCost = Mathf.FloorToInt(player.Dame * 2.0f);

        // Update Button Texts
        if (hpAddBtnText) hpAddBtnText.text = "Cần " + hpCost;
        if (kiAddBtnText) kiAddBtnText.text = "Cần " + kiCost;
        if (sdAddBtnText) sdAddBtnText.text = "Cần " + sdCost;

        // Update PlayerUI (HealthBar) immediately
        PlayerUI playerUI = FindObjectOfType<PlayerUI>();
        if (playerUI != null)
        {
            playerUI.UpdateMaxStats(player.MaxHp, player.MaxKi);
        }
    }

    [System.Serializable]
    private class AddPotentialPayload
    {
        public string idUser;
        public int amount;
        public string statType;
    }
}
