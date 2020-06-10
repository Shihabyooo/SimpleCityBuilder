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
    [SerializeField] int minStormDuration = 1;
    [SerializeField] int minStormRadius = 2;
    [SerializeField] List<Storm> activeStorms = new List<Storm>();
    [SerializeField] GameObject rainfallNodePrefab;
    [SerializeField] float cloudHeight = 10.0f;

    public bool isRaining {get; private set;}

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

        if (activeStorms.Count < 1)
            isRaining = false;
    }

    void SpawnStorms(System.DateTime date)
    {
        isRaining = true;
        int noNewOfStorms = Mathf.Min(Random.Range(1, maxNoOfActiveStorms), maxNoOfActiveStorms - activeStorms.Count);
        //print ("Spawning: " + noNewOfStorms + " storms"); //test

        for (int i = 0; i < noNewOfStorms; i++)
        {
            Vector2 stormCentre = new Vector2(Random.Range(1, Grid.grid.noOfCells.x - 1), Random.Range(1, Grid.grid.noOfCells.y - 1));
            //uint stormRadius = (uint)Random.Range(1, Mathf.RoundToInt((float)Mathf.Min(Grid.grid.noOfCells.x, Grid.grid.noOfCells.y) / 4.0f));
            uint stormRadius = (uint)Random.Range(minStormRadius, Mathf.RoundToInt((float)Mathf.Min(Grid.grid.noOfCells.x, Grid.grid.noOfCells.y) / 2.0f));
            uint stormDuration = (uint)Random.Range(minStormDuration, maxStormDuration);
            float averageStormRainfallAtCentre = Random.Range( Mathf.Max(averageDailyRainfallPerMonth[date.Month - 1] - maxVariationInRainfall * averageDailyRainfallPerMonth[date.Month - 1], 0.0f),
                                            averageDailyRainfallPerMonth[date.Month - 1] + maxVariationInRainfall * averageDailyRainfallPerMonth[date.Month - 1]);
            float dailyStormRainfallVariation = maxVariationInRainfall * averageStormRainfallAtCentre;

            Vector3 stormRainNodePosition = Grid.grid.GetCellPosition((uint)stormCentre.x, (uint)stormCentre.y);
            stormRainNodePosition.y += cloudHeight;
            GameObject stormRainNodeObj = GameObject.Instantiate(rainfallNodePrefab, stormRainNodePosition, this.transform.rotation, this.transform);
            stormRainNodeObj.GetComponent<RainNode>().Initialize(averageStormRainfallAtCentre, stormRadius + 1);

            Storm newStorm = new Storm(stormCentre, stormRadius, stormDuration, averageStormRainfallAtCentre, dailyStormRainfallVariation, date, stormRainNodeObj.GetComponent<RainNode>());
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
            if (!storm.isEnding)
            {
                float activeDays = date.Subtract(storm.startDate).Days;
                if (activeDays > storm.duration)
                {
                    EndStorm(storm);
                    //activeStorms.Remove(storm);
                }
                else
                {
                    //generate today's rainfall
                    float todaysRainfall = Random.Range(storm.averageRainfall - storm.dailyRainfallVariation, storm.averageRainfall + storm.dailyRainfallVariation);
                    //update rainfall layer
                    Grid.grid.SetRainfallCummulitive(storm.centreCell[0], storm.centreCell[1], storm.radius, todaysRainfall);
                    //update particlesystem
                    storm.rainNode.SetRainfall(todaysRainfall);
                }
            }
        }

        //Second loop to remove storms that are ending
        //The foeach loop can't have item removal inside it. This is a workaround. Another one is to nix the entire foreach loop and replace it with the one bellow.
        for (int i = activeStorms.Count - 1; i >= 0; i--)
        {
            if (activeStorms[i].isEnding)
            {
                activeStorms.Remove(activeStorms[i]);
            }
        }
    }

    void EndStorm(Storm storm)
    {
        storm.MarkEnding(); //Useless?
        StartCoroutine(StormEnd(storm));
    }

    float stormFadeOutPeriod = 3.0f;    
    IEnumerator StormEnd(Storm storm)
    {
        float helperTimer = 0.0f;
        float baseOpacity = storm.rainNode.opacity;
        float baseRainfall = storm.rainNode.rainfall;

        while (helperTimer < stormFadeOutPeriod)
        {
            yield return new WaitForEndOfFrame();
            storm.rainNode.SetRainfall(baseRainfall * (1 - helperTimer / stormFadeOutPeriod));
            storm.rainNode.SetOpacity(baseOpacity * (1 - helperTimer / stormFadeOutPeriod));
            helperTimer += Time.deltaTime;
        }

        Destroy(storm.rainNode.gameObject);
        yield return null;
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
    public RainNode rainNode;// {get; private set;}
    public bool isEnding; // {get; private set;} //Useless?

    public Storm(uint centre_x, uint centre_y, uint _radius, uint _duration, float _averageRainfall, float _dailyRainfallVariation, System.DateTime _date, RainNode _rainNode)
    {
        InitializeStorm(centre_x, centre_y, _radius, _duration, _averageRainfall, _dailyRainfallVariation, _date, _rainNode);
    }

    public Storm(Vector2 centre, uint _radius, uint _duration, float _averageRainfall, float _dailyRainfallVariation, System.DateTime _date, RainNode _rainNode)
    {
        InitializeStorm((uint)centre.x, (uint)centre.y, _radius, _duration, _averageRainfall, _dailyRainfallVariation, _date, _rainNode);
    }

    void InitializeStorm(uint centre_x, uint centre_y, uint _radius, uint _duration, float _averageRainfall, float _dailyRainfallVariation, System.DateTime _date, RainNode _rainNode)
    {
        centreCell = new uint[2];
        centreCell[0] = centre_x;
        centreCell[1] = centre_y;
        
        radius = _radius;
        duration = _duration;
        averageRainfall = _averageRainfall;
        dailyRainfallVariation = _dailyRainfallVariation;
        startDate = new System.DateTime(_date.Year, _date.Month, _date.Day, _date.Hour, _date.Minute, _date.Second); //TODO figure out if there is an easier way to deep copy objects in C#
        rainNode = _rainNode; //We know that the original List<RainNode> is temporarily created (outside this class) and will not have a ref living other than in this class.

        isEnding = false;
    }

    public void MarkEnding()
    {
        isEnding = true;
    }

}