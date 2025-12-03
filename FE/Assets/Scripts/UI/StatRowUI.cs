using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatRowUI : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI statName;
    public TextMeshProUGUI currentValue;
    public Button upgradeButton;
    public TextMeshProUGUI upgradeButtonText;

    private string statType;
    private int upgradeCost;

    public void Setup(string type, string name, int current, int cost)
    {
        statType = type;
        upgradeCost = cost;

        statName.text = name;
        currentValue.text = "Hiện tại: " + current.ToString("N0");
        upgradeButtonText.text = "+ " + cost.ToString("N0") + " TN";

        // Hide upgrade button initially
        upgradeButton.gameObject.SetActive(false);

        // Add click listener to row
        GetComponent<Button>().onClick.AddListener(OnRowClick);
        
        // Add click listener to upgrade button
        upgradeButton.onClick.AddListener(OnUpgradeClick);
    }

    void OnRowClick()
    {
        // Toggle upgrade button visibility
        upgradeButton.gameObject.SetActive(!upgradeButton.gameObject.activeSelf);
    }

    void OnUpgradeClick()
    {
        // Call manager to handle upgrade
        StatsUpgradeManager.Instance.UpgradeStat(statType, upgradeCost);
    }

    public void UpdateDisplay(int newValue, int newCost)
    {
        currentValue.text = "Hiện tại: " + newValue.ToString("N0");
        upgradeButtonText.text = "+ " + newCost.ToString("N0") + " TN";
        upgradeCost = newCost;
    }
}
