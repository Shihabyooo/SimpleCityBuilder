using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorkPlace))]
public class CommercialBuilding : Building
{
    protected override void Awake()
    {
        base.Awake();
        stats.type = BuildingType.commercial;
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.commercial);
    }
}
