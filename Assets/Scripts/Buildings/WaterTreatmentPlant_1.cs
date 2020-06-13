﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTreatmentPlant_1 : InfrastructureBuilding
{
    //TODO reset (uncomment) the {get; private set;} parts bellow:
    public float currentTreatmentRate;// {get; private set;}
    public float currentEfficiency;// {get; private set;}
    public float currentMaxTreatmentRate;// {get; private set;} //not to be confused with maxTreatmentRate in WaterTreatmentPlantStats. This one is variable depending on other simulation factors.
    public float availableGroundWater;// {get; private set;}
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
        availableGroundWater = Grid.grid.GetTotalGroundWaterVolume(occupiedCell[0], occupiedCell[1], plantStats.extractionRadius);
        
        //currentEfficiency = 0.8f;
      
        currentEfficiency = ComputeEfficiency();

        if (availableGroundWater < plantStats.minGroundWaterVolume)
            return 0.0f;

        currentMaxTreatmentRate = currentEfficiency * plantStats.maxTreatmentRate;
        //production = Mathf.Min(currentMaxTreatmentRate, currentDemand);
        production = currentMaxTreatmentRate;
        currentTreatmentRate = currentLoad * currentMaxTreatmentRate;

        return production;
    }
    
    public override void UpdateEffectOnNature(int timeWindow)
    {
        base.UpdateEffectOnNature(timeWindow);
        
        float waterAbstraction = currentTreatmentRate * timeWindow;
        
        if (waterAbstraction > 0.0001f) //no need to have the AbstracWater method run for nothing...
            AbstractWater(waterAbstraction);
    }

    void AbstractWater(float volume)
    {
        //TODO redo this algorithm so as to have abstraction happens equally for all covered cells (that has water in them).

        float abstractionRun = 0.0f;
        for (uint r = 0; r <= plantStats.extractionRadius; r++)
        {
            uint minY = (uint)Mathf.Max((long)occupiedCell[1] - (long)r, 0);
            uint maxY = (uint)Mathf.Min((long)occupiedCell[1] + (long)r, Grid.grid.noOfCells.y - 1);
            for (uint i = minY; i <= maxY; i++)
            {
                long sqrtVal = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(r, 2) - Mathf.Pow((long)i - (long)occupiedCell[1], 2))); //cache this calculation, since its result will be used twice.

                uint minX = (uint)Mathf.Max( (long)occupiedCell[0] - sqrtVal, 0);
                uint maxX = (uint)Mathf.Min( (long)occupiedCell[0] + sqrtVal, Grid.grid.noOfCells.x - 1);
                
                uint range = (uint)Mathf.Max(maxX - minX , 1);
                for (uint j = minX; j <= maxX; j+= range)
                {
                    float remainingAbstractionRequirement = Mathf.Max(volume - abstractionRun, 0.0f);
                    if (remainingAbstractionRequirement < 0.0001f)
                        return;

                    float cellAbstraction = Mathf.Min(remainingAbstractionRequirement, Grid.grid.groundWaterVolumeLayer.GetCellValue(j, i));
                    abstractionRun += cellAbstraction;
                    Grid.grid.groundWaterVolumeLayer.GetCellRef(j, i) += -1.0f * cellAbstraction;
                }
            }   
        }
    }
    
}

[System.Serializable]
public class WaterTreatmentPlantStats
{
    public float maxTreatmentRate = 0.0f; //unit volume per unit time.
    public float minGroundWaterVolume = 0.0f; //Minimum ground water available for plant to operate. Unit volume,
    public uint extractionRadius = 0; //The radius inside which the plant can extract ground water for use.
}