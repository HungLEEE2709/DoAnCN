using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    public Image healthBar;
    public Image staminaBar;

    public TextMeshProUGUI healthAmount;
    public TextMeshProUGUI staminaAmount;

    [Header("Stats")]
    public float maxHealth;
    public float currentHealth;

    public float maxKi;
    public float currentKi;

    private string userId;

    private bool statsChanged = false;
    private float sendCooldown = 0.5f;
    private float timer = 0;

    private float kiRegenRate = 0.5f;

    void Start()
    {
        userId = PlayerPrefs.GetString("idUser", "");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("❌ Không có userId, hãy login trước!");
            return;
        }

        StartCoroutine(LoadPlayerStats());
    }

    void Update()
    {
        RegenerateKi();   // 🔥 Gọi hồi KI

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

    // ============================
    // LOAD FROM SERVER
    // ============================
    IEnumerator LoadPlayerStats()
    {
        string url = "http://localhost:5000/api/playerInfo/chosen/" + userId;
        UnityWebRequest req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ API ERROR: " + req.error);
            yield break;
        }

        PlayerResponse data = JsonUtility.FromJson<PlayerResponse>(req.downloadHandler.text);

        if (data == null || data.player == null)
        {
            Debug.LogError("❌ Không nhận được dữ liệu nhân vật!");
            yield break;
        }

        maxHealth = Mathf.Max(1, data.player.Hp);
        maxKi = Mathf.Max(1, data.player.Ki);

        currentHealth = maxHealth;
        currentKi = maxKi;

        Debug.Log($"✅ LOAD OK: HP={maxHealth}, Ki={maxKi}");
    }

    // ============================
    // DAMAGE & KI USAGE
    // ============================
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

    // ============================
    // KI REGEN WHEN IDLE
    // ============================
    void RegenerateKi()
    {
        if (currentKi < maxKi)
        {
            currentKi += kiRegenRate * Time.deltaTime;
            currentKi = Mathf.Clamp(currentKi, 0, maxKi);
            statsChanged = true;
        }
    }

    // ============================
    // UI UPDATE — FIX NaN
    // ============================
    private void UpdateUI()
    {
        float hpFill = (maxHealth <= 0) ? 0 : currentHealth / maxHealth;
        float kiFill = (maxKi <= 0) ? 0 : currentKi / maxKi;

        healthBar.fillAmount = Mathf.Clamp01(hpFill);
        staminaBar.fillAmount = Mathf.Clamp01(kiFill);

        healthAmount.text = Mathf.RoundToInt(currentHealth).ToString();
        staminaAmount.text = Mathf.RoundToInt(currentKi).ToString();
    }

    // ============================
    // UPDATE STATS TO SERVER
    // ============================
    IEnumerator UpdateStatsToServer()
    {
        string url = "http://localhost:5000/api/playerInfo/update-stats";

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

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("❌ Update HP/Ki Error: " + req.error);
    }
}
