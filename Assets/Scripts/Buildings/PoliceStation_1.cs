﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorkPlace))]
public class PoliceStation_1 : InfrastructureBuilding
{
   override protected void Awake()
    {
        base.Awake();
    }

    public override void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.police);
    }
}
