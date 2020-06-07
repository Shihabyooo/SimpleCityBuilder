using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimateManager : MonoBehaviour //for the sake of simplicity, assume "climate" and "weather" to be synonyms..
{
    [SerializeField] float[] averageDailyRainfallPerMonth = new float[12]; //in units total depth rainfall per day (preferably mm if using SI-ish units in the project).
    [SerializeField] [Range(0.0f, 1.0f)] float[] dailyRainProbabilityPerMonth = new float [12]; //in any given day of the month, how probable will it precipitate.
    [SerializeField] float maxVariationInRainfall = 0.2f; //the range (plus or minus the average, as a fraction of the average) in which the rainfall will be randomized.
    [SerializeField] int maxNoOfActiveStorms = 5;
    [SerializeField] int maxStormDuration = 7;
    [SerializeField] List<Storm> activeStorms = new List<Storm>();

    public bool isRaining {get; private set;}
    //public float currentRainfall {get; private set;}

    void Awake()
    {
        isRaining = false;
    }


    public void UpdateClimate(System.DateTime date)
    {
       ProcessRainfall(date);
    }



    void ProcessRainfall(System.DateTime date)
    {   
        //First update existing storms
        UpdateStorms(date);


        //Then, see if we should add a new storm, based on probability of rainfall.
        if (activeStorms.Count < maxNoOfActiveStorms)
        { 
            float dieThrow = Random.Range(0.0f, 1.0f);        
            if (dieThrow <= dailyRainProbabilityPerMonth[date.Month - 1])
                SpawnStorms(date);
        }
    }


    void SpawnStorms(System.DateTime date)
    {
        
        isRaining = true;
        int noNewOfStorms = Mathf.Min(Random.Range(1, maxNoOfActiveStorms), maxNoOfActiveStorms - activeStorms.Count);
        print ("Spawning: " + noNewOfStorms + " storms"); //test

        for (int i = 0; i <= noNewOfStorms; i++)
        {
            Vector2 stormCentre = new Vector2(Random.Range(1, Grid.grid.noOfCells.x - 1), Random.Range(1, Grid.grid.noOfCells.y - 1));
            uint stormRadius = (uint)Random.Range(1, Mathf.RoundToInt((float)Mathf.Min(Grid.grid.noOfCells.x, Grid.grid.noOfCells.y) / 4.0f));
            uint stormDuration = (uint)Random.Range(1, 7);
            float averageStormRainfallAtCentre = Random.Range( Mathf.Max(averageDailyRainfallPerMonth[date.Month - 1] - maxVariationInRainfall * averageDailyRainfallPerMonth[date.Month - 1], 0.0f),
                                            averageDailyRainfallPerMonth[date.Month - 1] + maxVariationInRainfall * averageDailyRainfallPerMonth[date.Month - 1]);
            float dailyStormRainfallVariation = maxVariationInRainfall * averageStormRainfallAtCentre;

            Storm newStorm = new Storm(stormCentre, stormRadius, stormDuration, averageStormRainfallAtCentre, dailyStormRainfallVariation, date);
            activeStorms.Add(newStorm);
        }
    }

    void UpdateStorms(System.DateTime date)
    {
        if (activeStorms.Count < 1) //nothing to update
            return;
        
        Grid.grid.ZeroRainfallLayer(); //Because the loop bellow is cummulitive.

        foreach (Storm storm in activeStorms)
        {
            if (date.Subtract(date).Days > storm.duration)
            {
                //remove storm from the list
                activeStorms.Remove(storm);
            }
            else
            {
                //update rainfall layer
                float todaysRainfall = Random.Range(storm.averageRainfall - storm.dailyRainfallVariation, storm.averageRainfall + storm.dailyRainfallVariation);
                Grid.grid.SetRainfallCummulitive(storm.centreCell[0], storm.centreCell[1], storm.radius, todaysRainfall);
            }
        }
    }


    //Testing visualization
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach(Storm storm in activeStorms)
        {
            Gizmos.DrawSphere(Grid.grid.GetCellPosition(storm.centreCell[0], storm.centreCell[0]) + new Vector3(0.0f, 10.0f, 0.0f), storm.radius);
        }
    }

}

[System.Serializable]
public class Storm
{
    //TODO reset the {get; private set;} once testing is finished.
    public uint[] centreCell;// {get; private set;}
    public uint radius;// {get; private set;}
    public uint duration;// {get; private set;}
    public float averageRainfall;// {get; private set;} //for the central cell.
    public float dailyRainfallVariation;// {get; private set;}
    public System.DateTime startDate;// {get; private set;}

    public Storm(uint centre_x, uint centre_y, uint _radius, uint _duration, float _averageRainfall, float _dailyRainfallVariation, System.DateTime _date)
    {
        InitializeStorm(centre_x, centre_y, _radius, _duration, _averageRainfall, _dailyRainfallVariation, _date);
    }

    public Storm(Vector2 centre, uint _radius, uint _duration, float _averageRainfall, float _dailyRainfallVariation, System.DateTime _date)
    {
        InitializeStorm((uint)centre.x, (uint)centre.y, _radius, _duration, _averageRainfall, _dailyRainfallVariation, _date);
    }

    void InitializeStorm(uint centre_x, uint centre_y, uint _radius, uint _duration, float _averageRainfall, float _dailyRainfallVariation, System.DateTime _date)
    {
        centreCell = new uint[2];
        centreCell[0] = centre_x;
        centreCell[1] = centre_y;
        
        radius = _radius;
        duration = _duration;
        averageRainfall = _averageRainfall;
        dailyRainfallVariation = _dailyRainfallVariation;
        startDate = new System.DateTime(_date.Year, _date.Month, _date.Day, _date.Hour, _date.Minute, _date.Second); //TODO figure out if there is an easier way to deep copy objects in C#
    }
}