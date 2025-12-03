using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CreatePlayerPayload
{
    public string idUser;
    public string UserName;
    public string CharacterName;
}

[System.Serializable]
public class CreatePlayerResponse
{
    public bool success;
}

public class CreatePlayerUI : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField nameInput;

    [Header("Planet Buttons")]
    public Button planetBtn1;
    public Button planetBtn2;
    public Button planetBtn3;

    [Header("Character Buttons")]
    public Button charBtn1;
    public Button charBtn2;
    public TMP_Text char1Text;
    public TMP_Text char2Text;

    [Header("Prefabs")]
    public GameObject ryuPrefab;
    public GameObject lunaPrefab;
    public GameObject grimPrefab;
    public GameObject zikkPrefab;
    public GameObject eldriaPrefab;
    public GameObject morokPrefab;

    [Header("Scene Name")]
    public string nextScene = "QuantumGameScene";

    [Header("API")]
    public string apiCreate;

    private string selectedPlanet = "";
    private string selectedCharacter = "";
    private GameObject previewCharacter;

    private bool isSending = false;

    void Start()
    {
        planetBtn1.onClick.AddListener(() => SelectPlanet("Warrior"));
        planetBtn2.onClick.AddListener(() => SelectPlanet("Beast"));
        planetBtn3.onClick.AddListener(() => SelectPlanet("Mage"));

        charBtn1.onClick.AddListener(() => SelectCharacter(char1Text.text));
        charBtn2.onClick.AddListener(() => SelectCharacter(char2Text.text));
    }

    void SelectPlanet(string planet)
    {
        selectedPlanet = planet;

        switch (planet)
        {
            case "Warrior":
                char1Text.text = "RYU DAIKI";
                char2Text.text = "LUNA BLADE";
                break;
            case "Beast":
                char1Text.text = "GRIMJAW";
                char2Text.text = "ZIKK FANG";
                break;
            case "Mage":
                char1Text.text = "ELDRIA";
                char2Text.text = "MOROK";
                break;
        }

        RemovePreview();
    }

    void SelectCharacter(string name)
    {
        selectedCharacter = name;
        ShowPreview(GetPrefab(name));

        // Lưu luôn PrefabKey vào PlayerPrefs
        PlayerPrefs.SetString("PrefabKey", name);
        PlayerPrefs.Save();
        Debug.Log("Lưu PrefabKey: " + name);
    }

    GameObject GetPrefab(string name)
    {
        switch (name)
        {
            case "RYU DAIKI": return ryuPrefab;
            case "LUNA BLADE": return lunaPrefab;
            case "GRIMJAW": return grimPrefab;
            case "ZIKK FANG": return zikkPrefab;
            case "ELDRIA": return eldriaPrefab;
            case "MOROK": return morokPrefab;
        }
        return null;
    }

    void ShowPreview(GameObject prefab)
    {
        RemovePreview();

        previewCharacter = Instantiate(prefab);
        previewCharacter.transform.position = new Vector3(0, -2.5f, 0);
        previewCharacter.transform.localScale = Vector3.one * 2f;
    }

    void RemovePreview()
    {
        if (previewCharacter != null)
            Destroy(previewCharacter);
    }

    public void OnClickCreate()
    {
        if (!isSending)
            StartCoroutine(CreateRoutine());
    }

    IEnumerator CreateRoutine()
    {
        if (string.IsNullOrEmpty(nameInput.text))
            yield break;

        if (string.IsNullOrEmpty(selectedCharacter))
            yield break;

        string userId = PlayerPrefs.GetString("idUser");

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("userId trống — chưa login!");
            yield break;
        }

        isSending = true;

        CreatePlayerPayload payload = new CreatePlayerPayload
        {
            idUser = userId,
            UserName = nameInput.text,
            CharacterName = selectedCharacter
        };

        string json = JsonUtility.ToJson(payload);

        UnityWebRequest req = UnityWebRequest.PostWwwForm(apiCreate, "");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API lỗi: " + req.downloadHandler.text);
            isSending = false;
            yield break;
        }

        CreatePlayerResponse response =
            JsonUtility.FromJson<CreatePlayerResponse>(req.downloadHandler.text);

        if (!response.success)
        {
            Debug.LogError("Backend báo lỗi → không tạo được nhân vật");
            isSending = false;
            yield break;
        }

        // ✔ Thành công → vào game
        SceneManager.LoadScene(nextScene);
    }

    public void OnClickClose()
    {
        SceneManager.LoadScene("MyMenu");
    }
}
