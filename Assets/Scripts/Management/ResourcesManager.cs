using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    [SerializeField] CityResources resources = new CityResources(); //The serialization is only for testing (to view parameters in editor while testing)
    [SerializeField] CityFinances finances = new CityFinances(); //The serialization is only for testing (to view parameters in editor while testing)

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
        return resources.totalAvailableWaterSupply - resources.currentWaterConsumption;
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


    //Setters
    public void UpdatePowerDraw(float newDraw)
    {
        resources.currentPowerDraw = newDraw;
    }

    public void UpdateAvailablePower(float newAvailable)
    {
        resources.totalAvailablePower = newAvailable;
    }

    public void UpdateWaterConsumption(float newConsumption)
    {
        resources.currentWaterConsumption = newConsumption;
    }

    public void UpdateWaterSupply(float newSupply)
    {
        resources.totalAvailableWaterSupply = newSupply;
    }

    public void UpdateStudentCount(ulong newCount)
    {
        resources.currentStudentsCount = newCount;
    }

    public void UpdateAvailableEducationSeats(ulong newSeats)
    {
        resources.totalAvailableEducationSeats = newSeats;
    }

    public void UpdateAvailableHospitalBeds(ulong newBeds)
    {
        resources.totalAvailableHospitalBeds = newBeds;
    }

    public void UpdateFilledHospitalBeds(ulong newBeds)
    {
        resources.currentFilledHospitalBeds = newBeds;
    }


    //testing viz
    void OnGUI()
    {
        
        int screenWidth  = Screen.width;
        
        int dataDispWidth = 250;
        int lineHeight = 20;
        Rect rect = new Rect(screenWidth - dataDispWidth - 130, 50, dataDispWidth, lineHeight);

        string message = "Tresury: " + finances.treasury.ToString();
        GUI.Label(rect, message);

        rect.y += lineHeight + 5;
        message = "[PLACEHOLDER]";
        GUI.Label(rect, message);

        rect.y += lineHeight + 5;
        message = "Available Power: " + resources.totalAvailablePower.ToString();
        GUI.Label(rect, message);

        rect.y += lineHeight + 5;
        message = "Power Draw: " + resources.currentPowerDraw.ToString();
        GUI.Label(rect, message);

        rect.y += lineHeight + 5;
        message = "Water Supply: " + resources.totalAvailableWaterSupply.ToString();
        GUI.Label(rect, message);

        rect.y += lineHeight + 5;
        message = "Available Education Seats: " + resources.totalAvailableEducationSeats.ToString();
        GUI.Label(rect, message);

        rect.y += lineHeight + 5;
        message = "Current Students Count: " + resources.currentStudentsCount.ToString();
        GUI.Label(rect, message);

        rect.y += lineHeight + 5;
        message = "Available Hospital Beds: " + resources.totalAvailableHospitalBeds.ToString();
        GUI.Label(rect, message);

        rect.y += lineHeight + 5;
        message = "Filled Hospital Beds: " + resources.currentFilledHospitalBeds.ToString();
        GUI.Label(rect, message);
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

public class CityFinances
{
    public long treasury;

    //TODO add other finances/economy related parameters here
}