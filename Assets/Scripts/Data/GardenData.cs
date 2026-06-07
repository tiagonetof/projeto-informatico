using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlantData
{
    public int plantIndex;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public int plantStage = (int)Plant.PlantStage.Sprout;
    public float remainingGrowthTime;
}

[Serializable]
public class GardenData
{
    public int version = 2;
    public List<PlantData> plants = new List<PlantData>();
}
