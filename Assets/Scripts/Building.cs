using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingType //TODO add remaining types.
{
    residential,
}

public class Building : MonoBehaviour
{
    //TODO switch those variables to private set.
    public GameObject model;
    public BuildingStats stats;

}

[System.Serializable]
public class BuildingStats
{
    public int id;
    public uint cost;
    public BuildingType type;
    //TODO add remaining -universal- parameters here.

}