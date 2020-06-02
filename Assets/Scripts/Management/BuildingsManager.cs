﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsManager : MonoBehaviour
{
    BuildingProposal currentProposal = null;
    [SerializeField] BuildingsDatabase database;

    public List<Building> constructedBuildings {get; private set;}

    void Awake()
    {
        constructedBuildings = new List<Building>();
    }

    public BuildingProposal StartNewBuildingProposal(int buildingID)
    {
        print ("Starting building proposal for id: " + buildingID);
        currentProposal = new BuildingProposal(buildingID, this);

        return currentProposal;
    }

    public void CancelCurrentPlannedProposal()
    {
        currentProposal = null;
    }

    void AddConstructedBuilding(Building building)
    {
        constructedBuildings.Add(building);
    }


    //=======================================================================================================================
    //=======================================================================================================================
    //I made this class within BuildingsManager to access some of the latter's private members without extra lines of code...
    public class BuildingProposal
    {
        public BuildingsManager buildingsManRef {get; private set;}
        public GameObject targetBuilding {get; private set;}

        public BuildingProposal(int _targetBuildingID, BuildingsManager _buildingsManager)
        {
            buildingsManRef = _buildingsManager;

            //TODO once a mesh loader (or dedicated prefabs are created) for the mock avatar for the buildings, replace the line bellow.
            GameObject newProposedBuilding = buildingsManRef.database.GetBuildingObject(_targetBuildingID).gameObject;
            targetBuilding = GameObject.Instantiate(newProposedBuilding);

            if (targetBuilding == null)
            print ("WARNING! targetBuildingProposal is set to null, meaning no building of provided ID could be found.");
        }

      public bool CanConstructHere(Cell cell)
       {
          if (cell.isOccupied || !targetBuilding.GetComponent<Building>().CheckResourceRequirements(cell))
               return false;

           //TODO add logic to assess whether positition supports building of this type here.

         return true;
        }

        public bool Construct(Cell cell)
        {
            if (!CanConstructHere(cell))
                return false;
            
            //TODO handle object construction here (add to waiting queue, update relevant databases, etc)    
            Grid.grid.SetCellOccupiedState(cell, true);
            
            buildingsManRef.AddConstructedBuilding(targetBuilding.GetComponent<Building>());
            targetBuilding.GetComponent<Building>().BeginConstruction();
            targetBuilding = null;
                
            return true;
        }

        public void Cancel()
        {
            //if (targetBuildingAvatar != null) //In a complete implementation, calling this method shouldn't be possible without a targetBuildingAvatar set.
            Destroy (targetBuilding);
        
            buildingsManRef.currentProposal = null;
        }

        public void MovePlan(Vector3 positition)
        {
            targetBuilding.transform.position = positition;
        }
    }
}


[System.Serializable]
class BuildingsDatabase
{
    public Building[] buildings;

    public BuildingStats GetStatsForBuilding(int buildingID)
    {
        foreach (Building building in buildings)
        {
            if (building.GetStats().id == buildingID)
                return building.GetStats();
        }

        return null;
    }

    public GameObject GetBuildingObject(int buildingID)
    {
        foreach (Building building in buildings)
        {
            if (building.GetStats().id == buildingID)
                return building.gameObject;
        }

        return null;
    }

}