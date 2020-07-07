using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EDV_WaterTreatment : ExtendedDataViewer
{
    Text availableGW;
    Text currentProduction, peakProduction;
    Text load, efficiency;

    WaterTreatmentPlant_1 viewedPlant;

    protected override void Awake()
    {
        base.Awake();
        currentProduction = this.transform.Find("ProductionCurrent").GetComponent<Text>();
        peakProduction = this.transform.Find("ProductionPeak").GetComponent<Text>();
        load = this.transform.Find("Load").GetComponent<Text>();
        efficiency = this.transform.Find("Efficiency").GetComponent<Text>();
        availableGW = this.transform.Find("AvailableGW").GetComponent<Text>();

        //Attach button listeners to what we track for:
        currentProduction.gameObject.GetComponent<Button>().onClick.AddListener(ShowProductionHistory);
        load.gameObject.GetComponent<Button>().onClick.AddListener(ShowLoadHistory);
        efficiency.gameObject.GetComponent<Button>().onClick.AddListener(ShowEffiencyHistory);
        availableGW.gameObject.GetComponent<Button>().onClick.AddListener(ShowAvailableGWHistory);
    }


    //TODO Set industry class after it's implemented in the IndustrialBuilding script.
    public override void SetExtendedData(Building building)
    {
        SetWorkplaceDetails(building);

        viewedPlant = building.gameObject.GetComponent<WaterTreatmentPlant_1>();
        currentProduction.text = viewedPlant.currentTreatmentRate.ToString();
        peakProduction.text = viewedPlant.currentMaxTreatmentRate.ToString();
        load.text = viewedPlant.currentLoad.ToString();
        efficiency.text = viewedPlant.currentEfficiency.ToString();
        availableGW.text = viewedPlant.availableGroundWater.ToString();
    }


    //Button events
    void ShowProductionHistory()
    {
        GameManager.uiMan.ShowGraph(viewedPlant, InfrastructureBuilding.powerProductionTitle);
    }
        
    void ShowLoadHistory()
    {
        GameManager.uiMan.ShowGraph(viewedPlant, InfrastructureBuilding.loadTitle);
    }
    
    void ShowEffiencyHistory()
    {
        GameManager.uiMan.ShowGraph(viewedPlant, InfrastructureBuilding.efficiencyTitle);
    }

    void ShowAvailableGWHistory()
    {
        GameManager.uiMan.ShowGraph(viewedPlant, InfrastructureBuilding.availableGWTitle);
    }
}
