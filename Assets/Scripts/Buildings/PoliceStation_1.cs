using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorkPlace))]
public class PoliceStation_1 : InfrastructureBuilding
{
    [SerializeField] PoliceStationStats stationStats;
    public uint crimeFightingCapacity {get; private set;}

   override protected void Awake()
    {
        base.Awake();
        infrastructureType = InfrastructureService.safety;
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.police);
    }

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Grid.grid.SetInfrastructureLayerCoverageByID(occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence, InfrastructureService.safety, id);
    }

    public override void UpdateEffectOnNature(int timeWindow) //Update effect on nature is called every sim update, using it here to recalculate crimeFightingCapacity without
    {                                                         //adding more calls to sim manager.
        crimeFightingCapacity = workPlace.CurrentManpower() * crimeFightingCapacity;
    }
}


[System.Serializable]
public class PoliceStationStats
{
    public uint crimeFightingCapacityPerOfficer = 10;
}