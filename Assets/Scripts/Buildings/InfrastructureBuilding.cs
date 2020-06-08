﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfrastructureBuilding : Building
{
    [SerializeField] protected InfrastructureBuildingStats infraStats;
    [SerializeField] protected float currentLoad; //as a percentage of max production
    override protected void Awake()
    {
        base.Awake();
        stats.type = BuildingType.infrastructure;
    }

    public InfrastructureBuildingStats GetInfrastructureBuildingStats()
    {
        return infraStats;
    }

    public virtual float ComputeProduction()
    {
        return 0.0f;
    }
    
    public virtual void SetLoad(float load)
    {
        currentLoad = load;
    }

    public virtual void UpdateEffectOnNaturalResources(int timeWindow)
    {
        
    }

    

}

[System.Serializable]
public class InfrastructureBuildingStats
{
    public uint radiusOfInfluence = 0; //Measured in cells.
    public float minEfficiency = 0.2f;
    public float maxEfficiency = 1.0f;
}