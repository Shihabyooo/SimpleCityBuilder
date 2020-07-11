using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Building))]
public class WorkPlace : MonoBehaviour
{
    const uint baseWagesIlliterate = 85;
    const uint baseWagesPrimary = 115;
    const uint baseWagesSecondary = 345;
    const uint baseWagesTertiary = 1150;

    [SerializeField] uint minManpower = 0; //minimum amount of workers required for this building to operate.
    [SerializeField] uint maxManpower = 0; //maximum amount of workers this building can handle. Production is max when currentWorkersCount = maxWorker
    [SerializeField] EducationLevel minWorkerEducationLevel = EducationLevel.secondery;
    uint wages = 0; //in units of money per day.
    [SerializeField] float wagesSpread = 0.2f; //the max percentage +or- 1.0x baseWagesXXXX at min/max building budget, in units of money. Should not be >= baseWagesXXXX.
    [SerializeField] [Range(20, 50)] uint minWorkplaceQuality = 35;
    [SerializeField] [Range(70, 100)] uint maxWorkplaceQuality = 85;
    [SerializeField] protected List<Citizen> employees = new List<Citizen>(); //TODO remove serialization when testing is done.
    uint workplaceQuality;

//getters
    public uint AvailableWorkerSlots()
    {
        return (uint)((long)maxManpower  - employees.Count);
    }

    public EducationLevel WorkerEducationLevel()
    {
        return minWorkerEducationLevel;
    }
    
    public uint MinManpower()
    {
        return minManpower;
    }

    public uint MaxManpower()
    {
        return maxManpower;
    }

    public uint CurrentManpower()
    {
        return (uint)employees.Count;
    }

    public List<Citizen> Employees()
    {
        return employees;
    }

    public uint Wages()
    {
        return wages;
    }

    public uint WorkplaceQuality()
    {
        //get budget from building, compute the percentage relative to min and max.
        Building building = this.gameObject.GetComponent<Building>();
        float budgetPercent = (building.Budget() - building.GetStats().minBudget) / (building.GetStats().maxBudget - building.GetStats().minBudget);
        
        //Get resource allocation sufficiecy
        float resourcePercent = building.AllocatedResources().CompareToBaseResource(building.GetStats().requiredResources);
        //Modify resourcePercent to go from 50% to 100%
        resourcePercent = 0.5f + resourcePercent * 0.5f; //For buildings without resource requirements, this will always result in 0.5f. See CompareToBaseResource() implementation.

        //compute quality based on budget and resource sufficiency.
        uint quality = (uint)Mathf.RoundToInt(budgetPercent * resourcePercent * (maxWorkplaceQuality - minWorkplaceQuality) + minWorkplaceQuality);
        
        return quality;
    }

//setters
    public void AssignEmployee(Citizen citizen) //this method DOES NOT check that the assignment is valid (there are empty slots, citizen satisfies education requirement)
    {
        employees.Add(citizen);
    }

    public bool RemoveEmployee(Citizen citizen)
    {
        for (int i = employees.Count - 1; i >= 0; i--)
        {
            if (employees[i] == citizen)
            {
                employees.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public void UpdateWages(uint budget, uint minBudget, uint maxBudget)
    {
        uint baseWage = 0;
        
        switch (minWorkerEducationLevel)
        {
            case EducationLevel.illiterate:
                baseWage = baseWagesIlliterate;
                break;
            case EducationLevel.primary:
                baseWage = baseWagesPrimary;
                break;
            case EducationLevel.secondery:
                baseWage = baseWagesSecondary;
                break;
            case EducationLevel.tertiary:
                baseWage = baseWagesTertiary;
                break;
            default:
                break;
        }
        uint minWage = (uint)Mathf.Max(Mathf.RoundToInt((1.0f - wagesSpread) * (float)baseWage), 0);
        uint maxWage =  (uint)Mathf.RoundToInt((1.0f + wagesSpread) * (float)baseWage);

        float budgetEffect = Mathf.Clamp((float)((int)budget - (int)minBudget) / (float)((int)maxBudget - (int)minBudget), 0.0f, 1.0f);
        wages = (uint)Mathf.RoundToInt(budgetEffect * (float)(maxWage - minWage)) + minWage;
    }
}
