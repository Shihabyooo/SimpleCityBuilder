using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] int buildingID;
    Button button;
    bool isLocked = false;

    void Awake()
    {
        button = this.gameObject.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        //print ("Clicked from " + this.gameObject.name);
        GameManager.gameMan.SwitchToBuildingPlacement(buildingID);
    }


    public void UpdateState(long funds)
    {
        if (isLocked)
            return;

        SetButtonState(CanAfford(funds));
    }

    public void SetButtonLockState(bool lockState) //implies disabling button.
    {
        isLocked = lockState;
        SetButtonState(false);
    }

    void SetButtonState(bool state)
    {
        button.interactable = state;
    }

    bool CanAfford(long funds)
    {
        int buildingCost = (int)GameManager.buildingsMan.GetBuildingStats(buildingID).cost;

        if (buildingCost <= funds)
            return true;
        else
            return false;
    }
}
