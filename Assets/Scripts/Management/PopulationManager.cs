using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    //TODO implement a better DB system the performs well with the count of population we aim for.
    
     
    [SerializeField] List<Citizen> population = new List<Citizen>(); //TODO remove this serialization when testing is done.
    [SerializeField] PopulationGrowthMetrics growthStats = new PopulationGrowthMetrics();
    [SerializeField] Happiness populationHappiness = new Happiness(50);
    static uint maxPopulation = 10000000; //10 million
    static int maxImmigrationRate = 100; //citizens per day.

    public int PopulationCount()
    {
        return population.Count;
    }

    public bool IsPopulationFull()
    {
        return population.Count >= maxPopulation;
    }

    public bool AddCitizen(Citizen citizen)
    {
        if (IsPopulationFull())
            return false;

        population.Add(citizen);
        return true;
    }

    public void UpdateCitizenStats(System.Guid id, Citizen newStats)
    {
        foreach (Citizen citizen in population)
        {
            if (citizen.id == id)
            {
                citizen.birthDay = newStats.birthDay;
                citizen.educationalLevel = newStats.educationalLevel;
                citizen.happiness = newStats.happiness;
                citizen.income = newStats.income;

                citizen.homeAddress = newStats.homeAddress;
                citizen.workAddress = newStats.workAddress;
            }
        }
    }

    public void UpdateHappiness()
    {
        if (population.Count < 1)
        {
            populationHappiness = new Happiness(0);
            return;
        }

        ulong overallHappiness = 0;
        ulong healthHappines  = 0;
        ulong homeHappines = 0;
        ulong jobHappines = 0;
        ulong environmentHappiness = 0;

        foreach(Citizen citizen in population)
        {
            //compute citizen's happiness here.

            //add citizen's happines to sum
            overallHappiness += citizen.happiness.overall;
            healthHappines += citizen.happiness.health;
            homeHappines += citizen.happiness.home;
            jobHappines += citizen.happiness.job;
            environmentHappiness += citizen.happiness.environment;
        }

        int _populationCount = population.Count;
        populationHappiness.overall = (uint)Mathf.RoundToInt((float)overallHappiness / (float)_populationCount);
        populationHappiness.health = (uint)Mathf.RoundToInt((float)healthHappines / (float)_populationCount);
        populationHappiness.home = (uint)Mathf.RoundToInt((float)homeHappines / (float)_populationCount);
        populationHappiness.job = (uint)Mathf.RoundToInt((float)jobHappines / (float)_populationCount);
        populationHappiness.environment = (uint)Mathf.RoundToInt((float)environmentHappiness / (float)_populationCount);
    }

    public void ProcessMigration() //called once per day.
    {
        //compute immigration rate based on happiness and empty housing.
        HousingSlots availableHousing = GameManager.resourceMan.AvailableHousing();
        ulong totalHousing = availableHousing.Sum();

        int _immigration = Mathf.RoundToInt(((float)populationHappiness.overall / 100.0f) * (0.8f * maxImmigrationRate));
        _immigration = Random.Range(Mathf.RoundToInt(_immigration - 0.2f * maxImmigrationRate),  Mathf.RoundToInt(_immigration + 0.2f * maxImmigrationRate));
        _immigration = (int)Mathf.Min(_immigration, maxImmigrationRate, totalHousing);


        if (growthStats.immigrationRate > 0 && totalHousing > 0)
        {
            //Spawn new citizens here.
            //This is a test implementation
            float poorRatio = availableHousing.poor / totalHousing;
            float lowRatio = availableHousing.low / totalHousing;
            float middleRatio = availableHousing.middle / totalHousing;
            float highRatio = availableHousing.high / totalHousing;
            float obsceneRatio = availableHousing.obscene / totalHousing;
           

            for (int i = 0; i < Mathf.FloorToInt(_immigration * poorRatio); i++)
            {
                GenerateImmigrationCase(HousingClass.poor);               
            }
            for (int i = 0; i < Mathf.FloorToInt(_immigration * lowRatio); i++)
            {
                GenerateImmigrationCase(HousingClass.low);
            }
            for (int i = 0; i < Mathf.FloorToInt(_immigration * middleRatio); i++)
            {
                GenerateImmigrationCase(HousingClass.middle);
            }
            for (int i = 0; i < Mathf.FloorToInt(_immigration * highRatio); i++)
            {
                GenerateImmigrationCase(HousingClass.high);
            }
            for (int i = 0; i < Mathf.FloorToInt(_immigration * obsceneRatio); i++)
            {
                GenerateImmigrationCase(HousingClass.obscene);
            }
            
        }
        
        growthStats.immigrationRate = _immigration;
    }

    Citizen GenerateCitizen(HousingClass _class)
    {
        //Housing, work and income will not be set here.

        Citizen newCitizen = new Citizen();
        newCitizen.happiness = new Happiness(50);
        newCitizen.id = System.Guid.NewGuid();
        
        //uint ageMin = 18;
        //uint ageMax = 40;
        float[] educationLevelPropability = new float[4] {0.25f, 0.5f, 0.75f, 1.0f}; //4 elements conforming to EducationLevel order.
        int averageAge = 35;
        int ageRange = 10;

        long _savings = 1000;

        //public long savings; //{get; private set;} 

        switch (_class)
        {
            case HousingClass.poor:
                educationLevelPropability = new float[4] {0.4f, 1.0f, 1.1f, 1.1f};  //Any element following one with 1.0f probability will never happen (check the if-statements bellow)
                                                                                //This translates to: citizen has 60% chance of being illterate, 40% of having primary education.
                averageAge = 28;
                ageRange = 10;
                _savings = 500;
                break;
            case HousingClass.low:
                educationLevelPropability = new float[4] {0.1f, 0.25f, 0.9f, 1.0f};
                averageAge = 32;
                ageRange = 14;
                _savings = 1000;
                break;
            case HousingClass.middle:
                educationLevelPropability = new float[4] {0.0f, 0.0f, 0.55f, 1.0f};
                averageAge = 35;
                ageRange = 15;
                _savings = 5000;
                break;
            case HousingClass.high:
                educationLevelPropability = new float[4] {0.0f, 0.0f, 0.1f, 1.0f};
                averageAge = 35;
                ageRange = 10;
                _savings = 20000;
                break;
            case HousingClass.obscene:
                educationLevelPropability = new float[4] {0.0f, 0.0f, 0.05f, 1.0f};
                averageAge = 40;
                ageRange = 10;
                _savings = 100000;
                break;
        }

        float dieRoll = Random.Range(0.0f, 1.0f);

        if (dieRoll < educationLevelPropability[0])
            newCitizen.educationalLevel = EducationLevel.illetarte;
        else if (dieRoll < educationLevelPropability[1])
            newCitizen.educationalLevel = EducationLevel.primary;
        else if (dieRoll < educationLevelPropability[2])
            newCitizen.educationalLevel = EducationLevel.secondery;
        else if (dieRoll < educationLevelPropability[3])
            newCitizen.educationalLevel = EducationLevel.tertiary;

        newCitizen.birthDay = new System.DateTime(GameManager.simMan.date.Year - Random.Range(averageAge - ageRange, averageAge + ageRange), Random.Range(1,12), Random.Range(1,28));
        newCitizen.savings = _savings;

        return newCitizen;
    }

    void GenerateImmigrationCase(HousingClass _class)
    {
    //for each citizen class:
                //generate citizen.
                //assign home.
                //assign work.
                //assign income based on work.

        Citizen newCitizen = GenerateCitizen(_class);
        
        ResidentialBuilding home = GameManager.buildingsMan.GetResidentialBuildingWithEmptySlot(_class);
        if (home != null) //this shouldn't fail.
        {
            newCitizen.homeAddress = home.uniqueID;
            home.AddResident(newCitizen.id);
        }

        //TODO assign work
        //TODO compute income.

        population.Add(newCitizen);
    }
}


public enum EducationLevel
{
    illetarte, primary, secondery, tertiary
}

[System.Serializable]
public struct Happiness
{
    //From 0 to 100
    public uint overall;
    public uint health;
    public uint home;
    public uint job;
    public uint environment;

    public Happiness(uint fixedValue)
    {
        overall = health = home = job = environment = fixedValue;
    }
}

[System.Serializable]
public class Citizen
{
    //TODO reset the {get; private set;} once testing is done.
    
    public System.Guid id; //{get; private set;}
    public System.DateTime birthDay; //{get; private set;}
    public EducationLevel educationalLevel; //{get; private set;} 
    public Happiness happiness; //{get; private set;}  
    public float income; //{get; private set;} 
    public long savings; //{get; private set;} 

    public System.Guid homeAddress; //{get; private set;} 
    public System.Guid workAddress; //{get; private set;} 

}

[System.Serializable]
public class PopulationGrowthMetrics
{
    public int immigrationRate = 0; //in citizens per day. Positive for immigration, negative for emigration.
    public ulong birthRate = 0; //in citizens per day.
    public ulong deathRate = 0; //in citizens per day.

}