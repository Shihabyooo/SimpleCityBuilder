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
    
    List<Transform> templateTransforms = new List<Transform>();
    Transform activeTemplate = null;
    void Awake()
    {
        if (viewerHandler == null)
        {
            viewerHandler = this;
        }
        else
            Destroy(this.gameObject);

        foreach (Transform transform in this.transform)
        {
            templateTransforms.Add(transform);
            transform.gameObject.SetActive(false);
        }
    }

    public void Show(Building building, BuildingViewerTemplate template)
    {
        switch(template)
        {
            case BuildingViewerTemplate.generic:
                LaunchViewer(building, "GenericBuilding");
                break;
            case BuildingViewerTemplate.residential:
                break;
            case BuildingViewerTemplate.commercial:
                break;
            case BuildingViewerTemplate.industrial:
                break;
            case BuildingViewerTemplate.powerPlant:
                break;
            case BuildingViewerTemplate.windFarm:
                break;
            case BuildingViewerTemplate.waterTreatment:
                break;
            case BuildingViewerTemplate.school:
                break;
            case BuildingViewerTemplate.police:
                break;
            case BuildingViewerTemplate.hospital:
                break;
            case BuildingViewerTemplate.garbageDump:
                break;
            case BuildingViewerTemplate.park:
                break;
            default: //use generic
                break;
        }
    }

    bool LaunchViewer(Building building, string viewerName)
    {
        foreach (Transform transform in templateTransforms)
        {
            if (transform.gameObject.name == viewerName)
            {
                activeTemplate = this.transform.Find(viewerName);
                activeTemplate.gameObject.SetActive(true);
                activeTemplate.GetComponent<BaseDataViewer>().SetData(building);
                isShown = true;
                return true;
            }
        }

        return false;
    }

    public void Close()
    {
        isShown = false;
        activeTemplate.gameObject.SetActive(false);
        activeTemplate = null;
    }

    // public void OnBudgetSliderChange()
    // {

    // }
}
