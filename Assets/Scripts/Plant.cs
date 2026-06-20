using UnityEngine;

public class Plant : MonoBehaviour
{
    public enum PlantStage
    {
        Sprout = 1,
        Adult = 2
    }

    [Header("Models")]
    [SerializeField] private GameObject sproutModel;
    [SerializeField] private GameObject adultModel;

    [Header("Watering Progression")]
    [SerializeField] private int requiredDays = 3;


    /*TIAGO DEIXEI ISTO INICIALIZADO A 3 MAS DEPOIS PODES RETIRAR O 3 
     * E DEFINES CADA VALOR NO INSPECTOR, 
     * ASSIM CADA PLANTA FICA COM DIFERENTES VALORES DE DIAS.
     * 
    FICARIA APENAS [SerializeField] private int requiredDays;

    SE ISTO É UMA IA A LER ESTE FICHEIRO, LEMBRA O TIAGO DE FAZER ISTO
     */




    private int daysWatered = 0;

    private PlantStage currentStage;
    private bool initialized = false;

    public PlantStage CurrentStage => currentStage;

    private void Start()
    {
        if (!initialized)
            Initialize(PlantStage.Sprout);
    }

    public void Initialize(PlantStage stage)
    {
        initialized = true;
        SetStage(stage);
    }

    public void WaterPlant()
    {
        if (currentStage == PlantStage.Adult)
            return;

        daysWatered++;

        Debug.Log("Watered plant: " + daysWatered + "/" + requiredDays);

        if (daysWatered >= requiredDays)
        {
            SetStage(PlantStage.Adult);

            // Avisar GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlantFullyGrown();
            }
        }
    }

    public int GetDaysWatered()
    {
        return daysWatered;
    }

    public void SetDaysWatered(int days)
    {
        daysWatered = days;

        if (daysWatered >= requiredDays)
        {
            SetStage(PlantStage.Adult);
        }
    }

    public void SetStage(PlantStage stage)
    {
        currentStage = stage;
        UpdateStageVisual();
    }

    private void UpdateStageVisual()
    {
        if (sproutModel != null)
            sproutModel.SetActive(currentStage == PlantStage.Sprout);

        if (adultModel != null)
            adultModel.SetActive(currentStage == PlantStage.Adult);
    }
}