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
    

    void Awake()
    {
        if (viewerHandler == null)
        {
            viewerHandler = this;
        }
        else
            Destroy(this.gameObject);
    }

    // void Update()
    // {
    //     this.transform.rotation = Camera.main.transform.rotation;
    //     this.transform.Rotate(0.0f, 180.0f, 0.0f);
        
    //     //Close the viewer if it's outside camera view (player pans away from it)
    //     if (isShown)
    //     {
    //         Vector3 viewportPos = Camera.main.WorldToViewportPoint(this.transform.position);
    //         if (viewportPos.x <= -0.1f || viewportPos.x >= 1.1f
    //             ||viewportPos.y <= -0.1f || viewportPos.y >= 1.1f)
    //         {
    //             Close();
    //         }
    //     }
    // }

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

    void LaunchViewer(Building building, string viewerName)
    {
        this.transform.Find(viewerName).GetComponent<BaseDataViewer>().SetData(building);
    }

    public void Close()
    {
        isShown = false;
    }
}
