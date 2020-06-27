using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Building))]
public class WorkPlace : MonoBehaviour
{
    [SerializeField] uint minManpower = 0; //minimum amount of workers required for this building to operate.
    [SerializeField] uint maxManpower = 0; //maximum amount of workers this building can handle. Production is max when currentWorkersCount = maxWorker
    [SerializeField] EducationLevel minWorkerEducationLevel = EducationLevel.secondery;
    [SerializeField] uint wages = 0; //in units of money per day.
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

        //compute quality based on budget
        uint quality = (uint)Mathf.RoundToInt(budgetPercent * (maxWorkplaceQuality - minWorkplaceQuality) + minWorkplaceQuality);
        
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

}
