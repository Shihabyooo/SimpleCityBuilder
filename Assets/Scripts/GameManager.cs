using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CursorHandler))]
public class GameManager : MonoBehaviour
{
   static public GameManager gameMan = null;
   CursorHandler cursorHandler;
   BuildingsManager buildingsMan;
    void Awake()
    {
        if (gameMan == null)
        {
            gameMan = this;
            cursorHandler = this.gameObject.GetComponent<CursorHandler>();
            buildingsMan = this.gameObject.GetComponent<BuildingsManager>();
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
}
