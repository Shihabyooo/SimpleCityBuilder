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
        ComputeProduction();
        GameManager.buildingsMan.AddInfrastructureBuilding(this, InfrastructureService.power);
    }

    public override void ComputeProduction()  //To be implemented properly after calculations and balancing are finished. For now, use the simple calculations bellow.
    {
        currentEfficiency = ComputeEfficiency();
        
        currentMaxPowerProduction = currentEfficiency * farmStats.maxPowerProduction;
        currentPowerProduction = currentMaxPowerProduction * currentLoad;
    }

    public override float GetMaxProduction() 
    {
        return currentMaxPowerProduction;
    }

    protected override float ComputeEfficiency()
    {
        //TODO adjust windspeed for direction here.
        float cellWindSpeed = Grid.grid.windSpeedLayer.GetCellValue(occupiedCell[0], occupiedCell[1]);
        uint cellWindDirection = Grid.grid.windDirectionLayer.GetCellValue(occupiedCell[0], occupiedCell[1]);

        float efficiency = base.ComputeEfficiency() *  Mathf.Clamp(cellWindSpeed / farmStats.maxWindSpeed, 0.0f, 1.0f);

        if (efficiency > 0.001f)
            efficiency = Mathf.Max(efficiency, infraStats.minEfficiency);

        return efficiency;
    }

    public override void UpdateEffectOnNature(int timeWindow)
    {
        base.UpdateEffectOnNature(timeWindow);
        //TODO wind farms should reduce wind speed downstream (slightly).
    }


}

[System.Serializable]
public class WindFarmStats
{
    public float maxPowerProduction = 0.0f; //unit power.
    public float minWindSpeed = 0.0f; //unit speed (adjusted with direction), minimum speed for the farm to operate.
    public float maxWindSpeed = 0.0f; //unit speed (adjusted with direciton), maximum operatational wind speed, any excess speed will not generate any more power.

}
