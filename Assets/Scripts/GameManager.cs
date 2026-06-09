using UnityEngine;

// This class represents the "Identity Card" for each plant
[System.Serializable]
public class PlantInfo
{
    public string plantName;        // Name of the plant
    public int requiredLevel;       // Level needed to unlock this plant
    public int totalPlanted;        // How many times this plant has been placed in AR
    public Sprite plantIcon;        // Image of the plant
    public GameObject plantPrefab;  // The 3D model used in the AR scene
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public int currentLevel = 1; // Starting level, can be increased as the player progresses

    [Header("Inventory Selection")]
    public int selectedPlantIndex = -1; // -1 means no plant is currently selected

    [Header("Plant Database")]
    // Our array of 6 plants
    public PlantInfo[] plantDatabase = new PlantInfo[6]; 

    private void Awake()
    {
        // Singleton Pattern: keeps the GameManager alive between scene changes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Function called by the AR scene when a plant is successfully placed
    public void AddPlantedItem(int index)
    {
        if (index >= 0 && index < plantDatabase.Length)
        {
            plantDatabase[index].totalPlanted++;
            Debug.Log("Planted " + plantDatabase[index].plantName + "! Total: " + plantDatabase[index].totalPlanted);
        }
    }
}