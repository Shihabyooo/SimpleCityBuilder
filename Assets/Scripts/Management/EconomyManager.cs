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
        Income income = new Income();
        //Since we already have WorkPlace classes holding both the number of workers AND the wages, we'll simply loop over them.
        //This approach would make it easier to, in the future, complexify the wage computation (leaving it to the Workplace class)

        foreach(WorkPlace workPlace in GameManager.buildingsMan.workPlaces)
        {
            foreach(Citizen employee in workPlace.Employees())   
                income.AddToIncome(employee.citizenClass, workPlace.Wages());
        }

        int incomeTaxes = 0;
        incomeTaxes += Mathf.RoundToInt(income.poor * incomeTaxRates.poor / 100.0f);
        incomeTaxes += Mathf.RoundToInt(income.low * incomeTaxRates.low / 100.0f);
        incomeTaxes += Mathf.RoundToInt(income.middle * incomeTaxRates.middle / 100.0f);
        incomeTaxes += Mathf.RoundToInt(income.high * incomeTaxRates.high / 100.0f);
        incomeTaxes += Mathf.RoundToInt(income.obscene * incomeTaxRates.obscene / 100.0f);

        return incomeTaxes;
    }

}


[System.Serializable]
struct TaxRates //Tax rates are per day
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


struct Income
{
    public ulong poor;
    public ulong low;
    public ulong middle;
    public ulong high;
    public ulong obscene;

    public void AddToIncome(HousingClass _class, ulong value)
    {
        switch(_class)
        {
            case HousingClass.poor:
                poor += value;
                break;
            case HousingClass.low:
                low += value;
                break;
            case HousingClass.middle:
                middle += value;
                break;
            case HousingClass.high:
                high += value;
                break;
            case HousingClass.obscene:
                obscene += value;
                break;
            default:
                break;
        }
    }
}