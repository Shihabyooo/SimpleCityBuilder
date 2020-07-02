using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorkPlace))]
public class School_1 : InfrastructureBuilding
{
    override protected void Awake()
    {
        base.Awake();
        infrastructureType = InfrastructureService.education;
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.school);
    }
}
