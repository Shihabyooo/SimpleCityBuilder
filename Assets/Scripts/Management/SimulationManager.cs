using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    
    //TODO divide the simulation to several runs. Example:
        //Resident Happiness run.
        //Infrastructure Use run: Loop over residents/buildings, update load on infrastructure building accordingly.
        //Natural resources run: Loop over natural resource consuming building, based on their consumption, update natural resources.
        //Budget run: Loop over everything relevant, compute delta on player budget.

    public float timeBetweenUpdates = 0.5f; //the time between each update in sim in seconds.
    [SerializeField] bool isRunning = false;

    Coroutine infrastructureSim = null, happinessSim = null, natResourceSim = null, budgetSim = null;
    
    public void StartSimulation()
    {
        isRunning = true;
        infrastructureSim = StartCoroutine(InfrastructureSimRun());
        happinessSim = StartCoroutine(HappinessSimRun());
        natResourceSim = StartCoroutine(NaturalResourcesSimRun());
        budgetSim = StartCoroutine(BudgetSimRun());
    }

    public void PauseSimulation()
    {
        isRunning = false;
    }

    public void ResumeSimulation()
    {
        isRunning = true;
    }

    IEnumerator InfrastructureSimRun()
    {
        while (true)
        {
            if (isRunning)
            {
                float totalWaterProduction = 0.0f;
                float totalPowerProduction = 0.0f;

            //Compute production for all infra buildings
                foreach (InfrastructureBuilding building in GameManager.buildingsMan.powerProductionBuildings)
                    totalPowerProduction += building.ComputeProduction();
                foreach (InfrastructureBuilding building in GameManager.buildingsMan.waterProductionBuildings)
                    totalWaterProduction += building.ComputeProduction();
    
            //Update ResourcesManager
                GameManager.resourceMan.UpdateWaterSupply(totalWaterProduction);
                GameManager.resourceMan.UpdateAvailablePower(totalPowerProduction);

                yield return new WaitForSeconds(timeBetweenUpdates);
            }
            else
                yield return new WaitForFixedUpdate();
        }
    }

    //To be implemented
    IEnumerator HappinessSimRun()
    {
        while (true)
        {
            if (isRunning)
            {

                yield return new WaitForSeconds(timeBetweenUpdates);
            }
            else
                yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator NaturalResourcesSimRun()
    {
        while (true)
        {
            if (isRunning)
            {
                
                yield return new WaitForSeconds(timeBetweenUpdates);
            }
            else
                yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator BudgetSimRun()
    {
        while (true)
        {
            if (isRunning)
            {
                
                yield return new WaitForSeconds(timeBetweenUpdates);
            }
            else
                yield return new WaitForFixedUpdate();
        }
    }

}