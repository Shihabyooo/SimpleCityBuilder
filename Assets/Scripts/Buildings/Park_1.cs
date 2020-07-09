using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Parks reduce pollution to some extent, and grant a minimum boost to environment happiness.

[RequireComponent(typeof(WorkPlace))]
public class Park_1 : InfrastructureBuilding
{
    [SerializeField] ParkStats parkStats;
    [SerializeField] ulong id; //TODO remove serialization after testing

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

    //Because BuildingIDHandler in Buildings manager will also be used to track whether player has reached cap of builing of specific time, we need to reserve and get an ID
    //as soon as player clicks to build a park, BeginConstruction() is called by BuildingPlan to mark this event, we override this method to add ID reserving functionality.
    public override void BeginConstruction(Cell cell)
    {
        base.BeginConstruction(cell);
        id = GameManager.buildingsMan.GetNewID(InfrastructureService.parks);
    }

    public override void UpdateEffectOnNature(int timeWindow)
    {
        base.UpdateEffectOnNature(timeWindow);
        
        float efficiency = ComputeEfficiency();
        float pollutionReduction = efficiency * (parkStats.maxPollutionReduction - parkStats.minPollutionReduction) + parkStats.minPollutionReduction;
        pollutionReduction = -1.0f * pollutionReduction; //Because we will subtract it from the current pollution

        Grid.grid.SetPollutionCummilative(occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence, pollutionReduction);
    }

    //TODO efficiency calculated similarily to standard infrascture
        //reduction of pollution (assuming minimum operation is satisfied) according to efficiency, interp between min and max.
        //Ditto to happiness boost, which is stored as a fixed value (with a public getter), and polled by each citizen's happiness update loop (in population manager).

    //TODO add a a limit to how many parks we can have (32 or 64), tracked in buildings manager and have button disabled when limit reached (or a message when attempting to
    //construct a new park after limit).
    //Then, have a GridLayer<uint> (or GridLayer<ulong> for 64 limit) each encoding which parks the said cell are serviced with, using bitwise ops.
    //This, of course, means that each park should have an ID of 2^n, n from 0 to 32 (or 64). Buildings manager also needs to keep track of IDs used (could also use bitwise 
    //ops and masks), OR'ing in an ID when a park constructed and assigned the said ID, and AND'ing it out when destroyed/removed.

    //Try to make this system as modular and generic as possible, might convert other buildings/services to follow the same patterns.


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