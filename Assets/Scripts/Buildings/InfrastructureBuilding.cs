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


    //TODO rename this function to be clearer, it computes currentProduction and currentMaxProduction, and returns the latter, which is used by simulation manager to calculate
    //total supply for resource-in-question and then assigns the demand.
    public virtual float ComputeProduction()
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
        if (workPlace.CurrentManpower() >= workPlace.MinManpower())
            manpowerEffect = (float)workPlace.CurrentManpower() / (float)workPlace.MaxManpower();

        float efficiency =  manpowerEffect * budgetEffect * (infraStats.maxEfficiency - infraStats.minEfficiency);// + infraStats.minEfficiency; //linear interpolation

        if (manpowerEffect > 0.001f && budgetEffect > 0.001f)
            efficiency = Mathf.Max(efficiency, infraStats.minEfficiency);

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