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
    [SerializeField] protected List<Citizen> employees = new List<Citizen>(); //TODO remove serialization when testing is done.

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
