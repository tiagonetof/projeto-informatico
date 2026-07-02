using System.Collections.Generic;
using UnityEngine;

// This class represents the "Identity Card" for each plant
[System.Serializable]
public class PlantInfo
{
    public string plantName;        
    public int requiredLevel;       
    public int totalPlanted;        
    public Sprite plantIcon;        
    public GameObject plantPrefab;  
    public int wateringIntervalDays; // O Unity estava a queixar-se da falta disto!
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private const string GardenDataKey = "GardenData";
    private const string PlayerLevelKey = "PlayerLevel";
    private const int CurrentGardenDataVersion = 2;

    [Header("Player Data")]
    public int currentLevel = 1; 

    [Header("Inventory Selection")]
    public int selectedPlantIndex = -1; 

    [Header("Plant Database")]
    public PlantInfo[] plantDatabase = new PlantInfo[6];

    [Header("Dynamic Garden Data")]
    public GardenData userGarden = new GardenData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadUserGardenFromPrefs();
            LoadPlayerLevelFromPrefs();
            RebuildPlantCountsFromGarden();
            EvaluateLevelProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlantedItem(int index)
    {
        int speciesIndex = GetSpeciesIndexFromArIndex(index);
        if (speciesIndex < 0 || speciesIndex >= plantDatabase.Length)
            return;

        plantDatabase[speciesIndex].totalPlanted++;

        Debug.Log("Planted " + plantDatabase[speciesIndex].plantName + "! Total: " + plantDatabase[speciesIndex].totalPlanted);
    }

    public int GetSpeciesIndexFromArIndex(int arIndex)
    {
        if (arIndex < 0 || plantDatabase == null || plantDatabase.Length == 0)
            return -1;

        return arIndex % plantDatabase.Length;
    }

    public void SaveUserGardenToPrefs()
    {
        if (userGarden == null)
            userGarden = new GardenData();

        userGarden.version = CurrentGardenDataVersion;

        if (userGarden.plants == null || userGarden.plants.Count == 0)
        {
            PlayerPrefs.DeleteKey(GardenDataKey);
            PlayerPrefs.Save();
            return;
        }

        string json = JsonUtility.ToJson(userGarden);
        PlayerPrefs.SetString(GardenDataKey, json);
        PlayerPrefs.Save();
    }

    public void SavePlayerLevelToPrefs()
    {
        PlayerPrefs.SetInt(PlayerLevelKey, currentLevel);
        PlayerPrefs.Save();
    }

    public void LoadPlayerLevelFromPrefs()
    {
        if (PlayerPrefs.HasKey(PlayerLevelKey))
        {
            currentLevel = Mathf.Max(1, PlayerPrefs.GetInt(PlayerLevelKey, 1));
        }
    }

    public void LoadUserGardenFromPrefs()
    {
        if (!PlayerPrefs.HasKey(GardenDataKey))
        {
            if (userGarden == null)
                userGarden = new GardenData();

            RebuildPlantCountsFromGarden();

            return;
        }

        string json = PlayerPrefs.GetString(GardenDataKey);

        if (string.IsNullOrWhiteSpace(json) || !json.Contains("\"version\""))
        {
            PlayerPrefs.DeleteKey(GardenDataKey);
            PlayerPrefs.Save();
            userGarden = new GardenData();
            RebuildPlantCountsFromGarden();
            return;
        }

        GardenData loadedGardenData = JsonUtility.FromJson<GardenData>(json);

        if (loadedGardenData == null || loadedGardenData.version != CurrentGardenDataVersion)
        {
            PlayerPrefs.DeleteKey(GardenDataKey);
            PlayerPrefs.Save();
            userGarden = new GardenData();
            RebuildPlantCountsFromGarden();
            return;
        }

        userGarden = loadedGardenData;
        RebuildPlantCountsFromGarden();
    }

    public void EvaluateLevelProgress()
    {
        if (plantDatabase == null || plantDatabase.Length == 0)
            return;

        int targetLevel = Mathf.Clamp(currentLevel, 1, 3);

        while (targetLevel < 3 && CanAdvanceFromLevel(targetLevel))
        {
            targetLevel++;
        }

        if (targetLevel > currentLevel)
        {
            currentLevel = targetLevel;
            SavePlayerLevelToPrefs();
        }
    }

    private bool CanAdvanceFromLevel(int level)
    {
        int requiredSpeciesCount = GetRequiredSpeciesCountForLevel(level);
        if (requiredSpeciesCount <= 0)
            return false;

        if (userGarden == null || userGarden.plants == null || userGarden.plants.Count == 0)
            return false;

        bool[] plantedSpecies = new bool[requiredSpeciesCount];

        for (int i = 0; i < requiredSpeciesCount; i++)
        {
            plantedSpecies[i] = false;
        }

        foreach (PlantData plantData in userGarden.plants)
        {
            if (plantData.plantIndex < 0 || plantData.plantIndex >= requiredSpeciesCount)
                continue;

            plantedSpecies[plantData.plantIndex] = true;

            if (plantData.plantStage != 1)
                return false;
        }

        for (int i = 0; i < requiredSpeciesCount; i++)
        {
            if (!plantedSpecies[i])
                return false;
        }

        return true;
    }

    private int GetRequiredSpeciesCountForLevel(int level)
    {
        if (level <= 0)
            return 0;

        if (level == 1)
            return 2;

        if (level == 2)
            return 4;

        return 6;
    }

    public void RebuildPlantCountsFromGarden()
    {
        if (plantDatabase == null)
            return;

        for (int i = 0; i < plantDatabase.Length; i++)
        {
            if (plantDatabase[i] != null)
            {
                plantDatabase[i].totalPlanted = 0;
            }
        }

        if (userGarden == null || userGarden.plants == null)
            return;

        foreach (PlantData plantData in userGarden.plants)
        {
            if (plantData.plantIndex < 0 || plantData.plantIndex >= plantDatabase.Length)
                continue;

            if (plantDatabase[plantData.plantIndex] != null)
            {
                plantDatabase[plantData.plantIndex].totalPlanted++;
            }
        }
    }

    public void SelectPlantSpecies(int index)
    {
        if (index < 0 || index >= plantDatabase.Length)
        {
            Debug.LogWarning("Invalid plant species index selected: " + index);
            return;
        }

        selectedPlantIndex = index;
    }

    // A função que o PlantDetailsManager precisa para filtrar as plantas
    public List<PlantData> GetPlantedInstancesOfSpecies(int speciesIndex)
    {
        List<PlantData> filteredPlants = new List<PlantData>();
        
        foreach (PlantData p in userGarden.plants)
        {
            if (p.plantIndex == speciesIndex)
            {
                filteredPlants.Add(p); 
            }
        }
        return filteredPlants;
    }

    public void OnPlantFullyGrown()
    {
        EvaluateLevelProgress();
    }
}