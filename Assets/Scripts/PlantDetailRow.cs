using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PlantDetailRow : MonoBehaviour
{
    [Header("UI Text Fields")]
    public TMP_Text txtPlant;             
    public TMP_Text txtPlantedDate;       
    public TMP_Text txtLastWateredDate;   
    public TMP_Text txtTimesWatered;      
    public TMP_Text txtStage;             

    [Header("UI Hidden Elements (By Default)")]
    public Button btnWater;               
    public TMP_Text txtNeedsWater;        

    private PlantData attachedPlantData; 

    public void SetupRow(PlantData data, int plantNumber)
    {
        attachedPlantData = data;
        txtPlant.text = "Plant #" + plantNumber; 
        UpdateRowUI();
    }

    public void UpdateRowUI()
    {
        txtPlantedDate.text = "Planted at: " + attachedPlantData.plantingDate;
        txtTimesWatered.text = "Times Watered: " + attachedPlantData.daysWatered; 

        if (attachedPlantData.daysWatered >= 5 && attachedPlantData.plantStage == 0)
        {
            attachedPlantData.plantStage = 1;
        }
        
        if (string.IsNullOrEmpty(attachedPlantData.lastWatered))
        {
            txtLastWateredDate.text = "Last Watered: Never";
        }
        else
        {
            txtLastWateredDate.text = "Last Watered: " + attachedPlantData.lastWatered;
        }
        
        if (attachedPlantData.plantStage == 0) txtStage.text = "Stage: Initial";
        else txtStage.text = "Stage: Final";

        bool requiresWater = CheckIfPlantNeedsWater();
        
        btnWater.gameObject.SetActive(requiresWater);     
        txtNeedsWater.gameObject.SetActive(requiresWater); 
    }

    private bool CheckIfPlantNeedsWater()
    {
        if (string.IsNullOrEmpty(attachedPlantData.lastWatered)) return true;

        try
        {
            DateTime lastWateredDate = DateTime.ParseExact(attachedPlantData.lastWatered, "dd/MM/yyyy HH:mm", null);
            TimeSpan timePassed = DateTime.Now - lastWateredDate;
            int allowedDays = GameManager.Instance.plantDatabase[attachedPlantData.plantIndex].wateringIntervalDays;

            return timePassed.TotalDays >= allowedDays;
        }
        catch
        {
            return true; 
        }
    }

    public void OnWaterClick()
    {
        if (attachedPlantData != null)
        {
            attachedPlantData.daysWatered++; 
            attachedPlantData.lastWatered = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            if (attachedPlantData.daysWatered >= 5 && attachedPlantData.plantStage == 0)
            {
                attachedPlantData.plantStage = 1; 
                if (GameManager.Instance != null) GameManager.Instance.OnPlantFullyGrown();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveUserGardenToPrefs();
            }

            UpdateRowUI(); 
        }
    }
}