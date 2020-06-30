using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    //[SerializeField] TaxRates housingTaxRates = new TaxRates(1.0f);
    [SerializeField] TaxRates incomeTaxRates = new TaxRates(1.0f);
    //[SerializeField] TaxRates industryTaxRates = new TaxRates(1.0f);
    //[SerializeField] TaxRates commercialTaxRate = new TaxRates(1.0f);


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

        return taxes;
    }

}

[System.Serializable]
public struct TaxRates //Tax rates are per day
{
    public const float minTaxRate = 1.0f;
    public const float maxTaxRate = 30.0f;

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
}
