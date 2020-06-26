using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EDV_PowerPlant : ExtendedDataViewer
{
    Text emissions;
    Text currentProduction, peakProduction;
    Text load, efficiency;

    protected override void Awake()
    {
        base.Awake();
        emissions = this.transform.Find("Emissions").GetComponent<Text>();
        currentProduction = this.transform.Find("ProductionCurrent").GetComponent<Text>();
        peakProduction = this.transform.Find("ProductionPeak").GetComponent<Text>();
        load = this.transform.Find("Load").GetComponent<Text>();
        efficiency = this.transform.Find("Efficiency").GetComponent<Text>();
    }

    //TODO Set industry class after it's implemented in the IndustrialBuilding script.
    public override void SetExtendedData(Building building)
    {
        SetWorkplaceDetails(building);

        PowerPlant_1 powerPlant = building.gameObject.GetComponent<PowerPlant_1>();
        emissions.text = powerPlant.currentEmissionRate.ToString();
        currentProduction.text = powerPlant.currentPowerProduction.ToString();
        peakProduction.text = powerPlant.currentMaxPowerProduction.ToString();
        load.text = powerPlant.currentLoad.ToString();
        efficiency.text = powerPlant.currentEfficiency.ToString();
    }
}
