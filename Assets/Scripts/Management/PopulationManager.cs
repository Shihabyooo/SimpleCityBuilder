using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulationManager : MonoBehaviour
{
    //TODO implement a better DB system the performs well with the count of population we aim for.
    
    [SerializeField] //TODO remove this serialization when testing is done.
    List<Citizen> population = new List<Citizen>();

    static uint maxPopulation = 10000000; //10 million

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


    public void ProcessMigration()
    {
        
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

    public System.Guid homeAddress; //{get; private set;} 
    public System.Guid workAddress; //{get; private set;} 

}