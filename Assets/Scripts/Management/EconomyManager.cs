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
    }

    void UpdateEconomyHour(int hours) //assigned to the SimulationManager.onTimeUpdate delegate.
    {
        //TODO compute expenses here        
    }

    
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

        GameManager.resourceMan.UpdateIncomeTaxes(incomeTaxes);
        return incomeTaxes.Sum();
    }

}


[System.Serializable]
public struct TaxRates //Tax rates are per day
{
    public const float minTaxRate = 1.0f;
    public const float maxTaxRate = 30.0f;

    //TODO uncomment the {get; private set;} when testing is done.
    [Range(minTaxRate, maxTaxRate)] public float poor; //{get; private set;}
    [Range(minTaxRate, maxTaxRate)] public float low; //{get; private set;}
    [Range(minTaxRate, maxTaxRate)] public float middle; //{get; private set;}
    [Range(minTaxRate, maxTaxRate)] public float high; //{get; private set;}
    [Range(minTaxRate, maxTaxRate)] public float obscene; //{get; private set;}

    public TaxRates(float fixedValue)
    {
        poor = low = middle = high = obscene = fixedValue;
    }

    public TaxRates(float _poor, float _low, float _middle, float _high, float _obscene)
    {
        poor = _poor;
        low = _low;
        middle = _middle;
        high = _high;
        obscene = _obscene;
    }

    public TaxRates(TaxRates taxRates)
    {
        poor = taxRates.poor;
        low = taxRates.low;
        middle = taxRates.middle;
        high = taxRates.high;
        obscene = taxRates.obscene;
    }

    public void SetRate(HousingClass _class, float rate) //rate will be clamped to minTaxRate and maxTaxRate
    {
        rate = Mathf.Clamp(rate, minTaxRate, maxTaxRate);

        switch (_class)
        {
            case HousingClass.poor:
                poor = rate;
                break;
            case HousingClass.low:
                low = rate;
                break;
            case HousingClass.middle:
                middle = rate;
                break;
            case HousingClass.high:
                high = rate;
                break;
            case HousingClass.obscene:
                obscene = rate;
                break;
            default:
                break;
        }
    }
}


[System.Serializable]
public struct IncomeTaxes
{
    public int poor;
    public int low;
    public int middle;
    public int high;
    public int obscene;

    public int AddToIncomeTaxes(HousingClass _class, int baseIncome, TaxRates taxRates)
    {
        int newAddition = 0;
        switch(_class)
        {
            case HousingClass.poor:
                newAddition = Mathf.RoundToInt((float)baseIncome * taxRates.poor);
                poor += newAddition;
                break;
            case HousingClass.low:
                newAddition = Mathf.RoundToInt((float)baseIncome * taxRates.low);
                low += newAddition;
                break;
            case HousingClass.middle:
                newAddition = Mathf.RoundToInt((float)baseIncome * taxRates.middle);
                middle += newAddition;
                break;
            case HousingClass.high:
                newAddition = Mathf.RoundToInt((float)baseIncome * taxRates.high);
                high += newAddition;
                break;
            case HousingClass.obscene:
                newAddition = Mathf.RoundToInt((float)baseIncome * taxRates.obscene);
                obscene += newAddition;
                break;
            default:
                break;
        }

        return newAddition;
    }

    public int Sum()
    {
        return poor + low + middle + high + obscene; //TODO consider integer limits.
    }

    public void SetIncomeTaxes(IncomeTaxes newIncomeTaxes)
    {
        poor = newIncomeTaxes.poor;
        low = newIncomeTaxes.low;
        middle = newIncomeTaxes.middle;
        high = newIncomeTaxes.high;
        obscene = newIncomeTaxes.obscene;
    }


}