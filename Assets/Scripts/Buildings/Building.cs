﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType //TODO add remaining types.
{
    residential, commercial, industrial, infrastructure
}

public class Building : MonoBehaviour
{
    public GameObject model {get; private set;}
    [SerializeField] protected BuildingStats stats; //can't use {get; private set;} and have it serialized in the editor at the same time, so leaving at as private and making a getter method.
    public bool isUnderConstruction {get; private set;}
    public uint[] occupiedCell = new uint[2]; //the cell this building is constructed on, set by BuildingsManager (via ProposedBuilding).


    public BuildingStats GetStats()
    {
        return stats;
    }

    public virtual bool CheckResourceRequirements(Cell cell) //this does NOT include checking whether cell is occupied or not, which is handled in BuildingsManager.
    {   
        
        if (stats.requireResourcesToConstruct)
        {
            //There are two steps to this: Check that the cell is covered by required services (data contained in the cell),
            //and check that the available resources for the service are enought (from ResourcesManager)
            if ((stats.powerRequirements > 0.001f && !cell.isPowered)  //first check
                || stats.powerRequirements < GameManager.resourceMan.AvailablePower()) //second check.
                return false;

            if ((stats.waterRequirements > 0.001f && !cell.isWatered)  //first check
                || stats.waterRequirements < GameManager.resourceMan.AvailableWater()) //second check.
                return false;

        }
        
        return true;
    }

    public void BeginConstruction(Cell cell)
    {
        isUnderConstruction = true;
        occupiedCell = new uint[2]{cell.cellID[0], cell.cellID[1]};
        StartCoroutine(Construction());
    }

    float helperTimer = 0.0f;
    IEnumerator Construction()
    {
        print ("Begining construction of: " + this.gameObject.name + ", finishes in: " + stats.constructionTime); //test
        
        while (helperTimer < stats.constructionTime)
        {
            helperTimer += Time.fixedDeltaTime;
            float ratio = Mathf.Clamp(helperTimer/stats.constructionTime, 0.0f, 1.0f);//test
            this.transform.transform.localScale = new Vector3(ratio, ratio, ratio);//test
            yield return new WaitForFixedUpdate();
        }

        print ("Finshed construction of: " + this.gameObject.name ); //test
        OnConstructionComplete();
        yield return null;
    }

    protected virtual void OnConstructionComplete()
    {
        isUnderConstruction = false;
    }
}

[System.Serializable]
public class BuildingStats
{
    public int id;
    public uint cost = 0;
    public BuildingType type;
    public float constructionTime = 0.0f;

    //requirements for operation 
    public bool requireResourcesToConstruct = false; //if set to true, building won't be constructed unless resources are enough, else building will be constructed, but won't operate fully (or at all)
    public float powerRequirements = 1.0f; //in units per time.
    public float waterRequirements = 1.0f; //in units per time.
    //TODO add remaining requirements.

    //TODO add remaining -universal- parameters here.
}