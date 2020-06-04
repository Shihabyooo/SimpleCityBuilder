using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindFarm_1 : InfrastructureBuilding
{
    //TODO reset (uncomment) the {get; private set;} parts bellow:
    public float currentPowerProduction;// {get; private set;}
    public float currentEfficiency;// {get; private set;}
    public float currentMaxPowerProduction;// {get; private set;} //not to be confused with maxPowerProduction in WindFarmStats. This one is variable depending on other simulation factors.

    [SerializeField] WindFarmStats farmStats = new WindFarmStats();

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Grid.grid.SetInfrastructureState(InfrastructureService.power, occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence);
        ComputeCurrentProduction();
        GameManager.buildingsMan.AddInfrastructureBuilding(this, InfrastructureService.power);
        //UpdateCityResources();
    }

    float ComputeCurrentProduction() //To be implemented properly after calculations and balancing are finished. For now, use the simple calculations bellow.
    {
        float production = 0.0f;

        //Building Budget affects efficiency.
        //Wind speed affects efficiency.
        //Efficiency affect how much is the currentMaxPowerProduction compared to plantStats.maxPowerProduction.

        currentEfficiency = 0.8f;

        float cellWindSpeed = Grid.grid.windSpeedLayer.GetCellValue(occupiedCell[0], occupiedCell[1]);
        uint cellWindDirection = Grid.grid.windDirectionLayer.GetCellValue(occupiedCell[0], occupiedCell[1]);
        
        //Setting orientation of buildings is not yet implemented, so no adjustment based on wind direction will be implemented now.
        if (cellWindSpeed >= farmStats.minWindSpeed)
            currentEfficiency = currentEfficiency * Mathf.Clamp(cellWindSpeed / farmStats.maxWindSpeed, 0.0f, 1.0f);
        else
            currentEfficiency = 0.0f;
        
        currentMaxPowerProduction = currentEfficiency * farmStats.maxPowerProduction;
        production =  currentMaxPowerProduction;
        
        return production;
    }
}

[System.Serializable]
public class WindFarmStats
{
    public float maxPowerProduction = 0.0f; //unit power.
    public float minWindSpeed = 0.0f; //unit speed (adjusted with direction), minimum speed for the farm to operate.
    public float maxWindSpeed = 0.0f; //unit speed (adjusted with direciton), maximum operatational wind speed, any excess speed will not generate any more power.

}
