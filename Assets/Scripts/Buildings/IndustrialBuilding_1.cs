using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorkPlace))]
public class IndustrialBuilding_1 : Building
{
    IndustrialBuildingStats industryStats = new IndustrialBuildingStats();
    WorkPlace workPlace;

    [SerializeField][Range(0.0f, 10.0f)] float minEmissionToGenerateSmoke = 6.0f; //bellow or equall to this, smoke particle system would be turned off.
    [SerializeField][Range(20.0f, 60.0f)] float maxEmissionToGenerateSmoke = 30.0f; 
    [SerializeField][Range(0.5f, 1.0f)] float maxSmokeOpacity = 0.75f;
    ParticleSystem smoke;

    public float currentEmissionRate {get; private set;} //unit volume per unit time.
    public int currentProduction {get; private set;}

    protected override void Awake()
    {
        base.Awake();

        workPlace = this.gameObject.GetComponent<WorkPlace>();
        smoke = this.transform.Find("Smoke").GetComponent<ParticleSystem>();
        UpdateEmissionVisuals(0.0f);
        stats.type = BuildingType.industrial;
        currentProduction = 0;
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.industrial);
    }

    public float ComputeProduction() //Must be called only once per day. Returns profit for day. Different from ComputeProduction in InfrastructureBuildings in that it doesn't depend on load.
    {                               //TODO rename this
        currentProduction = Mathf.FloorToInt(ComputeEfficiency() * industryStats.maxProductionPerDay);
        currentEmissionRate = industryStats.emissionPerProductUnit * (float)currentProduction;
        UpdateEmissionVisuals(currentEmissionRate);

        return (float)currentProduction * industryStats.incomePerProduct;
    }
    
    float ComputeEfficiency() //nearly similar to InfrastructureBuildings efficiency, the difference is that it isn't clamped to custom min and max value, but straight to 1.0f and 0.0f.
    {
        float budgetEffect = (float)(budget - stats.minBudget) / (float) (stats.maxBudget - stats.minBudget); //linear interpolation
        
        float manpowerEffect = 0.0f;        
        if (workPlace.MaxManpower() == 0) //building does not need manpower
            manpowerEffect = 1.0f;          //TODO this could be removed (unless you're thinking of implementing some futuristic, fully automated industry)
        else if (workPlace.CurrentManpower() >= workPlace.MinManpower())
            manpowerEffect = (float)workPlace.CurrentManpower() / (float)workPlace.MaxManpower();

        float resourceEffect = allocatedResources.CompareToBaseResource(stats.requiredResources);
        float efficiency =  manpowerEffect * budgetEffect * resourceEffect;
        
        if (manpowerEffect < 0.001f || budgetEffect < 0.001f || resourceEffect < 0.001f)
            efficiency = 0.0f;

        return efficiency;
    }

    public override void UpdateEffectOnNature(int timeWindow)
    {
        base.UpdateEffectOnNature(timeWindow);
        GameManager.climateMan.AddPollution(occupiedCell[0], occupiedCell[1], currentEmissionRate * timeWindow);
    }
    
    void UpdateEmissionVisuals(float emission) //leaving the emission value as an argument instead of 0 to give option of turning the emission visuals off without setting global parameters to zero
    {
        float emissionToVisualize = Mathf.Clamp(emission, minEmissionToGenerateSmoke, maxEmissionToGenerateSmoke);
        ParticleSystem.EmissionModule particleEmission = smoke.emission;

        if (emissionToVisualize - minEmissionToGenerateSmoke < 0.001f) //nearly zero, in this case, no need to do remaining calculation or have particle system running.
        {
            particleEmission.enabled = false;
            return;
        }

        particleEmission.enabled = true;
        float emissionVisualPercentage = (emissionToVisualize - minEmissionToGenerateSmoke) / (maxEmissionToGenerateSmoke - minEmissionToGenerateSmoke);

        ParticleSystem.MainModule particleMain = smoke.main;
        particleMain.startColor = new Color(particleMain.startColor.color.r, particleMain.startColor.color.g, particleMain.startColor.color.b, emissionVisualPercentage * maxSmokeOpacity);
    }

}

[System.Serializable]
public class IndustrialBuildingStats
{
    public float incomePerProduct = 10.0f; //units of fund
    public int maxProductionPerDay = 5; //in units of products
    public float emissionPerProductUnit = 0.5f; //unit volume per unity time per unit power
}
