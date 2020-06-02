﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    [SerializeField] CityResources resources = new CityResources(); //The serialization is only for testing (to view parameters in editor while testing)


    public CityResources GetCityResources()
    {
        return resources;
    }

    public float AvailablePower()
    {
        return resources.totalAvailablePower - resources.currentPowerDraw;
    }

    public float AvailableWater()
    {
        return resources.totalAvailablePower - resources.currentPowerDraw;
    }

    public ulong AvailableEducation()
    {
        if (resources.totalAvailableEducationSeats == 0 || resources.totalAvailableEducationSeats <= resources.currentStudentsCount) //I don't trust computers, or myself....
            return 0;

        return resources.totalAvailableEducationSeats - resources.currentStudentsCount;
    }

    public ulong AvailableHealth()
    {
        if (resources.totalAvailableHospitalBeds == 0 || resources.totalAvailableHospitalBeds <= resources.currentFilledHospitalBeds) //Same as above.
            return 0;

        return resources.totalAvailableHospitalBeds - resources.currentFilledHospitalBeds;    
    }
}


[System.Serializable]
public class CityResources
{
    public float currentPowerDraw = 0.0f;
    public float totalAvailablePower = 0.0f;
    public float currentWaterConsumption = 0.0f;
    public float totalAvailableWaterSupply = 0.0f;
    public ulong currentStudentsCount = 0;
    public ulong totalAvailableEducationSeats = 0;
    public ulong currentFilledHospitalBeds = 0;
    public ulong totalAvailableHospitalBeds = 0;
}