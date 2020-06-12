using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsManager : MonoBehaviour
{
    BuildingProposal currentProposal = null;
    [SerializeField] BuildingsDatabase database = new BuildingsDatabase();

    public List<Building> constructedBuildings {get; private set;} //containers ALL constructed buildings.
    public List<InfrastructureBuilding> waterProductionBuildings {get; private set;} //contains only water producing buildings
    public List<InfrastructureBuilding> powerProductionBuildings {get; private set;} //contains only power producing buildings
    //TODO add lists for remaining infrastructure types and update the AddInfrastructureBuilding() method accordingly.


    void Awake()
    {
        constructedBuildings = new List<Building>();
        waterProductionBuildings = new List<InfrastructureBuilding>();
        powerProductionBuildings = new List<InfrastructureBuilding>();
    }

    public BuildingProposal StartNewBuildingProposal(int buildingID)
    {
        //print ("Starting building proposal for id: " + buildingID);
        currentProposal = new BuildingProposal(buildingID);//, this);

        return currentProposal;
    }

    public void CancelCurrentPlannedProposal()
    {
        currentProposal = null;
    }

    public void AddConstructedBuilding(Building building)
    {
        constructedBuildings.Add(building);
    }

    public void AddInfrastructureBuilding(InfrastructureBuilding building, InfrastructureService type)
    {
        switch(type) 
        {
            case InfrastructureService.water:
                waterProductionBuildings.Add(building);
                break;
            case InfrastructureService.power:
                powerProductionBuildings.Add(building);
                break;
            case InfrastructureService.health:
                break;
            case InfrastructureService.education:
                break;
            case InfrastructureService.gas:
                break;
            default:
                break;
        }
    }

    public System.Guid GetNewGUID()
    {
        System.Guid newID = System.Guid.NewGuid();
        return newID;
    }

    public ResidentialBuilding GetResidentialBuildingWithEmptySlot(HousingClass _class, bool random = true)
    {
        //This is a stupid attempt at randomizing building pickup.
        int count = 0;

        while (count < 100) //tries 100 times to find a random building, if fails, grabs the first one it find from the foreach loop bellow.
        {
            int randomInt = Random.Range(0, constructedBuildings.Count);

            if (constructedBuildings[randomInt].GetStats().type == BuildingType.residential
                && constructedBuildings[randomInt].gameObject.GetComponent<ResidentialBuilding>() != null
                && constructedBuildings[randomInt].gameObject.GetComponent<ResidentialBuilding>().ResidentClass() == _class
                && constructedBuildings[randomInt].gameObject.GetComponent<ResidentialBuilding>().EmptyHousingSlots() > 0)
            {
                return constructedBuildings[randomInt].gameObject.GetComponent<ResidentialBuilding>();
            }
            count++;
        }

        //Reaching here means that that stupid randomization thing above failed.
        foreach (Building building in constructedBuildings)
        {
            if (building.GetStats().type == BuildingType.residential
                && building.gameObject.GetComponent<ResidentialBuilding>() != null
                && building.gameObject.GetComponent<ResidentialBuilding>().ResidentClass() == _class
                && building.gameObject.GetComponent<ResidentialBuilding>().EmptyHousingSlots() > 0)
                {
                    return building.gameObject.GetComponent<ResidentialBuilding>();
                }
        }

        return null;
    }

    public WorkPlace GetEmptyWorkSlot(EducationLevel educationLevel, bool random = true)
    {
        int count = 0;

        while (count < 100) //tries 100 times to find a random building, if fails, grabs the first one it find from the foreach loop bellow.
        {
            int randomInt = Random.Range(0, constructedBuildings.Count);

            if ( constructedBuildings[randomInt].gameObject.GetComponent<WorkPlace>() != null
                && constructedBuildings[randomInt].gameObject.GetComponent<WorkPlace>().WorkerEducationLevel() == educationLevel
                && constructedBuildings[randomInt].gameObject.GetComponent<WorkPlace>().AvailableWorkerSlots() > 0)
            {
                return constructedBuildings[randomInt].gameObject.GetComponent<WorkPlace>();
            }
            count++;
        }

        foreach (Building building in constructedBuildings)
        {
            if (building.gameObject.GetComponent<WorkPlace>() != null
                && building.gameObject.GetComponent<WorkPlace>().WorkerEducationLevel() == educationLevel
                && building.gameObject.GetComponent<WorkPlace>().AvailableWorkerSlots() > 0)
                {
                    return building.gameObject.GetComponent<WorkPlace>();
                }
        }

        return null;
    }

    //=======================================================================================================================
    //=======================================================================================================================
    //I made this class within BuildingsManager to access some of the latter's private members without extra lines of code...
    public class BuildingProposal
    {
        //public BuildingsManager buildingsManRef {get; private set;}
        public GameObject targetBuilding {get; private set;}

        public BuildingProposal(int _targetBuildingID)//, BuildingsManager _buildingsManager)
        {
            //buildingsManRef = _buildingsManager;

            //TODO once a mesh loader (or dedicated prefabs are created) for the mock avatar for the buildings, replace the line bellow.
            GameObject newProposedBuilding = GameManager.buildingsMan.database.GetBuildingObject(_targetBuildingID).gameObject;
            targetBuilding = GameObject.Instantiate(newProposedBuilding);

            if (targetBuilding == null)
                print ("WARNING! targetBuildingProposal is set to null, meaning no building of provided ID could be found.");
        }

      public bool CanConstructHere(Cell cell)
       {
          if (cell.isOccupied || !targetBuilding.GetComponent<Building>().CheckConstructionResourceRequirements(cell))
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
            
            //buildingsManRef.AddConstructedBuilding(targetBuilding.GetComponent<Building>());
            targetBuilding.GetComponent<Building>().BeginConstruction(cell);
            targetBuilding = null;
                
            return true;
        }

        public void Cancel()
        {
            //if (targetBuildingAvatar != null) //In a complete implementation, calling this method shouldn't be possible without a targetBuildingAvatar set.
            Destroy (targetBuilding);
        
            GameManager.buildingsMan.currentProposal = null;
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