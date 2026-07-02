using System.Collections.Generic;
using UnityEngine;

public class PlantDetailsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject rowPrefab;  
    public Transform container;   

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
            return;
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