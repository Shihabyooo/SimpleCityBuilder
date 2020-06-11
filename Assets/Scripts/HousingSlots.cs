using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HousingSlots
{
    public ulong poor = 0;
    public ulong low = 0;
    public ulong middle = 0;
    public ulong high = 0;
    public ulong obscene = 0;
    

    public HousingSlots()
    {

    }

    public HousingSlots(ulong _poor, ulong _low, ulong _middle, ulong _high, ulong _obscene)
    {
        poor = _poor;
        low = _low;
        middle = _middle;
        high = _high;
        obscene = _obscene;
    }

    public HousingSlots(HousingSlots newSlots)
    {
        AssignNew(newSlots);
    }

    public void AssignNew(HousingSlots newSlots) //since we can't overload the assignment op and we sometimes want to deep copy an object
    {
        poor = newSlots.poor;
        low = newSlots.low;
        middle = newSlots.middle;
        high = newSlots.high;
        obscene = newSlots.obscene;
    }

    public void SetSlotValue(ulong value, HousingClass housingClass)
    {
        switch (housingClass)
        {
            case HousingClass.poor:
                poor = value;
                break;
            case HousingClass.low:
                low = value;
                break;
            case HousingClass.middle:
                middle = value;
                break;
            case HousingClass.high:
                high = value;
                break;
            case HousingClass.obscene:
                obscene = value;
                break;
            default:
                break;
        }
    }

    public void IncrementSlotValue(ulong increment, HousingClass housingClass)
    {
        switch (housingClass)
        {
            case HousingClass.poor:
                poor += increment;
                break;
            case HousingClass.low:
                low += increment;
                break;
            case HousingClass.middle:
                middle += increment;
                break;
            case HousingClass.high:
                high += increment;
                break;
            case HousingClass.obscene:
                obscene += increment;
                break;
            default:
                break;
        }
    }

    public ulong Sum()
    {
        //This is stupid
        // ulong sum = poor + low;
        // if (sum < poor || sum < low)
        //     return ulong.MaxValue;
        
        // sum += middle;
        // if (sum < poor + low || sum < middle)
        //     return ulong.MaxValue;

        // sum += high;
        // if (sum < poor + low + middle || sum < high)
        //     return ulong.MaxValue;
        
        // sum += obscene;
        // if (sum < poor + low + middle + high  || sum < obscene)
        //     return ulong.MaxValue;

        // return sum;


        //TODO find a way to limit the total number of housing slots to something that wouldn't cause ulong value to cycle back.
        return (poor + low + middle + high + obscene); 
    }
    
}
