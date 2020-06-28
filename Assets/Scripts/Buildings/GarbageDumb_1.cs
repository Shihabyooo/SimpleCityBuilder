using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorkPlace))]
public class GarbageDumb_1 : InfrastructureBuilding
{
    override protected void Awake()
    {
        base.Awake();
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.garbageDump);
    }
}
