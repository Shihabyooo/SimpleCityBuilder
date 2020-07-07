using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindFarm_1 : InfrastructureBuilding
{
    float currentPowerProduction;
    float currentEfficiency;
    float currentMaxPowerProduction; //not to be confused with maxPowerProduction in WindFarmStats. This one is variable depending on other simulation factors.

    [SerializeField] WindFarmStats farmStats = new WindFarmStats();

    protected override void Awake()
    {
        base.Awake();
        infrastructureType = InfrastructureService.power;
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.windFarm);
    }

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Grid.grid.SetInfrastructureState(InfrastructureService.power, occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence);
        ComputeProduction();
    }

     protected override void InitializeHistory()
    {
        string[] dataToBeSaved = {loadTitle, powerProductionTitle, efficiencyTitle};
        buildingHistory = new BuildingHistory(dataToBeSaved, constructionDate);
    }

    protected override void UpdateDailyAverage()
    {
        //IMPORTANT: Must make sure number of elements in lastTimePointData = buildingHistory.dataCount, itself = dataToBeSaved count set in InitializeHistory. Same for order.
        float[] lastTimePointData = {currentLoad, currentPowerProduction, currentEfficiency};
        dailyAverages.Add(new BuildingHistory.TimePoint(lastTimePointData));
    }

    public override void ComputeProduction()  //To be implemented properly after calculations and balancing are finished. For now, use the simple calculations bellow.
    {
        currentEfficiency = ComputeEfficiency();
        currentMaxPowerProduction = currentEfficiency * farmStats.maxPowerProduction;
        currentPowerProduction = currentMaxPowerProduction * currentLoad;
        
        UpdateDailyAverage();
    }

    protected override float ComputeEfficiency()
    {
        float cellWindSpeed = Grid.grid.windSpeedLayer.GetCellValue(occupiedCell[0], occupiedCell[1]);
        int cellWindDirection = (int)Grid.grid.windDirectionLayer.GetCellValue(occupiedCell[0], occupiedCell[1]); //Wind direction should be limited between 0 and 360, this casting should be safe.
        //  Note that localRot returns value from 0 to 360 always.
        //The code bellow assumes that localRot.y = 0 for a a wind farm model facing north (equivalent to windDir = 270)
        //Note that "turbineFacing" bellow calculates the inverse facing, that is, the direction the end of the turbine is pointing at.

        int turbineFacing = Mathf.RoundToInt( this.transform.eulerAngles.y);// - 270;  //TODO remember to adjust this 90 value if changed the wind farm's model 0 rotation facing.
        //print (this.gameObject.name + "  || facing: " + turbineFacing + ", localRot: " + this.transform.eulerAngles.y); //test
        int angleDifference = (Mathf.Max(cellWindDirection, Mathf.Abs(turbineFacing - 90)) - Mathf.Min(cellWindDirection, Mathf.Abs(turbineFacing - 90))) % 360;
        angleDifference = angleDifference > 180? 360 - angleDifference : angleDifference;
        
        if (cellWindSpeed < farmStats.minWindSpeed || angleDifference > farmStats.maxWindSpeed)
        {
            //print ("returning 0.0f for farm " + this.gameObject.name + ", difference: " + angleDifference); //test
            return 0.0f;
        }

        float windDirectionEffect = (Mathf.Max(angleDifference, farmStats.maxPeakPerformanceAngleDifference) - (float)farmStats.maxOperationalAngleDifference) / ((float)farmStats.maxPeakPerformanceAngleDifference - (float)farmStats.maxOperationalAngleDifference) ;
        float windSpeedEffect = Mathf.Min(cellWindSpeed / farmStats.maxWindSpeed, 1.0f);
        float efficiency = windDirectionEffect * windSpeedEffect * base.ComputeEfficiency();

        if (efficiency > 0.001f)
            efficiency = Mathf.Max(efficiency, infraStats.minEfficiency);

        //print (this.gameObject.name + "  || windDirectionEffect: " + windDirectionEffect + ", difference: " + angleDifference); //test
        return efficiency;
    }

    public override float GetMaxProduction() 
    {
        return currentMaxPowerProduction;
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
    public uint maxPeakPerformanceAngleDifference = 5; //in degrees, the angle difference between turbine facing and wind that would generate max performance, and greater difference reduces performance.
    public uint maxOperationalAngleDifference = 55; //the maximum angle difference that the turbine can operate at. Any greater difference and the turbine won't operate.
}
