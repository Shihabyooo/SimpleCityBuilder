using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingMenu : MonoBehaviour
{
   
   
   public void SelectBuilding(int id)
   {
       //the code to translate the ID into a building goes here. For now, we hardcode block spawning just to test it out.
       GameManager.gameMan.SwitchToBuildingPlacement(id);
   }
}
