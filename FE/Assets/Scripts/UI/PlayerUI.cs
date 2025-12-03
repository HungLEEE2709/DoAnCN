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
        string url = "http://localhost:5000/api/playerInfo/chosen/" + userId;
        UnityWebRequest req = UnityWebRequest.Get(url);

        yield return req.SendWebRequest();

        PlayerResponse data =
            JsonUtility.FromJson<PlayerResponse>(req.downloadHandler.text);

        maxHealth = Mathf.Max(1, data.player.Hp);
        maxKi = Mathf.Max(1, data.player.Ki);

        currentHealth = maxHealth;
        currentKi = maxKi;
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

    private void UpdateUI()
    {
        float hpFill = currentHealth / maxHealth;
        float kiFill = currentKi / maxKi;

        healthBar.fillAmount = Mathf.Clamp01(hpFill);
        staminaBar.fillAmount = Mathf.Clamp01(kiFill);

        healthAmount.text = Mathf.RoundToInt(currentHealth).ToString();
        staminaAmount.text = Mathf.RoundToInt(currentKi).ToString();
    }

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
    }
}
