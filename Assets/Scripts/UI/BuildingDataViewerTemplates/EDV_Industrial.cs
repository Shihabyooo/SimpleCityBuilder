using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EDV_Industrial : ExtendedDataViewer
{
    Text industrialClass;
    Text emissions, production;

    protected override void Awake()
    {
        base.Awake();
        industrialClass = this.transform.Find("Class").GetComponent<Text>();
        emissions = this.transform.Find("Emissions").GetComponent<Text>();
        production = this.transform.Find("Production").GetComponent<Text>();
    }

    //TODO Set industry class after it's implemented in the IndustrialBuilding script.
    public override void SetExtendedData(Building building)
    {
        SetWorkplaceDetails(building);

        IndustrialBuilding industry = building.gameObject.GetComponent<IndustrialBuilding>();
        emissions.text = industry.currentEmissionRate.ToString();
        production.text = industry.currentProduction.ToString();
    }
}
