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


    //Adding buildings to the lists bellow is done when adding building to constructedBuildings, since they are not subdivided into smaller types like infrastructure buildings.
    public List<WorkPlace> workPlaces {get; private set;}
    public List<ResidentialBuilding> residentialBuildings {get; private set;}
    public List<IndustrialBuilding> industrialBuildings {get; private set;}
    public List<CommercialBuilding> commercialBuildings {get; private set;}


    void Awake()
    {
        constructedBuildings = new List<Building>();
        waterProductionBuildings = new List<InfrastructureBuilding>();
        powerProductionBuildings = new List<InfrastructureBuilding>();
        workPlaces = new List<WorkPlace>();
        residentialBuildings = new List<ResidentialBuilding>();
        industrialBuildings = new List<IndustrialBuilding>();
        commercialBuildings = new List<CommercialBuilding>();
    }

    public BuildingStats GetBuildingStats(int buildingID) 
    {
        BuildingStats tempStats = new BuildingStats(database.GetStatsForBuilding(buildingID));
        
        return tempStats;
    }

    public BuildingProposal StartNewBuildingProposal(int buildingID)
    {
        currentProposal = new BuildingProposal(buildingID);
        return currentProposal;
    }

    public void CancelCurrentPlannedProposal()
    {
        currentProposal = null;
    }

    public void AddConstructedBuilding(Building building)
    {
        constructedBuildings.Add(building);

        if (building.GetStats().type == BuildingType.residential)
            residentialBuildings.Add(building.gameObject.GetComponent<ResidentialBuilding>());
        else if (building.GetStats().type == BuildingType.industrial)
            industrialBuildings.Add(building.gameObject.GetComponent<IndustrialBuilding>());
        else if (building.GetStats().type == BuildingType.commercial)
            commercialBuildings.Add(building.gameObject.GetComponent<CommercialBuilding>());

        if (building.gameObject.GetComponent<WorkPlace>() != null)
            workPlaces.Add(building.gameObject.GetComponent<WorkPlace>());
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
            case InfrastructureService.recreation:
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

    public ResidentialBuilding GetResidentialBuildingWithEmptySlot(CitizenClass _class, bool random = true)
    {
        //Make a list of available housing
        List<ResidentialBuilding> availableHousing = new List<ResidentialBuilding>();

        foreach (ResidentialBuilding residence in residentialBuildings)
        {
            if (residence.ResidentClass() == _class
                && residence.EmptyHousingSlots() > 0)
                {
                    //if we are not returning random housing, we return our first hit
                    if (!random)
                        return residence;

                    availableHousing.Add(residence);
                }
        }
        
        //if the count of refs in availableHousing is zero, means no housing is current available
        if (availableHousing.Count < 1) //This check has a side effect that it includes error state (negative numbers), which might be problematic if not handled explicitly.
            return null;

        //Pick a random residence and return it
        int randomInt = Random.Range(0, availableHousing.Count - 1);
        
        return availableHousing[randomInt];

        //Bellow as an old, "stupid" version of random method aquisition (though it prolly does take less memory).

        // //This is a stupid attempt at randomizing building pickup.
        // int count = 0;

        // while (count < 100) //tries 100 times to find a random building, if fails, grabs the first one it find from the foreach loop bellow.
        // {
        //     int randomInt = Random.Range(0, residentialBuildings.Count);

        //     if (residentialBuildings[randomInt].ResidentClass() == _class
        //         && residentialBuildings[randomInt].EmptyHousingSlots() > 0)
        //     {
        //         return residentialBuildings[randomInt];
        //     }
        //     count++;
        // }

        // //Reaching here means that that stupid randomization thing above failed.
        // foreach (ResidentialBuilding residence in residentialBuildings)
        // {
        //     if (residence.ResidentClass() == _class
        //         && residence.EmptyHousingSlots() > 0)
        //         {
        //             return residence;
        //         }
        // }
        
        //return null;
    }

    public WorkPlace GetEmptyWorkSlot(EducationLevel educationLevel, bool random = true, bool exactLevel = false) //This method is similar to GetResidentialBuildingWithEmptySlot().
    //If exactLevel == false and no empty workslot of said level is found, will (try to) return a workplace with either an education requirements less than required level.
    {                                                                                                               
        if (workPlaces.Count < 1)
            return null;

        //Make a list of available workplaces
        List<WorkPlace> availableWorkPlaces = new List<WorkPlace>();

        foreach (WorkPlace workPlace in workPlaces)
        {
            if (workPlace.AvailableWorkerSlots() > 0
                && workPlace.WorkerEducationLevel() == educationLevel)
                {
                    if (!random)
                        return workPlace;

                    availableWorkPlaces.Add(workPlace);
                }
        }
        
        //if the count of refs in availableWorkPlaces is zero, means no empty work slot is current available
        if (availableWorkPlaces.Count < 1) //This check has a side effect that it includes error state (negative numbers), which might be problematic if not handled explicitly.
        {
            if (exactLevel)
                return null;
            else //search for a workslot with a level less than the specified level through GetEmptyWorkSlotExtended()
                return GetEmptyWorkSlotExtended(educationLevel, random);
        }

        //Pick a random residence and return it
        int randomInt = Random.Range(0, availableWorkPlaces.Count - 1);
        
        return availableWorkPlaces[randomInt];

        // int count = 0;

        // while (count < 100) //tries 100 times to find a random building, if fails, grabs the first one it find from the foreach loop bellow.
        // {
        //     int randomInt = Random.Range(0, workPlaces.Count);

        //     if (workPlaces[randomInt].WorkerEducationLevel() == educationLevel
        //         && workPlaces[randomInt].AvailableWorkerSlots() > 0)
        //     {
        //         return workPlaces[randomInt];
        //     }
        //     count++;
        // }

        // foreach (WorkPlace workPlace in workPlaces)
        // {
        //     if (workPlace.WorkerEducationLevel() == educationLevel
        //         && workPlace.AvailableWorkerSlots() > 0)
        //         {
        //             return workPlace;
        //         }
        // }

        // return null;
    }

    WorkPlace GetEmptyWorkSlotExtended(EducationLevel educationLevel, bool random = true) //See comments on GetEmptyWorkSlot()
    {
        if (workPlaces.Count < 1)
            return null;

        List<WorkPlace> availableWorkPlaces = new List<WorkPlace>();

        foreach (WorkPlace workPlace in workPlaces)
        {
            if (workPlace.AvailableWorkerSlots() > 0
                && workPlace.WorkerEducationLevel() <= educationLevel)
            {
                if (!random)
                    return workPlace;

                availableWorkPlaces.Add(workPlace);
            }
        }

        if (availableWorkPlaces.Count < 1)
            return null;

        int randomInt = Random.Range(0, availableWorkPlaces.Count - 1);
        
        return availableWorkPlaces[randomInt];
    }

    //=======================================================================================================================
    //=======================================================================================================================
    //I made this class within BuildingsManager to access some of the latter's private members without extra lines of code...
    //TODO review the choice above.
    public class BuildingProposal
    {
        public GameObject targetBuilding {get; private set;}
        const float buildingRotationIncrements = 90.0f;

        public BuildingProposal(int _targetBuildingID)//, BuildingsManager _buildingsManager)
        {
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
            
            Grid.grid.SetCellOccupiedState(cell, true);
            targetBuilding.GetComponent<Building>().BeginConstruction(cell);
            targetBuilding = null;
                
            return true;
        }

        public void Cancel()
        {
            Destroy (targetBuilding);
            GameManager.buildingsMan.currentProposal = null;
        }

        public void MovePlan(Vector3 positition)
        {
            targetBuilding.transform.position = positition;
        }

        public void RotatePlan(float direction)
        {
            targetBuilding.transform.Rotate(0.0f, buildingRotationIncrements * Mathf.Sign(direction), 0.0f);
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