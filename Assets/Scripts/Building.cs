using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType //TODO add remaining types.
{
    residential,
}

public class Building : MonoBehaviour
{
    public GameObject model {get; private set;}
    [SerializeField] public BuildingStats stats; //can't use {get; private set;} and have it serialized in the editor at the same time, so leaving at as private and making a getter method.
    public bool isUnderConstruction {get; private set;}

    BuildingStats GetStats()
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
        //The material/opacity changing lines bellow are for testing purposes only (to show construction progress);
        print ("Begining construction of: " + this.gameObject.name + ", finishes in: " + stats.constructionTime); //test
        Material material = this.gameObject.GetComponent<MeshRenderer>().material; //test   
        this.gameObject.GetComponent<MeshRenderer>().material.color = new Color(material.color.r, material.color.g, material.color.b, 0.0f); //test

        while (helperTimer < stats.constructionTime)
        {
            helperTimer += Time.fixedDeltaTime;
            this.gameObject.GetComponent<MeshRenderer>().material.color = new Color(material.color.r, material.color.g, material.color.b, (Mathf.Clamp(helperTimer/stats.constructionTime, 0.0f, 1.0f))); //test
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

    //TODO add remaining -universal- parameters here.
}