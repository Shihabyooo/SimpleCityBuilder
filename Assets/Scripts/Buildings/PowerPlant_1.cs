using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPlant_1 : InfrastructureBuilding
{

    //TODO reset (uncomment) the {get; private set;} parts bellow:
    public float currentPowerProduction;// {get; private set;}
    public float currentEfficiency;// {get; private set;}
    public float currentMaxPowerProduction;// {get; private set;} //not to be confused with maxPowerProduction in PowerPlantStats. This one is variable depending on other simulation factors.
    public float currentEmissionRate; // {get; private set;} //unity volume per unit time.

    [SerializeField] PowerPlantStats plantStats = new PowerPlantStats();

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Grid.grid.SetInfrastructureState(InfrastructureService.power, occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence);
        ComputeProduction();
        GameManager.buildingsMan.AddInfrastructureBuilding(this, InfrastructureService.power);
    }

    public override float ComputeProduction() //To be implemented properly after calculations and balancing are finished. For now, use the simple calculations bellow.
    {
        float production = 0.0f;

        //Building Budget affects efficiency.
        //Efficiency affect how much is the currentMaxPowerProduction compared to plantStats.maxPowerProduction.
        
        //currentEfficiency = 0.8f;

        currentEfficiency = ComputeEfficiency();

        currentMaxPowerProduction = Mathf.Max(currentEfficiency * plantStats.maxPowerProduction, plantStats.minPowerProduction);
        production =  currentMaxPowerProduction;

        currentEmissionRate = (2.0f - currentEfficiency) * plantStats.baseEmissionPerPowerUnit *  production; //basically, at 100% efficiecy, emission = base esmission * production.

        return production;
    }


    public override void UpdateEffectOnNature(int timeWindow)    
    {
        base.UpdateEffectOnNature(timeWindow);
    }

}


[System.Serializable]
public class PowerPlantStats
{
    public float maxPowerProduction = 0.0f; //unit power.
    public float minPowerProduction = 0.0f; //Minimum power, plant will not go bellow this power. unit power
    public float baseEmissionPerPowerUnit = 0.0f; //unit volume per unity time per unit power
}
