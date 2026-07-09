using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlantDetailsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rowPrefab;  
    public Transform container;   
    public TMP_Text plantCountText;

    void Start()
    {
        GenerateDynamicList();
    }

    public void GenerateDynamicList()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        if (GameManager.Instance == null) return;

        int selectedSpecies = GameManager.Instance.selectedPlantIndex;
        if (selectedSpecies < 0)
        {
            if (plantCountText != null)
            {
                plantCountText.text = "Planted: 0";
            }

            return;
        }

        if (selectedSpecies >= GameManager.Instance.plantDatabase.Length ||
            GameManager.Instance.plantDatabase[selectedSpecies] == null)
        {
            if (plantCountText != null)
            {
                plantCountText.text = "Planted: 0";
            }

            return;
        }

        if (plantCountText != null)
        {
            plantCountText.text = GameManager.Instance.plantDatabase[selectedSpecies].totalPlanted.ToString();
        }

        List<PlantData> activePlants = GameManager.Instance.GetPlantedInstancesOfSpecies(selectedSpecies);

        for (int i = 0; i < activePlants.Count; i++)
        {
            GameObject newRow = Instantiate(rowPrefab, container);
            PlantDetailRow rowScript = newRow.GetComponent<PlantDetailRow>();
            if (rowScript != null)
            {
                int plantDisplayNumber = i + 1; 
                rowScript.SetupRow(activePlants[i], plantDisplayNumber);
            }
        }
    }
}