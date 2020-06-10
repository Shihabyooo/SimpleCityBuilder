using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidentialBuilding : Building
{
    [SerializeField] ResidentialBuildingStats residentStats = new ResidentialBuildingStats();
    protected List<System.Guid> occupants;
    public uint housingQuality;

   protected override void Awake()
   {
       base.Awake();
       occupants = new List<System.Guid>();
       stats.type = BuildingType.residential;
   }

    public virtual void UpdateHousingQuality()
    {
        //check the allocated resources vs required resources
        //Multiplying seperate resource ratios has an effect that, insufficiency in one resource would still significantly impact quality even if all other resources are maxmized.
        //In the future, this estimate could be changed to a weighted average.
        float resourceRatio = (allocatedResources.power / stats.requiredResources.power) * (allocatedResources.water / stats.requiredResources.water);

        housingQuality = (uint)Mathf.RoundToInt(Mathf.Max(resourceRatio * residentStats.maxHousingQuality, residentStats.minHousingQuality));
    }

    public virtual bool AddResident(System.Guid citizenID)
    {
        if (IsFull())
            return false;

        occupants.Add(citizenID);

        return true;        
    }

    public virtual bool RemoveResident(System.Guid citizenID) //returns false if no resident of citizenID is found, true otherwise.
    {
        for (int i = occupants.Count - 1; i >= 0; i--)
        {
            if (occupants[i] == citizenID)
            {
                occupants.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public bool IsFull()
    {
        return occupants.Count >= residentStats.residentCapacity;
    }

    public uint CountEmptyHousingSlots()
    {
        return (uint)Mathf.Max((long)residentStats.residentCapacity - occupants.Count, 0);
    }

    public uint HousingCapacity()
    {
        return residentStats.residentCapacity;
    }

    public uint CountResidents()
    {
        return (uint)occupants.Count;
    }

    public HousingClass ResidentClass()
    {
        return residentStats.housingClass;
    }
}


public enum HousingClass
{
    poor, low, middle, high, obscene
}

[System.Serializable]
public class ResidentialBuildingStats
{
    public uint minHousingQuality = 5;
    public uint maxHousingQuality = 100;
    public HousingClass housingClass = HousingClass.middle;
    public uint residentCapacity = 100;
    
}
