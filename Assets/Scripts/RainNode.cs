using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RainNode : MonoBehaviour
{
    ParticleSystem pSystem;
    ParticleSystem.EmissionModule emissionModule;
    ParticleSystem.ShapeModule shapeModule;
    [SerializeField] float maxRateOverTimePerUnitRadius = 200.0f;
    [SerializeField] float minRateOverTimePerUnitRadius = 10.0f;
    [SerializeField] float rainRampUpTime = 5.0f; //time between rain start to reach its max value. Should be less than what it takes the simulation to update rainfall.
    public bool isStarting {get; private set;}

    public float opacity {get; private set;}
    public float rainfall {get; private set;}

    void Awake()
    {
        pSystem = this.gameObject.GetComponent<ParticleSystem>();
        emissionModule = pSystem.emission;
        shapeModule = pSystem.shape;
        
        emissionModule.rateOverTime = 0.0f;
        shapeModule.radius = 1.0f;

        opacity = pSystem.main.startColor.color.a;
        isStarting = false;
    }

    public void Initialize(float _rainfall, float radius)
    {
        shapeModule.radius = radius;
        //SetRainfall(_rainfall);
        StartCoroutine(RainStart(_rainfall));
    }

    public void SetRainfall(float _rainfall)
    {
        //emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(0.99f * rainfall, 1.01f * rainfall);
        rainfall = _rainfall;
        float rateOverTime = Mathf.Clamp(_rainfall, minRateOverTimePerUnitRadius, maxRateOverTimePerUnitRadius) * shapeModule.radius * 2.0f;
        emissionModule.rateOverTime = rateOverTime;
    }

    public void SetRainfallUnclamped(float _rainfall) //for use in fading out
    {
        rainfall = _rainfall;
        float rateOverTime = _rainfall * shapeModule.radius;
        emissionModule.rateOverTime = rateOverTime;
    }

    public void SetOpacity(float _opacity)
    {
        
        //Color newColour = pSystem.GetComponent<Renderer>().material.color;
        Color newColour = pSystem.main.startColor.color;
        
        newColour.a = opacity = _opacity;
        //pSystem.GetComponent<Renderer>().material.color = newColour;
        ParticleSystem.MainModule mainModule = pSystem.main;
        mainModule.startColor =  newColour;

    }


    IEnumerator RainStart(float _rainfall)
    {
        isStarting = true;
        float helperTimer = 0.0f;
        float maxOpacity = opacity;

        while (helperTimer < rainRampUpTime)
        {
            yield return new WaitForEndOfFrame();
            SetRainfall(_rainfall * (helperTimer / rainRampUpTime));
            SetOpacity (maxOpacity * (helperTimer / rainRampUpTime));
            helperTimer += Time.deltaTime;
        }

        yield return isStarting = false;
    }
    
}
