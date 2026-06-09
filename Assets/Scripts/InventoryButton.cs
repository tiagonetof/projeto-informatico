using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryButton : MonoBehaviour
{
    [Header("Plant Setup")]
    public int plantIndex; // 0 for Plant1, 1 for Plant2, up to 5 for Plant6

    [Header("UI References")]
    public TMP_Text plantNameText;  // Reference to Txt_NamePlantX
    public TMP_Text countText;      // Reference to Txt_NumberPlantsX
    public Image plantImage;        // Reference to Img_PlantX
    
    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        UpdateButtonUI();
    }

    public void UpdateButtonUI()
    {
        // Safety check to ensure the GameManager exists
        if (GameManager.Instance == null) return;

        // Fetch the specific plant info from the GameManager database
        PlantInfo myInfo = GameManager.Instance.plantDatabase[plantIndex];

        // Level Validation Logic:
        // Level 1 allows Index 0 and 1
        // Level 2 allows Index 0, 1, 2, 3
        // Level 3 allows Index 0, 1, 2, 3, 4, 5
        if (GameManager.Instance.currentLevel >= myInfo.requiredLevel)
        {
            // UNLOCKED: Button is clickable, shows the real name and quantity
            myButton.interactable = true; 
            plantNameText.text = myInfo.plantName;
            countText.text = "Planted: " + myInfo.totalPlanted;

            if (myInfo.plantIcon != null)
            {
                plantImage.sprite = myInfo.plantIcon;
            }
        }
        else
        {
            // LOCKED: Button is disabled (greyed out), shows locked information
            myButton.interactable = false; 
            plantNameText.text = "Locked";
            countText.text = "Requires Lvl " + myInfo.requiredLevel;
        }
    }

    // This function will run when the player clicks an UNLOCKED button
    public void OnClickSelectPlant()
    {
        GameManager.Instance.selectedPlantIndex = plantIndex;
        Debug.Log("Selected plant index: " + plantIndex + " (" + GameManager.Instance.plantDatabase[plantIndex].plantName + ")");
    }
}