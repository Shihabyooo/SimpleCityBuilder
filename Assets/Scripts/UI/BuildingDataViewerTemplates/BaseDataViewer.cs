using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseDataViewer : MonoBehaviour
{

    protected Transform content;

    protected virtual void Awake()
    {
        content = this.transform.Find("Viewport").Find("Content");
    }

    public virtual void SetData(Building building)
    {

    }

    protected string GetBuildingType(Building building)
    {
        switch (building.GetStats().type)
        {
            case BuildingType.commercial:
                return "Commercial";
            case BuildingType.industrial:
                return "Industrial";
            case BuildingType.infrastructure:
                return "Infrasctructure";
            case BuildingType.residential:
                return "Residential";
            default:
                return "N/A";
        }
    }

    protected string GetDateString(Building building)
    {
        System.DateTime date = building.ConstructionDate();
        return (date.Day.ToString() + "-" + GameManager.uiMan.GetMonthAbbr(date.Month) + "-" + date.Year);
    }
}
