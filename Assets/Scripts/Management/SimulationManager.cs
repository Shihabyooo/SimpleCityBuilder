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
    System.DateTime date; //Known issue, this struct supports as far as 31-12-9999.
    [SerializeField] [Range(1,24)]  int dateUpdateRateHours = 0; //the hours increment should be something we can divide 24 with (to make days increment after same number
                                                                            //update ticks for all days), the days increment should be 1. Either days or hours should be set, not both.
                                                                            //System should work for any value though.
                                                                            
    Coroutine buildingsSim = null, happinessSim = null, natResourceSim = null, budgetSim = null, timeKeeper = null;
    
    public void Awake()
    {
        date = new System.DateTime(2020, 8, 1, 0, 0, 0);
    }

    public void StartSimulation()
    {
        isRunning = true;
        buildingsSim = StartCoroutine(BuildingsSimRun());
        happinessSim = StartCoroutine(HappinessSimRun());
        natResourceSim = StartCoroutine(NaturalResourcesSimRun());
        budgetSim = StartCoroutine(BudgetSimRun());
        timeKeeper = StartCoroutine(TimeKeeperRun());
    }

    public void PauseSimulation()
    {
        isRunning = false;
    }

    public void ResumeSimulation()
    {
        isRunning = true;
    }

    void NewDay()
    {
        GameManager.climateMan.UpdateClimate(date);
    }

    IEnumerator BuildingsSimRun() //the current logic assigns available resources prioritizing older buildings (those coming first in buildingsMan.constructedBuildings list)
    {                           
        //Current issue with this logic: It's expensive! Most buildings won't have their operational status and parameters changed each sim update cycle, yet we would still compute
        //productions and consumptions for all buildings at every sim update. A better approach would be to let each building process changes on its own by updating global values
        //or registering itself for processing by a central manager. Downside of this approach would be checking against race conditions, and a more complicated code.

        while (true)
        {
            if (isRunning)
            {

            //General Buildings
                float totalWaterDemand = 0.0f;
                float totalPowerDemand = 0.0f;

                foreach (Building building in GameManager.buildingsMan.constructedBuildings)
                {
                    totalWaterDemand += building.GetStats().requiredResources.water;
                    totalPowerDemand += building.GetStats().requiredResources.power;
                }

                GameManager.resourceMan.UpdateWaterDemand(totalWaterDemand);
                GameManager.resourceMan.UpdatePowerDemand(totalPowerDemand);

      
            //Computing production of infraStructure buildings
                float totalWaterProduction = 0.0f;
                float totalPowerProduction = 0.0f;

            //Compute production for all infra buildings
                foreach (InfrastructureBuilding building in GameManager.buildingsMan.powerProductionBuildings)
                    totalPowerProduction += building.ComputeProduction();
                foreach (InfrastructureBuilding building in GameManager.buildingsMan.waterProductionBuildings)
                    totalWaterProduction += building.ComputeProduction();
    
            //Update ResourcesManager
                GameManager.resourceMan.UpdateAvailableWater(totalWaterProduction);
                GameManager.resourceMan.UpdateAvailablePower(totalPowerProduction);

            //Allocate resources to buildings based on production and priority
                float totalWaterConsumption = 0.0f;
                float totalPowerConsumption = 0.0f;

            foreach (Building building in GameManager.buildingsMan.constructedBuildings)
            {
                BasicResources resources = new BasicResources(); 
                
                resources.power = Mathf.Clamp(building.GetStats().requiredResources.power, 0.0f, totalPowerProduction - totalPowerConsumption);
                resources.water = Mathf.Clamp(building.GetStats().requiredResources.water, 0.0f, totalWaterProduction - totalWaterConsumption);

                totalPowerConsumption += resources.power;
                totalWaterConsumption += resources.water;
                building.AllocateResources(resources);
            }
            
            //update ResourceManager
            GameManager.resourceMan.UpdateWaterConsumption(totalWaterConsumption);
            GameManager.resourceMan.UpdatePowerConsumption(totalPowerConsumption);

                yield return new WaitForSeconds(timeBetweenUpdates);
            }
            else
                yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator TimeKeeperRun()
    {
        while (true)
        {
            int currentDay = date.Day;
            if (isRunning)
            {
                date += new System.TimeSpan(0, dateUpdateRateHours, 0, 0);
                if (currentDay != date.Day)
                {
                    NewDay();
                    currentDay = date.Day;
                }

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


    //testing visualization
    void OnGUI()
    {
        int lineHeight = 20;
        int padding = 7;
        Rect rect = new Rect(150, Screen.height - lineHeight - 100, 200, lineHeight);
        GUIStyle style = new GUIStyle();
        style.fontSize = 25;
        
        
        string message = "Date: " + date.Day.ToString() + "-" + date.Month.ToString() + "-" + date.Year.ToString() + " - " + date.Hour.ToString();
        GUI.Label(rect, message, style);

        rect.y += lineHeight + padding;
        message = "Rainfall: " + GameManager.climateMan.currentRainfall.ToString();
        GUI.Label(rect, message, style);
    }
}