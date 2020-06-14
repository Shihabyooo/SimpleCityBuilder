using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorkPlace))]
public class InfrastructureBuilding : Building
{
    [SerializeField] protected InfrastructureBuildingStats infraStats;
    [SerializeField] protected float currentLoad; //as a percentage of max production //TODO remove serialization when testing is done.
    [SerializeField] protected WorkPlace workPlace;

    override protected void Awake()
    {
        base.Awake();
        stats.type = BuildingType.infrastructure;
        workPlace = this.gameObject.GetComponent<WorkPlace>();
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
        
        float manpowerEffect = 0.0f;        
        if (workPlace.MaxManpower() == 0) //building does not need manpower
            manpowerEffect = 1.0f;
        else if (workPlace.CurrentManpower() >= workPlace.MinManpower())
            manpowerEffect = (float)workPlace.CurrentManpower() / (float)workPlace.MaxManpower();


        float resourceEffect = allocatedResources.CompareToBaseResource(stats.requiredResources);

        float efficiency =  manpowerEffect * budgetEffect * resourceEffect * (infraStats.maxEfficiency - infraStats.minEfficiency) + infraStats.minEfficiency;
        
        if (manpowerEffect < 0.001f || budgetEffect < 0.001f || resourceEffect < 0.001f)
            efficiency = 0.0f;

        print (this.gameObject.name + ", manpowereffect: " + manpowerEffect + ", resourceEffect: " + resourceEffect + ", budgetEffect: " + budgetEffect + ", total effeicienct: " + efficiency);

        return efficiency;
    }

}

[System.Serializable]
public class InfrastructureBuildingStats
{
    public uint radiusOfInfluence = 0; //Measured in cells.
    public float minEfficiency = 0.2f;
    public float maxEfficiency = 1.0f;

}