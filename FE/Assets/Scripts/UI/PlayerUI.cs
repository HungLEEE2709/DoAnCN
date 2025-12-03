using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class PlayerUI : MonoBehaviour
{

    public Image healthBar;
    public Image staminaBar;

    public TextMeshProUGUI healthAmount;
    public TextMeshProUGUI staminaAmount;
    public TextMeshProUGUI goldAmount;
    public TextMeshProUGUI potentialAmount;
    public TextMeshProUGUI powerAmount;

    public float maxHealth;
    public float currentHealth;

    public float maxKi;
    public float currentKi;
    public int currentGold;
    public int currentPotential;
    public int currentPower;

    private string userId;

    private bool statsChanged = false;
    private float sendCooldown = 0.5f;
    private float timer = 0;

    private float kiRegenRate = 0.5f;

    void Start()
    {
        userId = PlayerPrefs.GetString("idUser", "");

        StartCoroutine(LoadPlayerStats());
    }

    void Update()
    {
        RegenerateKi();
        UpdateUI();

        if (statsChanged)
        {
            timer += Time.deltaTime;
            if (timer >= sendCooldown)
            {
                StartCoroutine(UpdateStatsToServer());
                timer = 0;
                statsChanged = false;
            }
        }
    }

    IEnumerator LoadPlayerStats()
    {
        string url = GameConfig.BaseUrl + "/api/playerInfo/chosen/" + userId;
        UnityWebRequest req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();

        PlayerResponse data =
            JsonUtility.FromJson<PlayerResponse>(req.downloadHandler.text);

        maxHealth = Mathf.Max(1, data.player.MaxHp);
        maxKi = Mathf.Max(1, data.player.MaxKi);

        currentHealth = data.player.Hp;
        currentKi = data.player.Ki;
        currentGold = data.player.Vang;
        currentPotential = data.player.TiemNang;
        currentPower = data.player.SucManh;
    }

    public void TakeDamage(float dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        statsChanged = true;
    }

    public void UseKi(float amt)
    {
        currentKi -= amt;
        currentKi = Mathf.Clamp(currentKi, 0, maxKi);
        statsChanged = true;
    }

    void RegenerateKi()
    {
        if (currentKi < maxKi)
        {
            currentKi += kiRegenRate * Time.deltaTime;
            currentKi = Mathf.Clamp(currentKi, 0, maxKi);
            statsChanged = true;
        }
    }

    public void SetHealthFromQuantum(float hp)
    {
        currentHealth = Mathf.Clamp(hp, 0, maxHealth);
        statsChanged = true;
    }

    public void SetGoldFromQuantum(int gold)
    {
        if (currentGold != gold)
        {
            currentGold = gold;
            statsChanged = true;
            
            // Update Inventory UI
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.UpdateGoldUI(currentGold);
            }
        }
    }

    public void SetPotentialFromQuantum(int potential)
    {
        if (currentPotential != potential)
        {
            currentPotential = potential;
            statsChanged = true;
            
            if (StatsUpgradeManager.Instance != null)
            {
                StatsUpgradeManager.Instance.UpdateStats(currentPotential, currentPower);
            }
        }
    }

    public void SetPowerFromQuantum(int power)
    {
        if (currentPower != power)
        {
            currentPower = power;
            statsChanged = true;

            if (StatsUpgradeManager.Instance != null)
            {
                StatsUpgradeManager.Instance.UpdateStats(currentPotential, currentPower);
            }
        }
    }

    public void SetMaxHealth(float max)
    {
        maxHealth = max;
        UpdateUI();
    }

    public void SetMaxKi(float max)
    {
        maxKi = max;
        UpdateUI();
    }

    private void UpdateUI()
    {
        float hpFill = currentHealth / maxHealth;
        float kiFill = currentKi / maxKi;

        healthBar.fillAmount = Mathf.Clamp01(hpFill);
        staminaBar.fillAmount = Mathf.Clamp01(kiFill);

        healthAmount.text = Mathf.RoundToInt(currentHealth).ToString();
        staminaAmount.text = Mathf.RoundToInt(currentKi).ToString();
        if (goldAmount != null) goldAmount.text = currentGold.ToString();
        if (potentialAmount != null) potentialAmount.text = currentPotential.ToString();
        if (powerAmount != null) powerAmount.text = currentPower.ToString();
    }

    IEnumerator UpdateStatsToServer()
    {
        string url = GameConfig.BaseUrl + "/api/playerInfo/updatestats";

        PlayerStatsData payload = new PlayerStatsData()
        {
            idUser = userId,
            Hp = Mathf.RoundToInt(currentHealth),
            Ki = Mathf.RoundToInt(currentKi)
        };

        string json = JsonUtility.ToJson(payload);

        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();
    }
}
