using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfrastructureBuilding : Building
{
    [SerializeField] protected InfrastructureBuildingStats infraStats;

    void Awake()
    {
        stats.type = BuildingType.infrastructure;
    }

    public InfrastructureBuildingStats GetInfrastructureBuildingStats()
    {
        return infraStats;
    }

    public virtual void UpdateCityResources()
    {

    }
    

}

[System.Serializable]
public class InfrastructureBuildingStats
{
    public uint radiusOfInfluence = 0; //Measured in cells.
}