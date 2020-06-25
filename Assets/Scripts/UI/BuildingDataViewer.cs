using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingViewerTemplate
{
    generic, residential, commercial, industrial, powerPlant, waterTreatment, windFarm, school, police, hospital, garbageDump, park
}

public class BuildingDataViewer : MonoBehaviour
{
    public static BuildingDataViewer viewerHandler = null;
    bool isShown = false;
    Transform viewer = null;

    void Awake()
    {
        if (viewerHandler == null)
        {
            viewerHandler = this;
        }
        else
            Destroy(this.gameObject);

        viewer = this.transform.Find("Viewer");
        viewer.gameObject.GetComponent<BaseDataViewer>().Initialize();
        viewer.gameObject.SetActive(false);
    }

    public void Show(Building building, BuildingViewerTemplate template)
    {
        switch(template)
        {
            case BuildingViewerTemplate.generic:
                LaunchViewer(building, "Generic");
                break;
            case BuildingViewerTemplate.residential:
                LaunchViewer(building, "Residential");
                break;
            case BuildingViewerTemplate.commercial:
                LaunchViewer(building, "Commercial");
                break;
            case BuildingViewerTemplate.industrial:
                LaunchViewer(building, "Industrial");
                break;
            case BuildingViewerTemplate.powerPlant:
                LaunchViewer(building, "PowerPlant");
                break;
            case BuildingViewerTemplate.windFarm:
                LaunchViewer(building, "WindFarm");
                break;
            case BuildingViewerTemplate.waterTreatment:
                LaunchViewer(building, "WaterTreatment");
                break;
            case BuildingViewerTemplate.school:
                LaunchViewer(building, "School");
                break;
            case BuildingViewerTemplate.police:
                LaunchViewer(building, "Police");
                break;
            case BuildingViewerTemplate.hospital:
                LaunchViewer(building, "Hospital");
                break;
            case BuildingViewerTemplate.garbageDump:
                LaunchViewer(building, "GarbageDump");
                break;
            case BuildingViewerTemplate.park:
                LaunchViewer(building, "Park");
                break;
            default: //use generic
                LaunchViewer(building, "Generic");
                break;
        }
    }

    void LaunchViewer(Building building, string extensionName)
    {
        viewer.gameObject.SetActive(true);
        viewer.gameObject.GetComponent<BaseDataViewer>().SetData(building);
        viewer.gameObject.GetComponent<BaseDataViewer>().SetExtendedData(extensionName);
    }

    public void Close()
    {
        isShown = false;
        viewer.gameObject.GetComponent<BaseDataViewer>().CloseActiveExtensions();
        viewer.gameObject.SetActive(false);
    }

}
