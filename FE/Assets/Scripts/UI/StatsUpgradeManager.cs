using System.Collections;
using UnityEngine;
using TMPro;
using Quantum;

public class StatsUpgradeManager : MonoBehaviour
{
    public static StatsUpgradeManager Instance;

    [Header("Prefabs")]
    public GameObject hpRowPrefab;
    public GameObject kiRowPrefab;
    public GameObject damageRowPrefab;

    [Header("UI References")]
    public Transform statsPanel;
    public TextMeshProUGUI tiemNangText;
    public TextMeshProUGUI sucManhText;

    private string userId;
    private int currentGold;
    private int currentTiemNang;
    private int currentSucManh;
    private int currentHP;
    private int currentKi;
    private int currentDamage;

    // Upgrade costs (will increase after each upgrade)
    private int hpUpgradeCost = 1000;
    private int kiUpgradeCost = 800;
    private int damageUpgradeCost = 1500;

    private StatRowUI hpRow;
    private StatRowUI kiRow;
    private StatRowUI damageRow;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        userId = PlayerPrefs.GetString("idUser", "");
        
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("No userId found!");
            return;
        }

        LoadPlayerStats();
    }

    void LoadPlayerStats()
    {
        // Load from PlayerPrefs (already loaded by PlayerDataLoader)
        currentHP = PlayerPrefs.GetInt("MaxHp", 100);
        currentKi = PlayerPrefs.GetInt("MaxKi", 50);
        currentDamage = PlayerPrefs.GetInt("Dame", 10);
        currentTiemNang = PlayerPrefs.GetInt("TiemNang", 0);
        currentSucManh = PlayerPrefs.GetInt("SucManh", 0);

        UpdateCurrencyDisplay();
        SpawnStatRows();
    }

    void SpawnStatRows()
    {
        // Spawn HP Row
        GameObject hpObj = Instantiate(hpRowPrefab, statsPanel);
        hpRow = hpObj.GetComponent<StatRowUI>();
        hpRow.Setup("HP", "HP gốc:", currentHP, hpUpgradeCost);

        // Spawn Ki Row
        GameObject kiObj = Instantiate(kiRowPrefab, statsPanel);
        kiRow = kiObj.GetComponent<StatRowUI>();
        kiRow.Setup("Ki", "Ki gốc:", currentKi, kiUpgradeCost);

        // Spawn Damage Row
        GameObject damageObj = Instantiate(damageRowPrefab, statsPanel);
        damageRow = damageObj.GetComponent<StatRowUI>();
        damageRow.Setup("Damage", "Sức đánh gốc:", currentDamage, damageUpgradeCost);
    }

    void UpdateCurrencyDisplay()
    {
        if (tiemNangText != null) tiemNangText.text = "TN: " + currentTiemNang.ToString("N0");
        if (sucManhText != null) sucManhText.text = "SM: " + currentSucManh.ToString("N0");
    }

    public void UpgradeStat(string statType, int cost)
    {
        // Check if player has enough TiemNang
        if (currentTiemNang < cost)
        {
            Debug.Log("Không đủ Tiềm Năng!");
            // TODO: Show notification to player
            return;
        }

        // Deduct cost
        currentTiemNang -= cost;

        // Upgrade stat
        switch (statType)
        {
            case "HP":
                currentHP += 20;
                hpUpgradeCost = Mathf.RoundToInt(hpUpgradeCost * 1.2f);
                hpRow.UpdateDisplay(currentHP, hpUpgradeCost);
                break;

            case "Ki":
                currentKi += 10;
                kiUpgradeCost = Mathf.RoundToInt(kiUpgradeCost * 1.2f);
                kiRow.UpdateDisplay(currentKi, kiUpgradeCost);
                break;

            case "Damage":
                currentDamage += 5;
                damageUpgradeCost = Mathf.RoundToInt(damageUpgradeCost * 1.2f);
                damageRow.UpdateDisplay(currentDamage, damageUpgradeCost);
                break;
        }

        UpdateCurrencyDisplay();
        
        // Update PlayerPrefs immediately
        PlayerPrefs.SetInt("MaxHp", currentHP);
        PlayerPrefs.SetInt("MaxKi", currentKi);
        PlayerPrefs.SetInt("Dame", currentDamage);
        PlayerPrefs.SetInt("TiemNang", currentTiemNang);
        PlayerPrefs.SetInt("SucManh", currentSucManh);
        PlayerPrefs.Save();
        
        // Update Quantum PlayerInfo (so MapStateSaver gets correct values)
        UpdateQuantumPlayerInfo();

        // Update PlayerUI immediately
        var playerUI = FindObjectOfType<PlayerUI>();
        if (playerUI != null)
        {
            playerUI.SetMaxHealth(currentHP);
            playerUI.SetMaxKi(currentKi);
            playerUI.SetPotentialFromQuantum(currentTiemNang);
            playerUI.SetPowerFromQuantum(currentSucManh);
        }
        
        // Save to DB immediately
        var mapStateSaver = FindObjectOfType<MapStateSaver>();
        if (mapStateSaver != null)
        {
            mapStateSaver.SaveMapState();
            Debug.Log("Stats saved to DB immediately!");
        }
        else
        {
            Debug.LogWarning("MapStateSaver not found! Stats only saved to PlayerPrefs.");
        }
    }

    unsafe void UpdateQuantumPlayerInfo()
    {
        var game = QuantumRunner.Default?.Game;
        if (game == null)
        {
            Debug.LogWarning("Quantum game not running!");
            return;
        }

        var frame = game.Frames.Verified;
        var filter = frame.Filter<Quantum.PlayerInfo>();
        
        while (filter.Next(out var entity, out var playerInfo))
        {
            // Get pointer to PlayerInfo
            if (frame.Unsafe.TryGetPointer<Quantum.PlayerInfo>(entity, out var ptr))
            {
                // Update the player's stats in Quantum
                ptr->MaxHealth = currentHP;
                // ptr->CurrentHealth = currentHP; // REMOVED: Don't heal fully
                ptr->MaxKi = currentKi;
                // ptr->Ki = currentKi; // REMOVED: Don't restore Ki fully
                ptr->Damage = currentDamage;
                ptr->TiemNang = currentTiemNang;
                ptr->SucManh = currentSucManh;
                
                Debug.Log($"Updated Quantum PlayerInfo: MaxHealth={currentHP}, TiemNang={currentTiemNang}");
            }
            break; // Only update first player (local player)
        }
    }
    public void UpdateStats(int tiemNang, int sucManh)
    {
        currentTiemNang = tiemNang;
        currentSucManh = sucManh;
        UpdateCurrencyDisplay();
    }
}
