using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InfrastructureService
{
    undefined = 0, water = 1, power = 2, gas = 4, education = 8, health = 16, safety = 32, parks = 64
}

[RequireComponent(typeof(WorkPlace))]
public class InfrastructureBuilding : Building
{
    public const string loadTitle = "Load"; //For use in retrieving historical data;
    public const string treatmentRateTitle = "TreatmentRate"; //For use in retrieving historical data;
    public const string efficiencyTitle = "Efficiency"; //For use in retrieving historical data;
    public const string availableGWTitle = "AvailableGroundWater"; //For use in retrieving historical data;
    public const string powerProductionTitle = "PowerProduction"; //For use in retrieving historical data;
    public const string emissionTitle = "Emission"; //For use in retrieving historical data;

    [SerializeField] protected InfrastructureBuildingStats infraStats;
    public float currentLoad {get; private set;} //as a percentage of max production
    [SerializeField] protected WorkPlace workPlace;
    [SerializeField] protected InfrastructureService infrastructureType;
    [SerializeField] protected ulong id; //Only buildings with limits to how many player can construct use this.  //TODO remove serialization after testing


    override protected void Awake()
    {
        base.Awake();
        stats.type = BuildingType.infrastructure;
        workPlace = this.gameObject.GetComponent<WorkPlace>();
    }
    
    //Because BuildingIDHandler in Buildings manager will also be used to track whether player has reached cap of builing of specific time, we need to reserve and get an ID
    //as soon as player clicks to build a park, BeginConstruction() is called by BuildingPlan to mark this event, we override this method to add ID reserving functionality.
    //Note, GameManager.buildingsMan.GetNewID(this) will only evalute to something meaningfull in a subset of infrastructure buildings (e.g. parks, schools, police stations)
    public override void BeginConstruction(Cell cell)
    {
        base.BeginConstruction(cell);
        id = GameManager.buildingsMan.GetNewID(this); 
    }

    public InfrastructureBuildingStats GetInfrastructureBuildingStats()
    {
        return infraStats;
    }

    public virtual void ComputeProduction() //computes efficiency, then current max production (based on efficiency), then current actual production (based on load)
    {

    }
    
    public virtual float GetMaxProduction() //for use by simulation manager in estimating total available capacity and assigning load based on it.
    {
        return 0.0f;
    }

    public virtual void SetLoad(float load)
    {
        currentLoad = load;
    }

    protected virtual float ComputeEfficiency()
    {
        float budgetEffect = (float)(budget - stats.minBudget) / (float) (stats.maxBudget - stats.minBudget); //linear interpolation
        budgetEffect = budgetEffect * 0.5f + 0.5f; //This limits the change in budget effect from 50% (at min budget) to 100% (at max budget)

        float manpowerEffect = 0.0f;        
        if (workPlace.MaxManpower() == 0) //building does not need manpower
            manpowerEffect = 1.0f;
        else if (workPlace.CurrentManpower() >= workPlace.MinManpower())
            manpowerEffect = (float)workPlace.CurrentManpower() / (float)workPlace.MaxManpower();

        float resourceEffect = allocatedResources.CompareToBaseResource(stats.requiredResources);
        float efficiency =  manpowerEffect * budgetEffect * resourceEffect * (infraStats.maxEfficiency - infraStats.minEfficiency) + infraStats.minEfficiency;
        
        if (manpowerEffect < 0.001f || budgetEffect < 0.001f || resourceEffect < 0.001f)
            efficiency = 0.0f;

        return efficiency;
    }

    public InfrastructureService InfrastructureType()
    {
        return infrastructureType;
    }
}

[System.Serializable]
public class InfrastructureBuildingStats
{
    public uint radiusOfInfluence = 0; //Measured in cells.
    public float minEfficiency = 0.2f;
    public float maxEfficiency = 1.0f;

}