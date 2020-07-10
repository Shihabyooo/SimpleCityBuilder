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

    [SerializeField] BuildingIDHandler<Park_1> parkIDHandler = new BuildingIDHandler<Park_1>();
    [SerializeField] BuildingIDHandler<School_1> schoolIDHandler = new BuildingIDHandler<School_1>();
    [SerializeField] BuildingIDHandler<PoliceStation_1> policeIDHandler = new BuildingIDHandler<PoliceStation_1>();

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
        BuildingType type = building.GetStats().type;

        if (type == BuildingType.residential)
            residentialBuildings.Add(building.gameObject.GetComponent<ResidentialBuilding>());
        else if (type == BuildingType.industrial)
            industrialBuildings.Add(building.gameObject.GetComponent<IndustrialBuilding>());
        else if (type == BuildingType.commercial)
            commercialBuildings.Add(building.gameObject.GetComponent<CommercialBuilding>());
        else if (type == BuildingType.infrastructure)
        {
            InfrastructureBuilding infrastructureBuilding = building.GetComponent<InfrastructureBuilding>();
            AddInfrastructureBuilding(infrastructureBuilding, infrastructureBuilding.InfrastructureType());
        }

        if (building.gameObject.GetComponent<WorkPlace>() != null)
            workPlaces.Add(building.gameObject.GetComponent<WorkPlace>());
    }

    void AddInfrastructureBuilding(InfrastructureBuilding building, InfrastructureService type)
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
            case InfrastructureService.safety:
                break;
            case InfrastructureService.parks:
                break;
            default:
                break;
        }
    }

    public void RemoveBuilding(Building building)
    {

    }

    public ulong GetNewID(InfrastructureBuilding building) //So far, only parks, police stations and schools require IDs and are limited in number.
    { 
        InfrastructureService type = building.InfrastructureType();
        ulong newID = 0;

        switch (type)
        {
            case InfrastructureService.parks:
                newID = parkIDHandler.AssignNewBuilding(building.gameObject.GetComponent<Park_1>());
                break;
            case InfrastructureService.safety:
                newID = policeIDHandler.AssignNewBuilding(building.gameObject.GetComponent<PoliceStation_1>());
                break;
            case InfrastructureService.education:
                newID = schoolIDHandler.AssignNewBuilding(building.gameObject.GetComponent<School_1>());
                break;
            default:
                break;
        }

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

    public InfrastructureBuilding GetInfrastructureBuilding(InfrastructureService type, ulong id) //Used for buildings with unique IDs only.
    {
        switch(type) 
        {
            // case InfrastructureService.water:
            //     break;
            // case InfrastructureService.power:
            //     break;
            // case InfrastructureService.health:
            //     break;
            case InfrastructureService.education:
                return schoolIDHandler.Building(id);
            // case InfrastructureService.gas:
            //     break;
            case InfrastructureService.safety:
                return policeIDHandler.Building(id);
            case InfrastructureService.parks:
                return parkIDHandler.Building(id);
            default:
                return null;
        }
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

[System.Serializable]
public class BuildingIDHandler<T>
{
    //Note: This object does not track the state of the objects that request an ID. Desotrying/Removing that object without it first releasing its ID will result in the
    //said ID remaining marked as assigned for the remainder of this object's life. 
    //A solution to this would be to overhall the system and have this object track both IDs and buildings (of its tracked type), including removal of this building.

    ulong idAssignmentTracker = 0;
    [SerializeField][Range(6, 64)] int maxCount = 32;
    [SerializeField] int currentCount = 0;

    T[] trackedBuildings;

    public BuildingIDHandler()
    {
        trackedBuildings = new T[maxCount];
    }

    public ulong AssignNewBuilding(T building)
    {
        ulong newID = GetNewID();
        
        if (newID > 0)
        {
            int order = Mathf.RoundToInt(Mathf.Log((float)newID, 2.0f));
            //print("Assigning new obj of order: " + order); //test
            trackedBuildings[order] = building;
        }

        return newID;
    }

    ulong GetNewID() //0 is an error state.
    {
        for (uint i = 0; i < maxCount; i++)
        {
            ulong id = ULongPow(2, i);

            if (!IsIDAssigned(id))
            {
                MarkAssigned(id);
                return id;
            }
        }

        return 0;
    }

    public T Building(ulong id)
    {
        int order = Mathf.RoundToInt(Mathf.Log((float)id, 2.0f));

        if (order < maxCount)
            return trackedBuildings[order];
        else
            return default(T);
    }

    public void ReleaseID(ulong id)
    {
        idAssignmentTracker = idAssignmentTracker & (~id);
        currentCount--;
    }

    bool IsIDAssigned(ulong id)
    {
        if ((idAssignmentTracker & id) == id) 
            return true;
        
        return false;
    }   

    void MarkAssigned(ulong id)
    {
        idAssignmentTracker = idAssignmentTracker | id;
        currentCount++;
    }
    
    static public ulong ULongPow(uint _base, uint power) 
    {
        //This method is to workaround the fact that Unity and Csharp (afaik) don't provide methods to calculate power of integers. And since we need exact integer results up
        //to the max cap of ulongs (18,446,744,073,709,551,615), but floats and doubles start to deviate at 16,777,217 and 9,007,199,254,740,993, we have to make a custom power
        //calculation that only uses integers.

        ulong result = 1;
        for (uint i = 0; i < power; i++)
            result = result * _base;

        return result;
    }

}