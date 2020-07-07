using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EDV_PowerPlant : ExtendedDataViewer
{
    Text emissions;
    Text currentProduction, peakProduction;
    Text load, efficiency;

    PowerPlant_1 viewedPlant;

    protected override void Awake()
    {
        base.Awake();
        emissions = this.transform.Find("Emissions").GetComponent<Text>();
        currentProduction = this.transform.Find("ProductionCurrent").GetComponent<Text>();
        peakProduction = this.transform.Find("ProductionPeak").GetComponent<Text>();
        load = this.transform.Find("Load").GetComponent<Text>();
        efficiency = this.transform.Find("Efficiency").GetComponent<Text>();

        //Attach button listeners to what we track for:
        currentProduction.gameObject.GetComponent<Button>().onClick.AddListener(ShowProductionHistory);
        load.gameObject.GetComponent<Button>().onClick.AddListener(ShowLoadHistory);
        efficiency.gameObject.GetComponent<Button>().onClick.AddListener(ShowEffiencyHistory);
        emissions.gameObject.GetComponent<Button>().onClick.AddListener(ShowEmissionHistory);
    }

    //TODO Set industry class after it's implemented in the IndustrialBuilding script.
    public override void SetExtendedData(Building building)
    {
        SetWorkplaceDetails(building);

        viewedPlant = building.gameObject.GetComponent<PowerPlant_1>();
        emissions.text = viewedPlant.currentEmissionRate.ToString();
        currentProduction.text = viewedPlant.currentPowerProduction.ToString();
        peakProduction.text = viewedPlant.currentMaxPowerProduction.ToString();
        load.text = viewedPlant.currentLoad.ToString();
        efficiency.text = viewedPlant.currentEfficiency.ToString();
    }


    //Button events
    void ShowProductionHistory()
    {
        GameManager.uiMan.ShowGraph(viewedPlant, InfrastructureBuilding.powerProductionTitle);
    }
    
    void ShowEmissionHistory()
    {
        GameManager.uiMan.ShowGraph(viewedPlant, InfrastructureBuilding.emissionTitle);
    }
    
    void ShowLoadHistory()
    {
        GameManager.uiMan.ShowGraph(viewedPlant, InfrastructureBuilding.loadTitle);
    }
    
    void ShowEffiencyHistory()
    {
        GameManager.uiMan.ShowGraph(viewedPlant, InfrastructureBuilding.efficiencyTitle);
    }
}
