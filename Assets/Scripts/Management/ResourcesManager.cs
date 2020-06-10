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

    public ulong AvailableHousing (HousingClass housingClass)
    {
        switch (housingClass)
        {
            case HousingClass.poor:
                return resources.totalHousingSlots.poor - resources.occuppiedHousingSlots.poor;
            case HousingClass.low:
                return resources.totalHousingSlots.low - resources.occuppiedHousingSlots.low;
            case HousingClass.middle:
                return resources.totalHousingSlots.middle - resources.occuppiedHousingSlots.middle;
            case HousingClass.high:
                return resources.totalHousingSlots.high - resources.occuppiedHousingSlots.high;
            case HousingClass.obscene:
                return resources.totalHousingSlots.obscene - resources.occuppiedHousingSlots.obscene;
            default:
                return 0;
        }
    }

    public HousingSlots AvailableHousing()
    {
        HousingSlots availableHousing = new HousingSlots();

        availableHousing.poor = resources.totalHousingSlots.poor - resources.occuppiedHousingSlots.poor;
        availableHousing.low = resources.totalHousingSlots.low - resources.occuppiedHousingSlots.low;
        availableHousing.middle = resources.totalHousingSlots.middle - resources.occuppiedHousingSlots.middle;
        availableHousing.high = resources.totalHousingSlots.high - resources.occuppiedHousingSlots.high;
        availableHousing.obscene = resources.totalHousingSlots.obscene - resources.occuppiedHousingSlots.obscene;

        return availableHousing;
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

    public void UpdateTotalHousingSlots (ulong count, HousingClass housingClass)
    {
        resources.totalHousingSlots.SetSlotValue(count, housingClass);
    }

    public void UpdateTotalHousingSlots (HousingSlots slots)
    {
        resources.totalHousingSlots.AssignNew(slots);
    }

    public void UpdateOccupiedHousingSlots (ulong count, HousingClass housingClass)
    {
        resources.occuppiedHousingSlots.SetSlotValue(count, housingClass);
    }

    public void UpdateOccupiedHousingSlots (HousingSlots slots)
    {
        resources.occuppiedHousingSlots.AssignNew(slots);
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
        message = "Total Housing Slots: poor: " + resources.totalHousingSlots.poor.ToString();
        GUI.Label(rect, message, style);
        rect.y += lineHeight + lineSpacing;
        message = "low: " + resources.totalHousingSlots.low.ToString();
        message += " | mid: " + resources.totalHousingSlots.middle.ToString();
        message += " | hi: " + resources.totalHousingSlots.high.ToString();
        message += " | obs: " + resources.totalHousingSlots.obscene.ToString();
        GUI.Label(rect, message, styleSmall);

        rect.y += lineHeight + lineSpacing;
        message = "Occuppied Housing Slots: poor: " + resources.occuppiedHousingSlots.poor.ToString();
        GUI.Label(rect, message, style);
        rect.y += lineHeight + lineSpacing;
        message = "low: " + resources.occuppiedHousingSlots.low.ToString();
        message += " | mid: " + resources.occuppiedHousingSlots.middle.ToString();
        message += " | hi: " + resources.occuppiedHousingSlots.high.ToString();
        message += " | obs: " + resources.occuppiedHousingSlots.obscene.ToString();
        GUI.Label(rect, message, styleSmall);
    }
}

[System.Serializable]
public class HousingSlots
{
    public ulong poor = 0;
    public ulong low = 0;
    public ulong middle = 0;
    public ulong high = 0;
    public ulong obscene = 0;
    

    public HousingSlots()
    {

    }

    public HousingSlots(ulong _poor, ulong _low, ulong _middle, ulong _high, ulong _obscene)
    {
        poor = _poor;
        low = _low;
        middle = _middle;
        high = _high;
        obscene = _obscene;
    }

    public HousingSlots(HousingSlots newSlots)
    {
        AssignNew(newSlots);
    }

    public void AssignNew(HousingSlots newSlots) //since we can't overload the assignment op and we sometimes want to deep copy an object
    {
        poor = newSlots.poor;
        low = newSlots.low;
        middle = newSlots.middle;
        high = newSlots.high;
        obscene = newSlots.obscene;
    }

    public void SetSlotValue(ulong value, HousingClass housingClass)
    {
        switch (housingClass)
        {
            case HousingClass.poor:
                poor = value;
                break;
            case HousingClass.low:
                low = value;
                break;
            case HousingClass.middle:
                middle = value;
                break;
            case HousingClass.high:
                high = value;
                break;
            case HousingClass.obscene:
                obscene = value;
                break;
            default:
                break;
        }
    }

    public void IncrementSlotValue(ulong increment, HousingClass housingClass)
    {
        switch (housingClass)
        {
            case HousingClass.poor:
                poor += increment;
                break;
            case HousingClass.low:
                low += increment;
                break;
            case HousingClass.middle:
                middle += increment;
                break;
            case HousingClass.high:
                high += increment;
                break;
            case HousingClass.obscene:
                obscene += increment;
                break;
            default:
                break;
        }
    }

    public ulong Sum()
    {
        //This is stupid
        // ulong sum = poor + low;
        // if (sum < poor || sum < low)
        //     return ulong.MaxValue;
        
        // sum += middle;
        // if (sum < poor + low || sum < middle)
        //     return ulong.MaxValue;

        // sum += high;
        // if (sum < poor + low + middle || sum < high)
        //     return ulong.MaxValue;
        
        // sum += obscene;
        // if (sum < poor + low + middle + high  || sum < obscene)
        //     return ulong.MaxValue;

        // return sum;


        //TODO find a way to limit the total number of housing slots to something that wouldn't cause ulong value to cycle back.
        return (poor + low + middle + high + obscene); 
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

public class CityFinances
{
    public long treasury;

    //TODO add other finances/economy related parameters here
}