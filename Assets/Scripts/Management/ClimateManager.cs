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
    [SerializeField] GameObject rainfallNodePrefab;
    [SerializeField] GameObject smogNodePrefab;
    [SerializeField] List<Storm> activeStorms = new List<Storm>(); //serialization for testing only.
    GameObject[,] smogNodes;// = new GameObject[Grid.grid.noOfCells.x, Grid.grid.noOfCells.y];
    [SerializeField] float cloudHeight = 10.0f;
    [SerializeField] float smogHeight = 2.0f;
    bool isShowingPollution = false;
    [SerializeField] float minPollutionToVisualize = 150.0f;
    [SerializeField] float maxPollutionToVisualize = 1000.0f;


    public bool isRaining {get; private set;}

    void Awake()
    {
        isRaining = false;
        SimulationManager.onTimeUpdate += UpdateClimateHour;
    }

    void Start()
    {
        smogNodes = new GameObject[Grid.grid.noOfCells.x, Grid.grid.noOfCells.y];
        
        float smogPSystemHeight = smogNodePrefab.GetComponent<ParticleSystem>().shape.scale.y;
        for (uint i = 0; i < Grid.grid.noOfCells.x; i++)
            for (uint j = 0; j < Grid.grid.noOfCells.y; j++)
                smogNodes[i, j] = GameObject.Instantiate(smogNodePrefab,
                                                        Grid.grid.GetCellPosition(i, j) + new Vector3(0.0f, smogHeight + smogPSystemHeight / 2.0f, 0.0f),
                                                        Grid.grid.transform.rotation,
                                                        this.transform);
        
        TogglePollutionDisplay(false);
    }

    public void UpdateClimateDay(System.DateTime date)
    {
       ProcessRainfall(date);
    }

    public void UpdateClimateHour(int timeFrame)
    {
        TransportPollution(timeFrame);

        if (isShowingPollution)
            UpdatePollutionNodes();
    }


//Rainfall
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

        for (int i = 0; i < noNewOfStorms; i++)
        {
            Vector2 stormCentre = new Vector2(Random.Range(1, Grid.grid.noOfCells.x - 1), Random.Range(1, Grid.grid.noOfCells.y - 1));
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
                }
                else
                {
                    //generate today's rainfall
                    float todaysRainfall = Random.Range(storm.averageRainfall - storm.dailyRainfallVariation, storm.averageRainfall + storm.dailyRainfallVariation);
                    //update rainfall layer
                    Grid.grid.SetRainfallCummulative(storm.centreCell[0], storm.centreCell[1], storm.radius, todaysRainfall);
                    //update particlesystem
                    storm.rainNode.SetRainfall(todaysRainfall);
                }
            }
        }

        //Second loop to remove storms that are ending
        //The foeach loop can't have item removal inside it. This is a workaround. Another one is to nix the entire foreach loop and replace it with the one bellow.
        //TODO look into the proposal above
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
        float baseRainOpacity = storm.rainNode.rainOpacity;
        float baseCloudOpacity = storm.rainNode.cloudOpacity;
        
        float baseRainfall = storm.rainNode.rainfall;

        while (helperTimer < stormFadeOutPeriod)
        {
            yield return new WaitForEndOfFrame();
            storm.rainNode.SetRainfall(baseRainfall * (1 - helperTimer / stormFadeOutPeriod));
            storm.rainNode.SetRainOpacity(baseRainOpacity * (1 - helperTimer / stormFadeOutPeriod));
            storm.rainNode.SetCloudOpacity(baseCloudOpacity * (1 - helperTimer / stormFadeOutPeriod));
            helperTimer += Time.deltaTime;
        }

        Destroy(storm.rainNode.gameObject);
        yield return null;
    }


//Pollution
    public void AddPollution(uint cellID_x, uint cellID_y, float volume)
    {
        //Grid.grid.pollutionLayer.SetCellValue(cellID_x, cellID_y, volume);
        Grid.grid.SetPollutionCummilative(cellID_x, cellID_y, 1, volume);
    }

    const float pollutionTransportPowerFactor = 2.2f;
    const float pollutionTransportDenominator = 10.0f;
    const float maxTransportPercent = 0.2f;
    void TransportPollution(int timeFrame) //happens on a daily basis (24 hours).
    {
        //We need to copy the existing the pollution layer to avoid having changes we made in the loop bellow affect calculation of the next cells in the same loop.
        //The other way is to calculate to the new layer, then add its values to the original layer at the end. 
        GridLayer<float> newPollutionLayer = new GridLayer<float>((uint)Grid.grid.noOfCells.x, (uint)Grid.grid.noOfCells.y);
        Grid.grid.pollutionLayer.CopyToLayer(newPollutionLayer);

        for(uint i = 0; i < Grid.grid.noOfCells.x; i++)
        {
            for(uint j = 0; j < Grid.grid.noOfCells.y; j++)
            {
                float pollution = newPollutionLayer.GetCellValue(i, j);
                if (pollution >= 0.01f)
                {
                    uint windDir = Grid.grid.windDirectionLayer.GetCellValue(i, j);
                    float windSpeed = Grid.grid.windSpeedLayer.GetCellValue(i, j);
                    
                    float sinDegree = Mathf.Sin(Mathf.Deg2Rad * windDir);
                    float cosDegree = Mathf.Cos(Mathf.Deg2Rad * windDir);
                    
                    long[,] cells = new long[2,2]; 
                    cells[0,0] = ((long)i + (cosDegree > 0.001f? Mathf.CeilToInt(cosDegree) : Mathf.FloorToInt(cosDegree)));
                    cells[0,1] = j;
                    cells[1,0] = i;
                    cells[1,1] = ((long)j + (sinDegree > 0.001f? Mathf.CeilToInt(sinDegree) : Mathf.FloorToInt(sinDegree)));

                    float[] cellsShare = new float[2];
                    cellsShare[0] = Mathf.Pow(cosDegree, 2.0f);
                    cellsShare[1] = Mathf.Pow(sinDegree, 2.0f);
                    //print ("Transporting from " + i + "," + j + " to " + cells[0,0] + "," + cells[0,1] + " and " + cells[1,0] + "," + cells[1,1]);

                    float transportPercentage = (Mathf.Pow(pollution, pollutionTransportPowerFactor - 1.0f) / Mathf.Pow(pollutionTransportDenominator, pollutionTransportPowerFactor)) / pollution;
                    transportPercentage = transportPercentage * windSpeed / 30.0f; //assuming max windspeed in game is set to 30.0f.
                    transportPercentage = Mathf.Min(transportPercentage, maxTransportPercent);
                    
                    float transportedVolume = transportPercentage * pollution * timeFrame;

                    Grid.grid.pollutionLayer.GetCellRef(i, j) -= transportedVolume;

                    if (cells[0,0] > 0 && cells[0,0] < Grid.grid.noOfCells.x && cells[0,1] > 0 && cells[0,1] < Grid.grid.noOfCells.y)
                        Grid.grid.pollutionLayer.GetCellRef((uint)cells[0,0], (uint)cells[0,1]) += cellsShare[0] * transportedVolume;
                    
                    if (cells[1,0] > 0 && cells[1,0] < Grid.grid.noOfCells.x && cells[1,1] > 0 && cells[1,1] < Grid.grid.noOfCells.y)
                        Grid.grid.pollutionLayer.GetCellRef((uint)cells[1,0], (uint)cells[1,1]) += cellsShare[1] * transportedVolume;
                }
            }
        }
    }


    public void TogglePollutionDisplay(bool state)
    {
        isShowingPollution = state;
        for (uint i = 0; i < Grid.grid.noOfCells.x; i++)
            for (uint j = 0; j < Grid.grid.noOfCells.y; j++)
            {
                // ParticleSystem.EmissionModule emissionModule = smogNodes[i, j].GetComponent<ParticleSystem>().emission;
                // emissionModule.enabled = state;
                smogNodes[i, j].SetActive(state);
            }

    }

    public void UpdatePollutionNodes()
    {
        if (!isShowingPollution)
            return;

        for (uint i = 0; i < Grid.grid.noOfCells.x; i++)
            for (uint j = 0; j < Grid.grid.noOfCells.y; j++)
            {
                GameObject node = smogNodes[i, j];                
                float pollutionAtCell = Grid.grid.pollutionLayer.GetCellValue(i, j);

                if(pollutionAtCell < minPollutionToVisualize)
                {
                    node.SetActive(false);
                }
                else
                {
                    node.SetActive(true);
                    ParticleSystem.MainModule mainModule = node.GetComponent<ParticleSystem>().main;
                    Color newColour = smogNodePrefab.GetComponent<ParticleSystem>().main.startColor.color;
                    newColour.a = newColour.a * Mathf.Clamp((pollutionAtCell - minPollutionToVisualize) / (maxPollutionToVisualize - minPollutionToVisualize), 0.0f, 1.0f);
                    mainModule.startColor = newColour;
                }
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
        startDate = new System.DateTime(_date.Year, _date.Month, _date.Day, _date.Hour, _date.Minute, _date.Second);
        rainNode = _rainNode; //We know that the original List<RainNode> is temporarily created (outside this class) and will not have a ref living other than in this class.

        isEnding = false;
    }

    public void MarkEnding()
    {
        isEnding = true;
    }

}