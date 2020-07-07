using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtendedDataViewer : MonoBehaviour
{
    Text employeeCount, employeeCapacity;
    Text employeeEducationLevel;
    Text wages;
    Text workplaceQuality;

    protected virtual void Awake()
    {
        
    }

    public virtual void SetExtendedData(Building building)
    {

    }

    protected void SetWorkplaceDetails(Building building) //to be called only for buildings with workplaces.
    {
        BuildWorkplaceFieldsReferences();
        WorkPlace workPlace = building.gameObject.GetComponent<WorkPlace>();
        employeeCount.text = workPlace.CurrentManpower().ToString();
        employeeCapacity.text = workPlace.MaxManpower().ToString();
        employeeEducationLevel.text = GetEducationLevelString(workPlace.WorkerEducationLevel());
        wages.text = workPlace.Wages().ToString();
        workplaceQuality.text = workPlace.WorkplaceQuality().ToString();
    }

    void BuildWorkplaceFieldsReferences()
    {
        employeeCount = this.transform.Find("EmployeeCount").GetComponent<Text>();
        employeeCapacity = this.transform.Find("EmployeeCap").GetComponent<Text>();
        employeeEducationLevel = this.transform.Find("EducationalLevel").GetComponent<Text>();
        wages = this.transform.Find("Wages").GetComponent<Text>();
        workplaceQuality = this.transform.Find("WorkplaceQuality").GetComponent<Text>();
    }


    protected string GetEducationLevelString(EducationLevel level)
    {
        switch (level)
        {
            case EducationLevel.illiterate:
                return "Illiterate";
            case EducationLevel.primary:
                return "Primary";
            case EducationLevel.secondery:
                return "Secondery";
            case EducationLevel.tertiary:
                return "Tertiary";
            default:
                return "N/A";
        }
    }

    protected string GetClassString(CitizenClass _class)
    {
        switch (_class)
        {
            case CitizenClass.low:
                return "Lower Class";
            case CitizenClass.middle:
                return "Middle Class";
            case CitizenClass.high:
                return "Higher Class";
            default:
                return "N/A";
        }
    }

    protected void StartGraph(TimeSeries<float> timeSeries)
    {

    }
}
