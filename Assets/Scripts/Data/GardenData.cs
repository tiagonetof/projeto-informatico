using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GardenData
{
    public int version;
    public List<PlantData> plants = new List<PlantData>();
}

[System.Serializable]
public class PlantData
{
    public int plantIndex;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public int plantStage = 0; 
    public int daysWatered;
    
    public string lastWatered;   
    public string plantingDate;  
}