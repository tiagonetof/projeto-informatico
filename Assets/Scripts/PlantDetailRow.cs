using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Globalization;

public class PlantDetailRow : MonoBehaviour
{
    private const int InitialStageValue = 0;
    private const int FinalStageValue = 1;
    private const int DeadStageValue = 2;

    private const float InitialNeedsWaterSeconds = 5f;
    private const float InitialRiskOfDeathSeconds = 10f;
    private const float InitialPlantDeadSeconds = 15f;

    private const float FinalNeedsWaterSeconds = 20f * 60f;
    private const float FinalRiskOfDeathSeconds = 40f * 60f;
    private const float FinalPlantDeadSeconds = 60f * 60f;
    private const string TimestampFormatWithSeconds = "dd/MM/yyyy HH:mm:ss";
    private const string LegacyTimestampFormat = "dd/MM/yyyy HH:mm";

    private enum WaterStatus
    {
        Hidden,
        NeedsWater,
        RiskOfDeath,
        Dead
    }

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
        txtPlantedDate.text = "Planted at: " + FormatTimestampForDisplay(attachedPlantData.plantingDate);
        txtTimesWatered.text = "Times Watered: " + attachedPlantData.daysWatered; 

        if (attachedPlantData.daysWatered >= 5 && attachedPlantData.plantStage == InitialStageValue)
        {
            attachedPlantData.plantStage = FinalStageValue;
        }
        
        if (string.IsNullOrEmpty(attachedPlantData.lastWatered))
        {
            txtLastWateredDate.text = "Last Watered: Never";
        }
        else
        {
            txtLastWateredDate.text = "Last Watered: " + FormatTimestampForDisplay(attachedPlantData.lastWatered);
        }
        
        if (attachedPlantData.plantStage == InitialStageValue) txtStage.text = "Stage: Initial";
        else if (attachedPlantData.plantStage == FinalStageValue) txtStage.text = "Stage: Final";
        else txtStage.text = "Stage: Dead";

        ApplyWaterStatusUI();
    }

    private WaterStatus GetWaterStatus()
    {
        DateTime referenceTime;

        if (!TryGetReferenceTime(out referenceTime))
        {
            return WaterStatus.NeedsWater;
        }

        TimeSpan timePassed = DateTime.Now - referenceTime;
        double elapsedSeconds = timePassed.TotalSeconds;
        float deadThreshold;
        float riskThreshold;
        float needsThreshold;

        GetWaterThresholds(out needsThreshold, out riskThreshold, out deadThreshold);

        if (attachedPlantData.plantStage == DeadStageValue)
        {
            return WaterStatus.Dead;
        }

        if (elapsedSeconds >= deadThreshold)
        {
            return WaterStatus.Dead;
        }

        if (elapsedSeconds >= riskThreshold)
        {
            return WaterStatus.RiskOfDeath;
        }

        if (elapsedSeconds >= needsThreshold)
        {
            return WaterStatus.NeedsWater;
        }

        return WaterStatus.Hidden;
    }

    private void ApplyWaterStatusUI()
    {
        WaterStatus status = GetWaterStatus();

        if (status == WaterStatus.Dead)
        {
            MarkPlantAsDead();
        }

        if (btnWater != null)
        {
            btnWater.gameObject.SetActive(status == WaterStatus.NeedsWater || status == WaterStatus.RiskOfDeath);
        }

        if (txtNeedsWater == null)
        {
            return;
        }

        if (status == WaterStatus.Hidden)
        {
            txtNeedsWater.gameObject.SetActive(false);
            return;
        }

        txtNeedsWater.gameObject.SetActive(true);

        if (status == WaterStatus.NeedsWater)
        {
            txtNeedsWater.text = "Needs Water!";
            txtNeedsWater.color = Color.black;
        }
        else if (status == WaterStatus.RiskOfDeath)
        {
            txtNeedsWater.text = "Risk of Death!";
            txtNeedsWater.color = Color.red;
        }
        else
        {
            txtNeedsWater.text = "Plant is Dead!";
            txtNeedsWater.color = Color.red;
        }
    }

    private void GetWaterThresholds(out float needsThreshold, out float riskThreshold, out float deadThreshold)
    {
        if (attachedPlantData.plantStage == FinalStageValue)
        {
            needsThreshold = FinalNeedsWaterSeconds;
            riskThreshold = FinalRiskOfDeathSeconds;
            deadThreshold = FinalPlantDeadSeconds;
            return;
        }

        needsThreshold = InitialNeedsWaterSeconds;
        riskThreshold = InitialRiskOfDeathSeconds;
        deadThreshold = InitialPlantDeadSeconds;
    }

    private void MarkPlantAsDead()
    {
        if (attachedPlantData.plantStage == DeadStageValue)
        {
            return;
        }

        attachedPlantData.plantStage = DeadStageValue;
        txtStage.text = "Stage: Dead";

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveUserGardenToPrefs();
            GameManager.Instance.EvaluateLevelProgress();
        }
    }

    private bool TryGetReferenceTime(out DateTime referenceTime)
    {
        referenceTime = default;

        if (!string.IsNullOrEmpty(attachedPlantData.lastWatered) &&
            TryParseTimestamp(attachedPlantData.lastWatered, out referenceTime))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(attachedPlantData.plantingDate) &&
            TryParseTimestamp(attachedPlantData.plantingDate, out referenceTime))
        {
            return true;
        }

        return false;
    }

    private bool TryParseTimestamp(string value, out DateTime parsedDate)
    {
        return DateTime.TryParseExact(
            value,
            new[] { TimestampFormatWithSeconds, LegacyTimestampFormat },
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate);
    }

    private string FormatTimestampForDisplay(string value)
    {
        if (TryParseTimestamp(value, out DateTime parsedDate))
        {
            return parsedDate.ToString(TimestampFormatWithSeconds);
        }

        return value;
    }

    private void Update()
    {
        if (attachedPlantData == null)
        {
            return;
        }

        ApplyWaterStatusUI();
    }

    public void OnWaterClick()
    {
        if (attachedPlantData != null)
        {
            if (attachedPlantData.plantStage == DeadStageValue)
            {
                return;
            }

            attachedPlantData.daysWatered++; 
            attachedPlantData.lastWatered = DateTime.Now.ToString(TimestampFormatWithSeconds);

            if (attachedPlantData.daysWatered >= 5 && attachedPlantData.plantStage == InitialStageValue)
            {
                attachedPlantData.plantStage = FinalStageValue; 
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