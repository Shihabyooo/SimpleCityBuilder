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
    public System.DateTime date {get; private set;} //Known issue, this struct supports as far as 31-12-9999.
    [SerializeField] [Range(1990, 2030)] int startYear = 2000;
    [SerializeField] [Range(1, 12)] int startMonth = 1;
    [SerializeField] [Range(1,24)]  int dateUpdateRateHours = 0; //the hours increment should be something we can divide 24 with (to make days increment after same number
                                                                            //update ticks for all days), the days increment should be 1. Either days or hours should be set, not both.
                                                                            //System should work for any value though.
                                                                            
    Coroutine buildingsSim = null, natResourceSim = null, budgetSim = null, timeKeeper = null;
    
    public void Awake()
    {
        date = new System.DateTime(startYear, startMonth, 1, 0, 0, 0);
    }

    public void StartSimulation()
    {
        isRunning = true;
        buildingsSim = StartCoroutine(BuildingsSimRun());
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
        GameManager.populationMan.UpdatePopulation();
    }

    //Simulation runs coroutines.
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
                HousingSlots totalHousingSlots = new HousingSlots();
                HousingSlots occuppiedHousingSlots = new HousingSlots();

                //To avoid having multiple loops, the loop bellow will cover several, building-specific calls depending on building types. Some exceptions will have their
                //own loops bellow, but in theory could also be merged with this loop.
                //TODO consider mergin into a single loop with if-statements.
                foreach (Building building in GameManager.buildingsMan.constructedBuildings)
                {
                    //Fill requirement resources 
                    //TODO consider moving this to a function that only updates when its state changes (e.g. new building constructed, stats changed, etc). Would complicate 
                    //the code elsewhere though.
                    totalWaterDemand += building.GetStats().requiredResources.water;
                    totalPowerDemand += building.GetStats().requiredResources.power;

                    //update building effect on natural resources and environment
                    building.UpdateEffectOnNature(dateUpdateRateHours);

                    //for residential buildings,
                    if (building.GetStats().type == BuildingType.residential)
                    {
                        ResidentialBuilding residentialBuilding = building.gameObject.GetComponent<ResidentialBuilding>();
                        //update housing slots
                        //totalHousingSlots += residentialBuilding.HousingCapacity();
                        //occuppiedHousingSlots += residentialBuilding.CountResidents();
                        totalHousingSlots.IncrementSlotValue(residentialBuilding.HousingCapacity(), residentialBuilding.ResidentClass());
                        occuppiedHousingSlots.IncrementSlotValue(residentialBuilding.ResidentsCount(), residentialBuilding.ResidentClass());
                        
                        residentialBuilding.UpdateHousingQuality();
                    }

                }

                //Update ResourceManager
                GameManager.resourceMan.UpdateWaterDemand(totalWaterDemand);
                GameManager.resourceMan.UpdatePowerDemand(totalPowerDemand);
                GameManager.resourceMan.UpdateTotalHousingSlots(totalHousingSlots);
                GameManager.resourceMan.UpdateOccupiedHousingSlots(occuppiedHousingSlots);


      
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

                //Divide load on infrastructure buildings and assign it
                float powerDemandToMaxProductionRatio = Mathf.Min(totalPowerDemand / totalPowerProduction, 1.0f );
                float waterDemandToMaxProductionRatio = Mathf.Min(totalWaterDemand / totalWaterProduction, 1.0f );

                foreach (InfrastructureBuilding building in GameManager.buildingsMan.powerProductionBuildings)
                    building.SetLoad(powerDemandToMaxProductionRatio);
                foreach (InfrastructureBuilding building in GameManager.buildingsMan.waterProductionBuildings)
                    building.SetLoad(waterDemandToMaxProductionRatio);


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

    IEnumerator NaturalResourcesSimRun()
    {
        while (true)
        {
            if (isRunning)
            {
                if (GameManager.climateMan.isRaining)
                    RechargeGroundWaterByRainfall();

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

    //natural resources updating
    void RechargeGroundWaterByRainfall()
    {
        //Loop over all cells.
        //First, each GW layer will be recharged using 

        //float adjaceCellsRechargePercentage = 0.5f;     //The percentage of excess rainfall volume (after substracting cell's recharge) that will recharge adjacent cells.
                                                        //TODO make this as a global, game parameters.

        GridLayer<float> potentialRecharge = new GridLayer<float>((uint)Grid.grid.noOfCells.x, (uint)Grid.grid.noOfCells.y);
        //TODO modify the algorithm bellow to update potentialRecharge (without clampting to cell's recharge values), afterwords, append potentialRecharge cell values to 
        //GW volume layer (while clamping for its recharge rate).
        
        for (uint i = 0; i < Grid.grid.noOfCells.x; i++)
        {
            for (uint j = 0; j < Grid.grid.noOfCells.y; j++)
            {
                float cellRainfall = Grid.grid.rainFallLayer.GetCellValue(i, j);
                if (cellRainfall > 0.001f) //no need to do calculations for cells without rainfall..
                {
                    float capacity = Grid.grid.groundWaterCapacityLayer.GetCellValue(i, j);
                    float recharge = Grid.grid.groundWaterRechargeLayer.GetCellValue(i, j);
                    
                    //Assume that, to convert rainfall depth to a volume, we multiply by 1000.0f (including unit adjustements).
                    float rainfallVolume = 1000.0f * cellRainfall / (float)(24 / dateUpdateRateHours);

                    float maxRechargeVolume =  Mathf.Clamp(rainfallVolume / dateUpdateRateHours, 0.0f, recharge) * dateUpdateRateHours; 
                    float usedRechargeVolume = Mathf.Clamp(capacity - Grid.grid.groundWaterVolumeLayer.GetCellValue(i, j), 0.0f, maxRechargeVolume);
                    
                    Grid.grid.groundWaterVolumeLayer.GetCellRef(i, j) += usedRechargeVolume;
                    
                    // //(percentage of) excess rainfall volume will recharge adjacent cells
                    // float excessRainfallVolume = adjaceCellsRechargePercentage * (rainfallVolume - usedRechargeVolume) / 4.0f; //divide by number of adjacent cells.

                    // uint lowerX = (uint)Mathf.Max((int)i - 1, 0);
                    // uint lowerY = (uint)Mathf.Max((int)j - 1, 0);
                    // uint upperX = (uint)Mathf.Min(i + 1, Grid.grid.noOfCells.x - 1);
                    // uint upperY = (uint)Mathf.Min(j + 1, Grid.grid.noOfCells.y - 1);
                    // for (uint k = lowerX; k <= upperX; k++)
                    // {
                    //     for (uint l = lowerY; l <= upperY; l++)
                    //     {
                    //         capacity = Grid.grid.groundWaterCapacityLayer.GetCellValue(k, l);
                    //         recharge = Grid.grid.groundWaterRechargeLayer.GetCellValue(k, l);
                    //         maxRechargeVolume = Mathf.Clamp(excessRainfallVolume / dateUpdateRateHours, 0.0f, recharge) * dateUpdateRateHours; 
                    //         usedRechargeVolume = Mathf.Clamp(capacity - Grid.grid.groundWaterVolumeLayer.GetCellValue(k, l), 0.0f, maxRechargeVolume);
                    //         Grid.grid.groundWaterVolumeLayer.GetCellRef(k, l) += usedRechargeVolume;
                    //     }
                    // }
                }
            }
        }
    }

    //testing visualization
    void OnGUI()
    {
        int lineHeight = 20;
        //int padding = 7;
        Rect rect = new Rect(150, Screen.height - lineHeight - 100, 200, lineHeight);
        GUIStyle style = new GUIStyle();
        style.fontSize = 25;
        
        
        string message = "Date: " + date.Day.ToString() + "-" + date.Month.ToString() + "-" + date.Year.ToString() + " - " + date.Hour.ToString();
        GUI.Label(rect, message, style);

        // rect.y += lineHeight + padding;
        // message = "Rainfall: " + GameManager.climateMan.currentRainfall.ToString();
        // GUI.Label(rect, message, style);
    }
}
