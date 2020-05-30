using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingsManager : MonoBehaviour
{
    
    BuildingProposal currentProposal = null;
    [SerializeField] BuildingsDatabase database;

    public BuildingProposal StartNewBuildingProposal(int buildingID)
    {
        currentProposal = new BuildingProposal(buildingID, this);

        return currentProposal;
    }

    public void CancelCurrentPlannedProposal()
    {
        currentProposal = null;
    }

    //I made this class within BuildingsManager to access some of the latter's private members without extra lines of code...
    public class BuildingProposal
    {
         public BuildingsManager buildingsManRef {get; private set;}
        public BuildingStats targetBuildingStats {get; private set;}
        public GameObject targetBuildingAvatar {get; private set;}

        public BuildingProposal(int _targetBuildingID, BuildingsManager _buildingsManager)
        {
            buildingsManRef = _buildingsManager;
            targetBuildingStats = buildingsManRef.database.GetStatsForBuilding(_targetBuildingID);

            //TODO once a mesh loader (or dedicated prefabs are created) for the mock avatar for the buildings, replace the line bellow.
            targetBuildingAvatar = GameObject.Instantiate(buildingsManRef.database.GetBuildingObject(_targetBuildingID).gameObject);

            if (targetBuildingAvatar == null)
            print ("WARNING! targetBuildingProposal is set to null, meaning no building of provided ID could be found.");
        }

      public bool CanConstructHere(Cell cell)
       {
          if (cell.isOccupied)
               return false;

           //TODO add logic to assess whether positition supports building of this type here.

         return true;
        }

        public bool Construct(Cell cell)
        {
            if (!CanConstructHere(cell))
                return false;
                

            //TODO handle object construction here (add to waiting queue, update relevant databases, etc)
            
            return true;
        }

        public void Cancel()
        {
            //if (targetBuildingAvatar != null) //In a complete implementation, calling this method shouldn't be possible without a targetBuildingAvatar set.
            Destroy (targetBuildingAvatar);
        
            buildingsManRef.currentProposal = null;
        }

        public void MovePlan(Vector3 positition)
        {
            targetBuildingAvatar.transform.position = positition;
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
            if (building.stats.id == buildingID)
                return building.stats;
        }

        return null;
    }

    public Building GetBuildingObject(int buildingID)
    {
        foreach (Building building in buildings)
        {
            if (building.stats.id == buildingID)
                return building;
        }

        return null;
    }

}