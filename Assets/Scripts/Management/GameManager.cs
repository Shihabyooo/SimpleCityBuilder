using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CursorHandler))]
[RequireComponent(typeof(ResourcesManager))]
[RequireComponent(typeof(SimulationManager))]
[RequireComponent(typeof(BuildingsManager))]
public class GameManager : MonoBehaviour
{
    static public GameManager gameMan = null;
    static public ResourcesManager resourceMan = null;
    CursorHandler cursorHandler;
    static public BuildingsManager buildingsMan = null;
    SimulationManager simMan;

    void Awake()
    {
        if (gameMan == null)
        {
            gameMan = this;
            resourceMan = this.gameObject.GetComponent<ResourcesManager>();
            cursorHandler = this.gameObject.GetComponent<CursorHandler>();
            buildingsMan = this.gameObject.GetComponent<BuildingsManager>();
            simMan = this.gameObject.GetComponent<SimulationManager>();
        }
        else
        {
            Destroy (this.gameObject);
        }
    }

    public void SwitchToBuildingPlacement(int buildingID)
    {
        cursorHandler.SwitchToBuildingPlacement(buildingsMan.StartNewBuildingProposal(buildingID));
    }

    public void SwitchToCellInspector()
    {
        cursorHandler.SwitchToCellInspection();
    }
}
