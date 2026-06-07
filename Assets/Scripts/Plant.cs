using System.Collections;
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

    [Header("Growth")]
    [SerializeField] private float growthTime = 10f;

    private PlantStage currentStage;
    private Coroutine growthCoroutine;
    private bool initialized;
    private float remainingGrowthTime;
    private float growthStartedAtRealtime;

    public PlantStage CurrentStage => currentStage;
    public float GrowthTime => growthTime;
    public float RemainingGrowthTime
    {
        get
        {
            if (currentStage == PlantStage.Adult)
                return 0f;

            return Mathf.Max(0f, remainingGrowthTime - (Time.realtimeSinceStartup - growthStartedAtRealtime));
        }
    }

    private void Start()
    {
        if (!initialized)
            Initialize(PlantStage.Sprout, growthTime);
    }

    public void Initialize(PlantStage stage, float remainingGrowthTime = 0f)
    {
        initialized = true;
        SetStage(stage);

        if (growthCoroutine != null)
            StopCoroutine(growthCoroutine);

        if (stage == PlantStage.Sprout)
        {
            this.remainingGrowthTime = Mathf.Max(0f, remainingGrowthTime);
            growthStartedAtRealtime = Time.realtimeSinceStartup;
            growthCoroutine = StartCoroutine(GrowthRoutine(this.remainingGrowthTime));
        }
        else
        {
            this.remainingGrowthTime = 0f;
        }
    }

    public void StartGrowth()
    {
        Initialize(PlantStage.Sprout, growthTime);
    }

    private IEnumerator GrowthRoutine(float remainingGrowthTime)
    {
        yield return new WaitForSeconds(remainingGrowthTime);

        SetStage(PlantStage.Adult);
        this.remainingGrowthTime = 0f;
        growthCoroutine = null;
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
