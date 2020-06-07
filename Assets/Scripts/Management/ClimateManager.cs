using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimateManager : MonoBehaviour //for the sake of simplicity, assume "climate" and "weather" to be synonyms..
{
    [SerializeField] float[] averageDailyRainfallPerMonth = new float[12]; //in units total depth rainfall per day (preferably mm if using SI-ish units in the project).
    [SerializeField] [Range(0.0f, 1.0f)] float[] dailyRainProbabilityPerMonth = new float [12]; //in any given day of the month, how probable will it precipitate.
    [SerializeField] float maxVariationInRainfall = 0.2f; //the range (plus or minus the average, as a fraction of the average) in which the rainfall will be randomized.

    [SerializeField] bool isRaining = false; //serialized for testing only.
    public float currentRainfall {get; private set;}

    public void UpdateClimate(System.DateTime date)
    {
        float dieThrow = Random.Range(0.0f, 1.0f);
        print ("die throw: " + dieThrow);
        
        if (dieThrow <= dailyRainProbabilityPerMonth[date.Month - 1])
            StartRain(date.Month - 1);
        else
            StopRain();
    }

    void StartRain(int month) //month starting from 0 to 11;
    {
        isRaining = true;
        currentRainfall = Random.Range(Mathf.Clamp(averageDailyRainfallPerMonth[month] - maxVariationInRainfall * averageDailyRainfallPerMonth[month], 0.0f, float.MaxValue),
                                        averageDailyRainfallPerMonth[month] + maxVariationInRainfall * averageDailyRainfallPerMonth[month]);
    }

    void StopRain()
    {
        isRaining = false;
        currentRainfall = 0.0f;
    }

}
