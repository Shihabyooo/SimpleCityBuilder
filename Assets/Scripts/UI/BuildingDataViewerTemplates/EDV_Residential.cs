using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EDV_Residential : ExtendedDataViewer
{
    Text residenceClass;
    Text capacity, occupancy;
    Text quality;
    Text rent;

    protected override void Awake()
    {
        base.Awake();
        residenceClass = this.transform.Find("Class").GetComponent<Text>();
        capacity = this.transform.Find("Capacity").GetComponent<Text>();
        occupancy = this.transform.Find("Occupancy").GetComponent<Text>();
        quality = this.transform.Find("Quality").GetComponent<Text>();
        rent = this.transform.Find("Rent").GetComponent<Text>();
    }

    public override void SetExtendedData(Building building)
    {
        ResidentialBuilding residence = building.gameObject.GetComponent<ResidentialBuilding>();
        residenceClass.text = GetClassString(residence.ResidentClass());
        capacity.text = residence.HousingCapacity().ToString();
        occupancy.text = residence.ResidentsCount().ToString();
        quality.text = residence.housingQuality.ToString();
        rent.text = residence.Rent().ToString();
    }

}
