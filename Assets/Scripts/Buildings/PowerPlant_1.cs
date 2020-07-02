using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPlant_1 : InfrastructureBuilding
{
    public float currentPowerProduction {get; private set;}
    public float currentEfficiency {get; private set;}
    public float currentMaxPowerProduction {get; private set;} //not to be confused with maxPowerProduction in PowerPlantStats. This one is variable depending on other simulation factors.
    public float currentEmissionRate {get; private set;} //unit volume per unit time.
    [SerializeField][Range(0.0f, 10.0f)] float minEmissionToGenerateSmoke = 6.0f; //bellow or equall to this, smoke particle system would be turned off.
    [SerializeField][Range(20.0f, 60.0f)] float maxEmissionToGenerateSmoke = 30.0f; 
    [SerializeField][Range(0.5f, 1.0f)] float maxSmokeOpacity = 0.75f;
    ParticleSystem smoke;    
    [SerializeField] PowerPlantStats plantStats = new PowerPlantStats();

    override protected void Awake()
    {
        base.Awake();
        smoke = this.transform.Find("Smoke").GetComponent<ParticleSystem>();
        UpdateEmissionVisuals(0.0f);
        infrastructureType = InfrastructureService.power;
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.powerPlant);
    }

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Grid.grid.SetInfrastructureState(InfrastructureService.power, occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence);
        ComputeProduction();
    }

    public override void ComputeProduction() //To be implemented properly after calculations and balancing are finished. For now, use the simple calculations bellow.
    {
        currentEfficiency = ComputeEfficiency();

        if (currentEfficiency < 0.001f) //aproximatly zero. This happens when minimum operational requirements are not satisified, so we we set our production(s) to zero.
        {
            currentMaxPowerProduction = 0.0f;
            currentPowerProduction = 0.0f;
            return;
        }
        
        //Efficiency affect how much is the currentMaxPowerProduction compared to plantStats.maxPowerProduction.
        currentMaxPowerProduction = Mathf.Max(currentEfficiency * plantStats.maxPowerProduction, plantStats.minPowerProduction);
        currentPowerProduction = Mathf.Max(currentMaxPowerProduction * currentLoad, plantStats.minPowerProduction);
        currentEmissionRate = (2.0f - currentEfficiency) * plantStats.baseEmissionPerPowerUnit *  currentPowerProduction; //basically, at 100% efficiecy, emission = base esmission * production.
        UpdateEmissionVisuals(currentEmissionRate);
    }

    public override float GetMaxProduction()
    {
        //return currentPowerProduction;
        return currentMaxPowerProduction;
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
public class PowerPlantStats
{
    public float maxPowerProduction = 0.0f; //unit power.
    public float minPowerProduction = 0.0f; //Minimum power, plant will not go bellow this power. unit power
    public float baseEmissionPerPowerUnit = 0.0f; //unit volume per unity time per unit power
}
