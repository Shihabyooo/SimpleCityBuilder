using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    //[SerializeField] TaxRates housingTaxRates = new TaxRates(0.01f);
    [SerializeField] TaxRates incomeTaxRates = new TaxRates(0.01f);
    //[SerializeField] TaxRates industryTaxRates = new TaxRates(0.01f);
    //[SerializeField] TaxRates commercialTaxRates = new TaxRates(0.01f);
    [SerializeField] float industryTaxRate = 0.01f;
    [SerializeField] float commercialTaxRate = 0.01f;

    void Awake()
    {
        SimulationManager.onTimeUpdate += UpdateEconomyHour;
    }

    public void UpdateEconomyDay()
    {
        GameManager.resourceMan.AddToTreasury(ComputeTaxes());
        GameManager.resourceMan.SubstractFromTreasury(ComputeCityExpenses());
    }

    void UpdateEconomyHour(int hours) //assigned to the SimulationManager.onTimeUpdate delegate.
    {
        //TODO compute expenses here        
    }

    
    //City expenses
    int ComputeCityExpenses()
    {
        //Add all budgets and return them
        int totalExpenses = ComputeBuildingsBudget();
        //TODO add other running city expenditure.

        return totalExpenses;
    }

    int ComputeBuildingsBudget()
    {
        int buildingsBudget = 0;
        foreach (Building building in GameManager.buildingsMan.constructedBuildings)
        {
            buildingsBudget += (int)building.Budget();
        }

        //Update metrics
        GameManager.resourceMan.BuildingExpenses() = buildingsBudget;

        return buildingsBudget;
    }

    //Taxes
    int ComputeTaxes()
    {
        int taxes = 0;
        taxes += ComputeIncomeTaxes();
        taxes += ComputeIndustryTaxes();
        taxes += ComputeCommercialTaxes();
        //TODO add remaining forms of taxes here.
        return taxes;
    }

    int ComputeIncomeTaxes()
    {
        IncomeTaxes incomeTaxes = new IncomeTaxes();
        //Since we already have WorkPlace classes holding both the number of workers AND the wages, we'll simply loop over them.
        //This approach would make it easier to, in the future, complexify the wage computation (leaving it to the Workplace class)

        foreach(WorkPlace workPlace in GameManager.buildingsMan.workPlaces)
        {
            foreach(Citizen employee in workPlace.Employees())   
            {
                int employeeTax = incomeTaxes.AddToIncomeTaxes(employee.citizenClass, (int)workPlace.Wages(), incomeTaxRates); //AddToIncomeTaxes() returns amount that was added.
                employee.SubstractTax(employeeTax);
            }
        }
        
        //Update metrics
        GameManager.resourceMan.IncomeTaxes() = incomeTaxes;

        return incomeTaxes.Sum();
    }

    int ComputeIndustryTaxes()
    {
        int taxes = 0;
        long totalProfits = 0;

        foreach (IndustrialBuilding industrialBuilding in GameManager.buildingsMan.industrialBuildings)
        {
            totalProfits += industrialBuilding.ComputeProduction();
        }

        taxes = Mathf.RoundToInt(industryTaxRate * totalProfits);
        
        //update metrics
        GameManager.resourceMan.IndustryTaxes() = taxes;

        return taxes;
    }

    int ComputeCommercialTaxes()
    {
        int taxes = 0;
        long totalProfits = 0;

        foreach (CommercialBuilding commercialBuilding in GameManager.buildingsMan.commercialBuildings)
        {
            //TODO finish this
        }

        taxes = Mathf.RoundToInt(commercialTaxRate * totalProfits);

        //update metrics
        GameManager.resourceMan.CommercialTaxes() = taxes;

        return taxes;
    }

    //One time expenses (e.g. construction costs checks and substraction)
    public bool CanAfford(int cost)
    {
        if (GameManager.resourceMan.AvailableTreasury() - cost < 0)
            return false;
        else
            return true;
    }

    public bool Pay(int cost) //Returns false if no sufficient funds are available.
    {
        if (CanAfford(cost))
        {
            GameManager.resourceMan.SubstractFromTreasury(cost);
            return true;
        }
        else
            return false;
    }

}

[System.Serializable]
public struct TaxRates //Tax rates are per day
{
    public const float minTaxRate = 0.01f;
    public const float maxTaxRate = 0.3f;

    //TODO uncomment the {get; private set;} when testing is done.
    [Range(minTaxRate, maxTaxRate)] public float low; //{get; private set;}
    [Range(minTaxRate, maxTaxRate)] public float middle; //{get; private set;}
    [Range(minTaxRate, maxTaxRate)] public float high; //{get; private set;}


    public TaxRates(float fixedValue)
    {
        low = middle = high = fixedValue;
    }

    public TaxRates(float _low, float _middle, float _high)
    {
        low = _low;
        middle = _middle;
        high = _high;
    }

    public TaxRates(TaxRates taxRates)
    {
        low = taxRates.low;
        middle = taxRates.middle;
        high = taxRates.high;
    }

    public void SetRate(CitizenClass _class, float rate) //rate will be clamped to minTaxRate and maxTaxRate
    {
        rate = Mathf.Clamp(rate, minTaxRate, maxTaxRate);

        switch (_class)
        {
            case CitizenClass.low:
                low = rate;
                break;
            case CitizenClass.middle:
                middle = rate;
                break;
            case CitizenClass.high:
                high = rate;
                break;
            default:
                break;
        }
    }
}

[System.Serializable]
public struct IncomeTaxes
{
    public int low;
    public int middle;
    public int high;

    public int AddToIncomeTaxes(CitizenClass _class, int baseIncome, TaxRates taxRates)
    {
        int newAddition = 0;
        switch(_class)
        {
            case CitizenClass.low:
                newAddition = Mathf.RoundToInt((float)baseIncome * taxRates.low);
                low += newAddition;
                break;
            case CitizenClass.middle:
                newAddition = Mathf.RoundToInt((float)baseIncome * taxRates.middle);
                middle += newAddition;
                break;
            case CitizenClass.high:
                newAddition = Mathf.RoundToInt((float)baseIncome * taxRates.high);
                high += newAddition;
                break;
            default:
                break;
        }

        return newAddition;
    }

    public int Sum()
    {
        return low + middle + high; //TODO consider integer limits.
    }

    public void SetIncomeTaxes(IncomeTaxes newIncomeTaxes)
    {
        low = newIncomeTaxes.low;
        middle = newIncomeTaxes.middle;
        high = newIncomeTaxes.high;
    }

    static public IncomeTaxes operator+ (IncomeTaxes in1, IncomeTaxes in2)
    {
        IncomeTaxes sum = new IncomeTaxes();

        sum.low = in1.low + in2.low;
        sum.middle = in1.middle + in2.middle;
        sum.high = in1.high + in2.high;

        return sum;
    }

    static public IncomeTaxes operator/ (IncomeTaxes inc, float denominator)
    {
        IncomeTaxes result = new IncomeTaxes();

        result.low = Mathf.RoundToInt((float)inc.low / denominator);
        result.middle = Mathf.RoundToInt((float)inc.middle / denominator);
        result.high = Mathf.RoundToInt((float)inc.high / denominator);

        return result;
    }
}
