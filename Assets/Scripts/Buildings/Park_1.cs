using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Parks reduce pollution to some extent, and grant a minimum boost to environment happiness.

[RequireComponent(typeof(WorkPlace))]
public class Park_1 : InfrastructureBuilding
{
    [SerializeField] ParkStats parkStats;

    public uint environmentHappinessBoost {get; private set;}

    override protected void Awake()
    {
        base.Awake();
        infrastructureType = InfrastructureService.parks;
        environmentHappinessBoost = 0;
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.park);
    }

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Grid.grid.SetInfrastructureLayerCoverageByID(occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence, InfrastructureService.parks, id);
    }

    public override void UpdateEffectOnNature(int timeWindow) //Update effect on nature is called every sim update. So we can use it to calculate environment boost as well (less code)
    {
        base.UpdateEffectOnNature(timeWindow);
        
        float efficiency = ComputeEfficiency();
        float pollutionReduction = efficiency * (parkStats.maxPollutionReduction - parkStats.minPollutionReduction) + parkStats.minPollutionReduction;
        pollutionReduction = -1.0f * pollutionReduction; //Because we will subtract it from the current pollution

        Grid.grid.SetPollutionCummilative(occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence, pollutionReduction);

        CalculateEnvironmentHappinessBoost();
    }

    void CalculateEnvironmentHappinessBoost()
    {
        float efficiency = ComputeEfficiency();
        environmentHappinessBoost = (uint)Mathf.RoundToInt(efficiency * (float)(parkStats.maxEnvironmentHappinessBoost - parkStats.minEnvironmentHappinessBoost) + parkStats.minEnvironmentHappinessBoost);
    }

    static public uint EnvironmentBoostMax(uint cellID_x, uint cellID_y) //Requires an active Grid and BuildingsManager (which should always exist in this game, anyway)
    {   
        int maxBoost = 0;

        List<ulong> coveringParks = Grid.grid.GetInfrastructureIDsCoveringCell(cellID_x, cellID_y, InfrastructureService.parks);

        foreach (ulong id in coveringParks)
        {
            Park_1 park = (Park_1)GameManager.buildingsMan.GetInfrastructureBuilding(InfrastructureService.parks, id);
            
            if (park != null)
                maxBoost = Mathf.Max((int)park.environmentHappinessBoost, maxBoost);
        }

        return (uint)maxBoost;
    }
}

[System.Serializable]
public class ParkStats
{
    [Range(0, 4)] public uint minEnvironmentHappinessBoost;
    [Range(6, 50)] public uint maxEnvironmentHappinessBoost;

    //[Range(0, 4)]
    public float minPollutionReduction;
    //[Range(6, 50)]
    public float maxPollutionReduction;
}