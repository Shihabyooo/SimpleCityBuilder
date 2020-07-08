using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    [SerializeField] CityResources resources = new CityResources(); //The serialization is only for testing (to view parameters in editor while testing)
    [SerializeField] CityFinances finances = new CityFinances(); //The serialization is only for testing (to view parameters in editor while testing)

    public delegate void OnTreasuryChange(long newTreasury);
    public static OnTreasuryChange onTreasuryChange; 


    CityResources dailySumResources = new CityResources();
    CityFinances dailySumFinances = new CityFinances();
    int helperCounter = 0;
    ResourcesHistory resourceHistory = new ResourcesHistory();

    void Awake()
    {
        SimulationManager.onTimeUpdate += TimeProgress;
    }

    //TODO while CityResources change over the course of the day, the majority of CityFinances (as of July, 3rd implementation) are the same per day, difference is in the treasury
    //only (which could change due to player paying for constructions, etc). The current implementation in TimeProgress() and UpdateHistory() has some redundant calculations.
    //For now, leaving it as is, as there *might* be additional variables in the future in CityFinances that are are updated hourly. If that didn't turn out to be the case, this
    //is a venue for some cleanup and optimization (for prolly statistically insignificant gains, but still).

    public void TimeProgress(int hours)
    {
        dailySumResources += resources;
        dailySumFinances += finances;
        helperCounter++;
    }

    public void UpdateHistory(System.DateTime date)
    {
        //Divide the sum by the amounts added to get the average
        CityResources averageDailyResources = dailySumResources / (float)helperCounter;
        CityFinances averageDailyFinances = dailySumFinances / (float)helperCounter;

        //Add to history
        resourceHistory.AddToHistory(date, averageDailyResources, averageDailyFinances);

        //reset the averages objects and counter
        dailySumResources = new CityResources();
        dailySumFinances = new CityFinances();
        helperCounter = 0;
    }


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
    
    public TimeSeries<float> GetTimeSeries(ResourcesHistory.DataType dataType)
    {
        TimeSeries<float> ts = new TimeSeries<float>(resourceHistory.GetDates(), resourceHistory.GetAllRecordsFor(dataType));
        return ts;
    }

    public float GetLastHistoryEntry(ResourcesHistory.DataType dataType)
    {
        return resourceHistory.GetLastRecordFor(dataType);
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

        if (onTreasuryChange != null)
            onTreasuryChange.Invoke(finances.treasury);
    }

    public void SubstractFromTreasury(int newExpenses)
    {
        finances.treasury -= newExpenses;

        if (onTreasuryChange != null)
            onTreasuryChange.Invoke(finances.treasury);
    }

    public ref long Population()
    {
        return ref finances.population;
    }

    public ref IncomeTaxes IncomeTaxes()
    {
        return ref finances.incomeTaxes;
    }

    public ref int IndustryTaxes()
    {
        return ref finances.industryTaxes;
    }

    public ref int CommercialTaxes()
    {
        return ref finances.commercialTaxes;
    }

    public ref int BuildingExpenses()
    {
        return ref finances.buildingExpenses;
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
        //Rect rect = new Rect(screenWidth - dataDispWidth - 150, 50, dataDispWidth, lineHeight);
        Rect rect = new Rect(250, 50, dataDispWidth, lineHeight);

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

    public static CityResources operator+ (CityResources res1, CityResources res2)
    {
        CityResources sum = new CityResources();
        
        sum.powerDemand = res1.powerDemand + res2.powerDemand;
        sum.powerConsumption = res1.powerConsumption + res2.powerConsumption;
        sum.totalAvailablePower = res1.totalAvailablePower + res2.totalAvailablePower;
        sum.waterDemand = res1.waterDemand + res2.waterDemand;
        sum.waterConsumption = res1.waterConsumption + res2.waterConsumption;
        sum.totalAvailableWaterSupply = res1.totalAvailableWaterSupply + res2.totalAvailableWaterSupply;
        sum.studentsCount = res1.studentsCount + res2.studentsCount;
        sum.totalAvailableEducationSeats = res1.totalAvailableEducationSeats + res2.totalAvailableEducationSeats;
        sum.filledHospitalBeds = res1.filledHospitalBeds + res2.filledHospitalBeds;
        sum.totalAvailableHospitalBeds = res1.totalAvailableHospitalBeds + res2.totalAvailableHospitalBeds;
        
        sum.totalHousingSlots = res1.totalHousingSlots + res2.totalHousingSlots;
        sum.occuppiedHousingSlots = res1.occuppiedHousingSlots + res2.occuppiedHousingSlots;

        return sum;
    }

    public static CityResources operator/ (CityResources res, float denominator)
    {
        CityResources result = new CityResources();

        result.powerDemand = res.powerDemand / denominator;
        result.powerConsumption = res.powerConsumption / denominator;
        result.totalAvailablePower = res.totalAvailablePower / denominator;
        result.waterDemand = res.waterDemand / denominator;
        result.waterConsumption = res.waterConsumption / denominator;
        result.totalAvailableWaterSupply = res.totalAvailableWaterSupply / denominator;
        result.studentsCount = (uint)Mathf.RoundToInt((float)res.studentsCount / denominator);
        result.totalAvailableEducationSeats = (uint)Mathf.RoundToInt((float)res.totalAvailableEducationSeats / denominator);
        result.filledHospitalBeds = (uint)Mathf.RoundToInt((float)res.filledHospitalBeds / denominator);
        result.totalAvailableHospitalBeds = (uint)Mathf.RoundToInt((float)res.totalAvailableHospitalBeds / denominator);
        
        result.totalHousingSlots = res.totalHousingSlots / denominator;
        result.occuppiedHousingSlots = res.occuppiedHousingSlots / denominator;

        return result;
    }
}

[System.Serializable]
public class CityFinances
{
    public long population;
    public long treasury;

    //Values bellow are not used for any calculations and are solely for future statistics display to player.
    public IncomeTaxes incomeTaxes;
    public int industryTaxes;
    public int commercialTaxes;
    public int buildingExpenses;
    //TODO add other finances/economy related parameters here

    static public CityFinances operator+ (CityFinances fin1, CityFinances fin2)
    {
        CityFinances sum = new CityFinances();

        sum.population = fin1.population + fin2.population;
        sum.treasury = fin1.treasury + fin2.treasury;
        sum.incomeTaxes = fin1.incomeTaxes + fin2.incomeTaxes;
        sum.industryTaxes = fin1.industryTaxes + fin2.industryTaxes;
        sum.commercialTaxes = fin1.commercialTaxes + fin2.commercialTaxes;
        sum.buildingExpenses = fin1.buildingExpenses + fin2.buildingExpenses;
    
        return sum;
    }

    static public CityFinances operator/ (CityFinances fin, float denominator)
    {
        CityFinances result = new CityFinances();

        result.treasury = System.Convert.ToInt64(System.Convert.ToSingle(fin.treasury)/ denominator);
        result.population = System.Convert.ToInt64(System.Convert.ToSingle(fin.population)/ denominator);
        result.incomeTaxes = fin.incomeTaxes / denominator;
        
        result.industryTaxes = Mathf.RoundToInt((float)fin.industryTaxes / denominator);
        result.commercialTaxes = Mathf.RoundToInt((float)fin.commercialTaxes / denominator);
        result.buildingExpenses = Mathf.RoundToInt((float)fin.buildingExpenses / denominator);

        return result;
    }
}

[System.Serializable]
public class ResourcesHistory
{
    //Ballparking size of history in memory:
    //104 bytes for Resources + 32 bytes for finances + 1 for allignement = 137 bytes.
    //Assume date is int/int/int = 3 * 4 = 12 bytes, total  = 149 bytes, say 150 bytes.
    //for 10 years: days = 10 * 365 + 3 (worst case: three leap years) = 3653 days.
    //Total size for 10 years history = 3653 * 150 / 1024 =~ 535KB.
    //Using same initial ests, for 10000 days, size =~ 1.4MB
    //Even if my ests were off to half the actuall, those are acceptable figures....


    //TODO add PopulationManager's PopulationMetrics struct for recording.

    public struct TimePoint
    {
        public System.DateTime date;
        public CityResources resources;
        public CityFinances finances;

        public TimePoint(System.DateTime _date, CityResources _resources, CityFinances _finances)
        {
            date = _date;
            resources = _resources;
            finances = _finances;
        }
    }

    public enum DataType
    {
        undefined, population, treasury, incomeTax, industryTax, commerceTax, buildingExpense, powerDemand, powerCons, powerAvail, waterDemand, waterCons, waterAvail, studentCount, 
        eduSeat, hospitalFilled, hospitalAvail
    }

    int maxHistory = 10000; //in days. 10,000 is roughly 27 years.
    public List<TimePoint> history {get; private set;}

    public ResourcesHistory()
    {
        history = new List<TimePoint>();
    }

    public void AddToHistory(System.DateTime date, CityResources avgDailyResources, CityFinances avgDailyFinances)
    {
        if(history.Count >= maxHistory)
            history.RemoveAt(0);
        
        history.Add(new TimePoint(date, avgDailyResources, avgDailyFinances));
    }

    
    public List<System.DateTime> GetDates()
    {
        List<System.DateTime> dates = new List<System.DateTime>();

        foreach (TimePoint timePoint in history)
            dates.Add(timePoint.date);

        return dates;
    }


    public float GetNthRecordFor(DataType dataType, int order)
    {
        if (order >= history.Count)
            return 0.0f;

        switch (dataType)
        {
            case DataType.population:
                return history[order].finances.population;
            case DataType.treasury:
                return history[order].finances.treasury;
            //TODO ad remaining datatypes.
            default:
                return 0.0f;
        }
    }

    public List<float> GetAllRecordsFor(DataType dataType)
    {
        if (dataType == DataType.undefined)
            return new List<float>();

        List<float> data = new List<float>();
        
        for (int i = 0; i < history.Count; i++)
        {
            data.Add(GetNthRecordFor(dataType, i));
        }

        return data;
    }

    public float GetLastRecordFor(DataType dataType)
    {
        if (history.Count < 1)
            return 0.0f;

        return GetNthRecordFor(dataType, history.Count - 1);
    }
}