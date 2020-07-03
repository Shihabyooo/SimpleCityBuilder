using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HousingSlots
{
    public ulong low = 0;
    public ulong middle = 0;
    public ulong high = 0;
    
    public HousingSlots()
    {

    }

    public HousingSlots( ulong _low, ulong _middle, ulong _high)
    {
        low = _low;
        middle = _middle;
        high = _high;
    }

    public HousingSlots(HousingSlots newSlots)
    {
        AssignNew(newSlots);
    }

    public void AssignNew(HousingSlots newSlots) //since we can't overload the assignment op and we sometimes want to deep copy an object
    {
        low = newSlots.low;
        middle = newSlots.middle;
        high = newSlots.high;
    }

    public void SetSlotValue(ulong value, CitizenClass housingClass)
    {
        switch (housingClass)
        {
            case CitizenClass.low:
                low = value;
                break;
            case CitizenClass.middle:
                middle = value;
                break;
            case CitizenClass.high:
                high = value;
                break;
            default:
                break;
        }
    }

    public void IncrementSlotValue(ulong increment, CitizenClass housingClass)
    {
        switch (housingClass)
        {
            case CitizenClass.low:
                low += increment;
                break;
            case CitizenClass.middle:
                middle += increment;
                break;
            case CitizenClass.high:
                high += increment;
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
        return (low + middle + high); 
    }
    

    static public HousingSlots operator+ (HousingSlots slots1, HousingSlots slots2)
    {
        HousingSlots sum = new HousingSlots();

        sum.low = slots1.low + slots2.low;
        sum.middle = slots1.middle + slots2.middle;
        sum.high = slots1.high + slots2.high;

        return sum;
    }

    static public HousingSlots operator/ (HousingSlots slots, float denominator)
    {
        HousingSlots sum = new HousingSlots();

        sum.low =  System.Convert.ToUInt32(System.Convert.ToDouble(slots.low) / denominator);
        sum.middle = System.Convert.ToUInt32(System.Convert.ToDouble(slots.middle) / denominator) ;
        sum.high = System.Convert.ToUInt32(System.Convert.ToDouble(slots.high) / denominator) ;

        return sum;
    }
}
