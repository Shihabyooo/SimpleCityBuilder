using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    [SerializeField] CityResources resources = new CityResources(); //The serialization is only for testing (to view parameters in editor while testing)
    [SerializeField] CityFinances finances = new CityFinances(); //The serialization is only for testing (to view parameters in editor while testing)

    //Getters
    public CityResources GetCityResources()
    {
        return resources;
    }

    public float AvailablePower()
    {
        return resources.totalAvailablePower - resources.powerConsumption;
    }

    public float AvailableWater()
    {
        return resources.totalAvailableWaterSupply - resources.waterConsumption;
    }

    public ulong AvailableEducation()
    {
        if (resources.totalAvailableEducationSeats == 0 || resources.totalAvailableEducationSeats <= resources.studentsCount) //I don't trust computers, or myself....
            return 0;

        return resources.totalAvailableEducationSeats - resources.studentsCount;
    }

    public ulong AvailableHealth()
    {
        if (resources.totalAvailableHospitalBeds == 0 || resources.totalAvailableHospitalBeds <= resources.filledHospitalBeds) //Same as above.
            return 0;

        return resources.totalAvailableHospitalBeds - resources.filledHospitalBeds;    
    }

    public ulong AvailableHousing (CitizenClass housingClass)
    {
        switch (housingClass)
        {
            case CitizenClass.low:
                return resources.totalHousingSlots.low - resources.occuppiedHousingSlots.low;
            case CitizenClass.middle:
                return resources.totalHousingSlots.middle - resources.occuppiedHousingSlots.middle;
            case CitizenClass.high:
                return resources.totalHousingSlots.high - resources.occuppiedHousingSlots.high;
            default:
                return 0;
        }
    }

    public HousingSlots AvailableHousing()
    {
        HousingSlots availableHousing = new HousingSlots();

        availableHousing.low = resources.totalHousingSlots.low - resources.occuppiedHousingSlots.low;
        availableHousing.middle = resources.totalHousingSlots.middle - resources.occuppiedHousingSlots.middle;
        availableHousing.high = resources.totalHousingSlots.high - resources.occuppiedHousingSlots.high;

        return availableHousing;
    }

    public long AvailableTreasury()
    {
        return finances.treasury;
    }
    
    //Setters
    public void UpdatePowerDemand(float newDemand)
    {
        resources.powerDemand = newDemand;
    }

    public void UpdatePowerConsumption(float newConsumption)
    {
        resources.powerConsumption = newConsumption;
    }

    public void UpdateAvailablePower(float newAvailable)
    {
        resources.totalAvailablePower = newAvailable;
    }

    public void UpdateWaterDemand(float newDemand)
    {
        resources.waterDemand = newDemand;
    }

    public void UpdateWaterConsumption(float newConsumption)
    {
        resources.waterConsumption = newConsumption;
    }

    public void UpdateAvailableWater(float newAvailable)
    {
        resources.totalAvailableWaterSupply = newAvailable;
    }

    public void UpdateStudentCount(ulong newCount)
    {
        resources.studentsCount = newCount;
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
        resources.filledHospitalBeds = newBeds;
    }

    public void UpdateTotalHousingSlots (ulong count, CitizenClass housingClass)
    {
        resources.totalHousingSlots.SetSlotValue(count, housingClass);
    }

    public void UpdateTotalHousingSlots (HousingSlots slots)
    {
        resources.totalHousingSlots.AssignNew(slots);
    }

    public void UpdateOccupiedHousingSlots (ulong count, CitizenClass housingClass)
    {
        resources.occuppiedHousingSlots.SetSlotValue(count, housingClass);
    }

    public void UpdateOccupiedHousingSlots (HousingSlots slots)
    {
        resources.occuppiedHousingSlots.AssignNew(slots);
    }

    public void AddToTreasury(int newFunds)
    {
        finances.treasury += newFunds;
    }

    public void UpdateIncomeTaxes(IncomeTaxes incomeTaxes)
    {
        finances.incomeTaxes.SetIncomeTaxes(incomeTaxes);
    }

    //testing viz
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        GUIStyle styleSmall = new GUIStyle();

        style.fontSize = 20;
        styleSmall.fontSize = 17;
        int screenWidth  = Screen.width;
        
        int dataDispWidth = 250;
        int lineHeight = 21;
        int lineSpacing = 7;
        Rect rect = new Rect(screenWidth - dataDispWidth - 150, 50, dataDispWidth, lineHeight);

        string message = "Tresury: " + finances.treasury.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "[PLACEHOLDER]";
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Available Power: " + resources.totalAvailablePower.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Power Demand: " + resources.powerDemand.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Power Draw: " + resources.powerConsumption.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Water Supply: " + resources.totalAvailableWaterSupply.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Water Demand: " + resources.waterDemand.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Water Consumption: " + resources.waterConsumption.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Available Education Seats: " + resources.totalAvailableEducationSeats.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Current Students Count: " + resources.studentsCount.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Available Hospital Beds: " + resources.totalAvailableHospitalBeds.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Filled Hospital Beds: " + resources.filledHospitalBeds.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + lineSpacing;
        message = "Total Housing Slots:";
        GUI.Label(rect, message, style);
        rect.y += lineHeight + lineSpacing;
        message = "low: " + resources.totalHousingSlots.low.ToString();
        message += " | mid: " + resources.totalHousingSlots.middle.ToString();
        message += " | hi: " + resources.totalHousingSlots.high.ToString();
        GUI.Label(rect, message, styleSmall);

        rect.y += lineHeight + lineSpacing;
        message = "Occuppied Housing Slots:";
        GUI.Label(rect, message, style);
        rect.y += lineHeight + lineSpacing;
        message = "low: " + resources.occuppiedHousingSlots.low.ToString();
        message += " | mid: " + resources.occuppiedHousingSlots.middle.ToString();
        message += " | hi: " + resources.occuppiedHousingSlots.high.ToString();
        GUI.Label(rect, message, styleSmall);
    }
}

[System.Serializable]
public class CityResources
{
    public float powerDemand = 0.0f;
    public float powerConsumption = 0.0f;
    public float totalAvailablePower = 0.0f;
    public float waterDemand = 0.0f;
    public float waterConsumption = 0.0f;
    public float totalAvailableWaterSupply = 0.0f;
    public ulong studentsCount = 0;
    public ulong totalAvailableEducationSeats = 0;
    public ulong filledHospitalBeds = 0;
    public ulong totalAvailableHospitalBeds = 0;


    public HousingSlots totalHousingSlots = new HousingSlots();
    public HousingSlots occuppiedHousingSlots = new HousingSlots();
}

[System.Serializable]
public class CityFinances
{
    public long treasury;
    public IncomeTaxes incomeTaxes; //Not used for any calculations, the actuall incomeTaxes computation is local to a method in EconomyManager, this version is
                                    //solely for future statistics display to player.
    //TODO add other finances/economy related parameters here
}