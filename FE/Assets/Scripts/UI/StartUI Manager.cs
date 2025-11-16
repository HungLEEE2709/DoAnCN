using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUIManager : MonoBehaviour
{
    [Header("Buttons")]
    public UnityEngine.UI.Button continueButton;     
    public UnityEngine.UI.Button newGameButton;      
    public UnityEngine.UI.Button changeAccountButton; 

    [Header("Scene Names")]
    public string quantumGameScene = "QuantumGameScene";
    public string createPlayerScene = "Create Player";
    public string loginUIScene = "LoginUI";

    private void Start()
    {
        continueButton.onClick.AddListener(OnContinueClick);
        newGameButton.onClick.AddListener(OnNewGameClick);
        changeAccountButton.onClick.AddListener(OnChangeAccountClick);
    }

    private void OnContinueClick()
    {
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            SceneManager.LoadScene(quantumGameScene);
        }
        else
        {
            SceneManager.LoadScene(createPlayerScene);
        }
    }

    private void OnNewGameClick()
    {
        SceneManager.LoadScene(createPlayerScene);
    }

    private void OnChangeAccountClick()
    {
        SceneManager.LoadScene(loginUIScene);
    }
}
