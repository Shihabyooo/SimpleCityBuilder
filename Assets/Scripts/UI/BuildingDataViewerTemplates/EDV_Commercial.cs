using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EDV_Commercial : ExtendedDataViewer
{
    Text commercialClass;
    Text count, capacity;
    Text educationLevel;
    protected override void Awake()
    {
        base.Awake();
        commercialClass = this.transform.Find("Class").GetComponent<Text>();
        count = this.transform.Find("EmployeeCount").GetComponent<Text>();
        capacity = this.transform.Find("EmployeeCap").GetComponent<Text>();
        educationLevel = this.transform.Find("EducationalLevel").GetComponent<Text>();
    }

    //TODO Set commercial class (and any other relevant data) after a script controlling this type of buildings is implemented.
    public override void SetExtendedData(Building building)
    {
        WorkPlace workPlace = building.gameObject.GetComponent<WorkPlace>();
        count.text = workPlace.CurrentManpower().ToString();
        capacity.text = workPlace.MaxManpower().ToString();
        educationLevel.text = GetEducationLevelString(workPlace.WorkerEducationLevel());
    }
}
