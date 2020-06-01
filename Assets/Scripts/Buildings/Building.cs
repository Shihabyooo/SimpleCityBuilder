using System.Collections;
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

    public BuildingStats GetStats()
    {
        return stats;
    }

    public void BeginConstruction()
    {
        isUnderConstruction = true;
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
        yield return isUnderConstruction = false;
    }
}

[System.Serializable]
public class BuildingStats
{
    public int id;
    public uint cost = 0;
    public BuildingType type;
    public float constructionTime = 0.0f;

    //requirements
    public float powerRequirements = 1.0f; //in units per time.
    public float waterRequirements = 1.0f; //in units per time.
    //TODO add remaining requirements.

    //TODO add remaining -universal- parameters here.
}