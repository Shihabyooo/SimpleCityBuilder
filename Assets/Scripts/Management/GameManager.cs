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

    void Start()
    {
        simMan.StartSimulation();
    }

    public void SwitchToBuildingPlacement(int buildingID)
    {
        //this check is redunandt in SwitchToBuildingPlacement, but because StartNewBuidlingProposal() does work we don't want to happen if we are not constructing, we 
        //do this check here.
        //Alternative: Modify SwitchToBuildingPlacement to take a buildingID and have it handle talking buildingsMan.
        if (cursorHandler.CurrentCursorMode() != ControlMode.freeMode) 
            return;                                                    

        cursorHandler.SwitchToBuildingPlacement(buildingsMan.StartNewBuildingProposal(buildingID));
    }

    public void SwitchToCellInspector()
    {
        cursorHandler.SwitchToCellInspection();
    }
}
