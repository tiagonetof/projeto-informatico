using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlantData
{
    public int plantIndex;
    public Vector3 localPosition;
    public Quaternion localRotation;
}

[Serializable]
public class GardenData
{
    public List<PlantData> plants = new List<PlantData>();
}