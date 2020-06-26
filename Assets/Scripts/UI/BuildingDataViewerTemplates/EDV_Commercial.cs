using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EDV_Commercial : ExtendedDataViewer
{
    Text commercialClass;
    
    protected override void Awake()
    {
        base.Awake();
        commercialClass = this.transform.Find("Class").GetComponent<Text>();
    }

    //TODO Set commercial class (and any other relevant data) after a script controlling this type of buildings is implemented.
    public override void SetExtendedData(Building building)
    {
        SetWorkplaceDetails(building);
    }
}
