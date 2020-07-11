using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Gender
{
    male, female
} //FITE ME!!!!

public enum CitizenClass
{
    low, middle, high
} //FITE ME!

public enum EducationLevel
{
    illiterate = 1, primary = 2, secondery = 4, tertiary = 8
}

[System.Serializable]
public class Citizen
{
    const long maxSavings = 100000; //worst case difference is 9,223,372,036,854,775,807 - 2,147,483,647 = 9,223,372,034,707,292,160‬ before the savings overflow.
                                        //however, to add some difficulty (i.e. to prevent player from benefiting from a successfull period of good choice for a long time),
                                        //we set a max savings after which, any more gains are pointless. So if citizen lost job or income became too low, they won't stick around
                                        //for long
    
    //TODO reset the {get; private set;} once testing is done.
    public System.Guid id; //{get; private set;}
    public System.DateTime birthDay; //{get; private set;}
    public CitizenClass citizenClass; //{get; private set;} 
    public EducationLevel educationalLevel; //{get; private set;} 
    public Happiness happiness; //{get; private set;}  
    //public float income; //{get; private set;} 
    public long savings; //{get; private set;} 
    public int health; //{get; private set;} 

    public ResidentialBuilding homeAddress; //{get; private set;} 
    public WorkPlace workAddress; //{get; private set;} 

    public Gender gender; //{get; private set;} 
    public Citizen spouse; //{get; private set;} 

    public const int happinessChangeRatePerDay = 1;
    public const int lifeStyleExpensesPerDayLow = 25; //in units of money per day
    public const int lifeStyleExpensesPerDayMiddle = 50; //in units of money per day
    public const int lifeStyleExpensesPerDayHigh = 250; //in units of money per day

    public const int minHealthBeforeSeekingHospitals = 50;

    public const float minPollutionToAffectHealthHappiness = 100.0f;
    public const float maxPollutionToAffectHealthHappiness = 2000.0f;

    public Citizen()
    {
        //isInDebt = false;
        spouse = null;
        workAddress = null;
        health = 100;
    }

    public bool ProcessFinances() //Must be called once per day. Returns false if citizen can pay their expenses.
    {
        long income = 0;
        long expenses = 0;
        
        //get paid
        if (workAddress != null)        
            income += workAddress.Wages();
        
        //add rent to expenses
        expenses += homeAddress.Rent();

        //add other life expenses
        switch (citizenClass)
        {
            case CitizenClass.low:
                expenses += lifeStyleExpensesPerDayLow;
                break;
            case CitizenClass.middle:
                expenses += lifeStyleExpensesPerDayMiddle;
                break;
            case CitizenClass.high:
                expenses += lifeStyleExpensesPerDayHigh;
                break;
        }

        expenses -= income; //if income > expenses, result will be negative, which will increase savings.
        savings = System.Convert.ToInt64(Mathf.Min(savings - expenses, maxSavings)); 
        
        if (savings < 0)
        {
            //AssignDebt();
            return false;
        }

        return true;
    }

    public void SubstractTax(int tax)
    {
        //leave checking whether citizen is broke to the ProcessFinances() call.
        savings -= tax;
    }

    public void Lookups() //searches for work, health if needed, etc.s
    {
        if (workAddress == null)
        {
            WorkPlace workPlace = GameManager.buildingsMan.GetEmptyWorkSlot(educationalLevel);
            if (workPlace != null) //contrary to the home case, this is a possibility.
            {
                workAddress = workPlace;
                workPlace.AssignEmployee(this);
            }
        }
        if (health < minHealthBeforeSeekingHospitals)
        {
            //TODO handle hopsital lookups and visits here.
        }
    }
    
    // void AssignDebt()
    // {
    //     isInDebt = true;

    // }

    // void ProcessDebt()
    // {

    // }

    public void Emigrate()
    {
        if (homeAddress !=null)
            homeAddress.RemoveResident(this);

        if (workAddress != null)
            workAddress.RemoveEmployee(this);
    }
}