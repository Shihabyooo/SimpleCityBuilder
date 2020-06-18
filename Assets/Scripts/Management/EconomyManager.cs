using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] TaxRates housingTaxRates = new TaxRates();
    [SerializeField] TaxRates industryTaxRates = new TaxRates();
    [SerializeField] float commercialTaxRate = 0.0f;

    void Awake()
    {
        SimulationManager.onTimeUpdate += UpdateEconomyHour;
    }

    public void UpdateEconomyDay()
    {

    }

    void UpdateEconomyHour(int hours) //assigned to the SimulationManager.onTimeUpdate delegate.
    {
        
    }

}


[System.Serializable]
class TaxRates //Tax rates are per day
{
    float poor;
    float low;
    float middle;
    float highl;
    float obscene;
}
