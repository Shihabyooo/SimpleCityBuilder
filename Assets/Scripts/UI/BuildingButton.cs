using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [SerializeField] int buildingID;

    void Awake()
    {
        this.gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        //print ("Clicked from " + this.gameObject.name);
        GameManager.gameMan.SwitchToBuildingPlacement(buildingID);
    }

}
