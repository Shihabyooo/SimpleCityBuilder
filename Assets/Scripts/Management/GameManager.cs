using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ControlManager))]
[RequireComponent(typeof(ResourcesManager))]
[RequireComponent(typeof(SimulationManager))]
[RequireComponent(typeof(BuildingsManager))]
[RequireComponent(typeof(ClimateManager))]
[RequireComponent(typeof(PopulationManager))]
[RequireComponent(typeof(UIManager))]
public class GameManager : MonoBehaviour
{
    static public GameManager gameMan = null;
    static public ResourcesManager resourceMan = null;
    ControlManager controlMan;
    static public BuildingsManager buildingsMan = null;
    static public ClimateManager climateMan = null;
    static public SimulationManager simMan;
    static public PopulationManager populationMan = null;
    static public UIManager uiMan = null;

    void Awake()
    {
        if (gameMan == null)
        {
            gameMan = this;
            resourceMan = this.gameObject.GetComponent<ResourcesManager>();
            controlMan = this.gameObject.GetComponent<ControlManager>();
            buildingsMan = this.gameObject.GetComponent<BuildingsManager>();
            simMan = this.gameObject.GetComponent<SimulationManager>();
            climateMan = this.gameObject.GetComponent<ClimateManager>();
            populationMan = this.gameObject.GetComponentInChildren<PopulationManager>();
            uiMan = this.gameObject.GetComponentInChildren<UIManager>();
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
        if (controlMan.CurrentCursorMode() != ControlMode.freeMode) 
            return;                                                    

        controlMan.SwitchToBuildingPlacement(buildingsMan.StartNewBuildingProposal(buildingID));
    }

    public void SwitchToCellInspector()
    {
        controlMan.SwitchToCellInspection();
    }
}