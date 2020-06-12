using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Gender
{
    male, female
}

[System.Serializable]
public class Citizen
{
    //TODO reset the {get; private set;} once testing is done.
    
    public System.Guid id; //{get; private set;}
    public System.DateTime birthDay; //{get; private set;}
    public HousingClass citizenClass; //{get; private set;} 
    public EducationLevel educationalLevel; //{get; private set;} 
    public Happiness happiness; //{get; private set;}  
    //public float income; //{get; private set;} 
    public long savings; //{get; private set;} 

    //public System.Guid homeAddress; //{get; private set;} 
    public ResidentialBuilding homeAddress; //{get; private set;} 
    public WorkPlace workAddress; //{get; private set;} 

    //public bool isInDebt;  //{get; private set;} 

    public const int happinessChangeRatePerDay = 1;
    public const int lifeStyleExpensesPerDayPoor = 10; //in units of money per day
    public const int lifeStyleExpensesPerDayLow = 25; //in units of money per day
    public const int lifeStyleExpensesPerDayMiddle = 50; //in units of money per day
    public const int lifeStyleExpensesPerDayHigh = 250; //in units of money per day
    public const int lifeStyleExpensesPerDayObscene = 500; //in units of money per day

    public Gender gender;
    public Citizen spouse; //{get; private set;} 



    public Citizen()
    {
        //isInDebt = false;
        spouse = null;
        workAddress = null;
    }

    public bool ProcessFinances() //Must be called once per day. Returns false if citizen can pay their expenses.
    {
        long income = 0;
        long expenses = 0;
        
        //get paid
        if (workAddress != null)
        {
            income += workAddress.Wages();
        }
        

        //add rent to expenses
        expenses += homeAddress.Rent();

        //add other life expenses
        switch (citizenClass)
        {
            case HousingClass.poor:
                expenses += lifeStyleExpensesPerDayPoor;
                break;
            case HousingClass.low:
                expenses += lifeStyleExpensesPerDayLow;
                break;
            case HousingClass.middle:
                expenses += lifeStyleExpensesPerDayMiddle;
                break;
            case HousingClass.high:
                expenses += lifeStyleExpensesPerDayHigh;
                break;
            case HousingClass.obscene:
                expenses += lifeStyleExpensesPerDayObscene;
                break;
        }

        //TODO handle cases of integer overflow/cycling.
        expenses -= income; //if income > expenses, result will be negative, which will increase savings.
        savings -= expenses;
        
        if (savings < 0)
        {
            //AssignDebt();
            return false;
        }

        return true;
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

        //TODO add call to remove citizen from work here.
    }
}
