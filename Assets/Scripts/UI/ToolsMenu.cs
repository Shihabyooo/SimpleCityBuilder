using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsMenu : MonoBehaviour
{
    public void StartTool (int toolID)
    {
        switch(toolID)
        {
            case 1:
            GameManager.gameMan.SwitchToCellInspector();
                break;
            default:
            print ("!!!!!!!!!!!!!!!!!!!!!! Warning! Attempting to start an unknown tool");
                break;
        }
    }

    public void OverLayToggle(int layerID) 
    {
        bool state = this.transform.Find(layerID.ToString()).GetComponent<UnityEngine.UI.Toggle>().isOn;

        switch(layerID)
        {
            case 0:
                Grid.grid.visualizePowerInfra = state;
                break;
            case 1:
                Grid.grid.visualizeWaterInfra = state;
                break;
            case 2:
                Grid.grid.visualizeGroundWaterCapacity = state;
                break;
            case 3:
                Grid.grid.visualizeGroundWaterVolume = state;
                break;
            case 4:
                Grid.grid.visualizeGroundWaterRecharge = state;
                break;
            case 5:
                Grid.grid.visualizeWindSpeed = state;
                break;
            case 6:
                Grid.grid.visualizeWindDirection = state;
                break;
            case 7:
                Grid.grid.visualizePollution = state;
                break;
            case 8:
                Grid.grid.visualizeRainfall = state;
                break;
        }
    }
}
