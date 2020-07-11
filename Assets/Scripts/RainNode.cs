using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RainNode : MonoBehaviour
{
    ParticleSystem rainSystem, cloudSystem;
    ParticleSystem.EmissionModule rainEmission, cloudEmission;
    ParticleSystem.ShapeModule rainShape, cloudShape;
    
    [SerializeField] float maxRateOverTimePerUnitRadius = 200.0f;
    [SerializeField] float minRateOverTimePerUnitRadius = 10.0f;
    [SerializeField] float rainToCloudEmissionRatio = 0.5f;
    [SerializeField] float rainRampUpTime = 5.0f; //time between rain start to reach its max value. Should be less than what it takes the simulation to update rainfall.
    
    public bool isStarting {get; private set;}
    public float rainOpacity {get; private set;}
    public float cloudOpacity {get; private set;}
    public float rainfall {get; private set;}

    void Awake()
    {
        rainSystem = this.gameObject.GetComponent<ParticleSystem>();
        cloudSystem = this.transform.Find("CloudNode").gameObject.GetComponent<ParticleSystem>();
        
        rainEmission = rainSystem.emission;
        cloudEmission = cloudSystem.emission;
        
        rainShape = rainSystem.shape;
        cloudShape = cloudSystem.shape;
        
        rainEmission.rateOverTime = 0.0f;
        rainShape.radius = 1.0f;

        cloudEmission.rateOverTime = 0.0f;
        cloudShape.radius = rainShape.radius + 1.0f;

        rainOpacity = rainSystem.main.startColor.color.a;
        cloudOpacity = cloudSystem.main.startColor.color.a;
        
        isStarting = false;
    }

    public void Initialize(float _rainfall, float radius)
    {
        rainShape.radius = radius;
        cloudShape.radius = rainShape.radius + 1.0f;
        StartCoroutine(RainStart(_rainfall));
    }

    public void SetRainfall(float _rainfall)
    {
        rainfall = _rainfall;
        float rateOverTimePerUnitRadius = Mathf.Clamp(_rainfall, minRateOverTimePerUnitRadius, maxRateOverTimePerUnitRadius);

        rainEmission.rateOverTime = rateOverTimePerUnitRadius  * rainShape.radius * 2.0f;
        cloudEmission.rateOverTime = rateOverTimePerUnitRadius * rainToCloudEmissionRatio * cloudShape.radius; 
    }

    public void SetRainfallUnclamped(float _rainfall) //for use in fading out
    {
        rainfall = _rainfall;
        float rateOverTime = _rainfall * rainShape.radius;
        rainEmission.rateOverTime = rateOverTime;
    }

    public void SetRainOpacity(float _opacity)
    {        
        Color newColour = rainSystem.main.startColor.color;
        newColour.a = rainOpacity = _opacity;

        ParticleSystem.MainModule mainModule = rainSystem.main;
        mainModule.startColor =  newColour;
    }

    public void SetCloudOpacity(float _opacity)
    {        
        Color newColour = cloudSystem.main.startColor.color;
        newColour.a = cloudOpacity = _opacity;

        ParticleSystem.MainModule mainModule = cloudSystem.main;
        mainModule.startColor =  newColour;
    }

    IEnumerator RainStart(float _rainfall)
    {
        isStarting = true;
        float helperTimer = 0.0f;
        float maxOpacity = rainOpacity;

        while (helperTimer < rainRampUpTime)
        {
            yield return new WaitForEndOfFrame();
            
            SetRainfall(_rainfall * (helperTimer / rainRampUpTime));
            SetRainOpacity (maxOpacity * (helperTimer / rainRampUpTime));
            helperTimer += Time.deltaTime;
        }

        yield return isStarting = false;
    }
}
