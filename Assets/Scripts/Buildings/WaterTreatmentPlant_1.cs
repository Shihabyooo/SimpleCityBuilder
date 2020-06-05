using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTreatmentPlant_1 : InfrastructureBuilding
{
    //TODO reset (uncomment) the {get; private set;} parts bellow:
    public float currentTreatmentRate;// {get; private set;}
    public float currentEfficiency;// {get; private set;}
    public float currentMaxTreatmentRate;// {get; private set;} //not to be confused with maxTreatmentRate in WaterTreatmentPlantStats. This one is variable depending on other simulation factors.
    //public float currentDemand = 0.0f;


    [SerializeField] WaterTreatmentPlantStats plantStats = new WaterTreatmentPlantStats();

    protected override void OnConstructionComplete()
    {
        base.OnConstructionComplete();
        Grid.grid.SetInfrastructureState(InfrastructureService.water, occupiedCell[0], occupiedCell[1], infraStats.radiusOfInfluence);
        ComputeProduction();
        GameManager.buildingsMan.AddInfrastructureBuilding(this, InfrastructureService.water);
    }

    public override float ComputeProduction()  //To be implemented properly after calculations and balancing are finished. For now, use the simple calculations bellow.
    {
        float production = 0.0f;

        //Compute the volume of water within the extractionRadius
        //Building Budget affects efficiency.
        //Efficiency affect how much is the currentMaxTreatmentRate compared to plantStats.maxTreatmentRate.
        
        //Sample groundWaterVolumeLayer in the main Grid
        float availableGroundWater = Grid.grid.GetTotalGroundWaterVolume(occupiedCell[0], occupiedCell[1], plantStats.extractionRadius);
        currentEfficiency = 0.8f;

        if (availableGroundWater < plantStats.minGroundWaterVolume)
            return 0.0f;

        currentMaxTreatmentRate = currentEfficiency * plantStats.maxTreatmentRate;
        //production = Mathf.Min(currentMaxTreatmentRate, currentDemand);
        production = currentMaxTreatmentRate;

        return production;
    }
    

    
    // void OnDrawGizmos() //test
    // {
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawWireSphere(this.transform.position, infraStats.radiusOfInfluence * Grid.cellSize);
    // }
}

[System.Serializable]
public class WaterTreatmentPlantStats
{
    public float maxTreatmentRate = 0.0f; //unit volume per unit time.
    public float minGroundWaterVolume = 0.0f; //Minimum ground water available for plant to operate. Unit volume,
    public uint extractionRadius = 0; //The radius inside which the plant can extract ground water for use.
}