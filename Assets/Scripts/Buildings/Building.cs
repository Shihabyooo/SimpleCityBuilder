using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO consider moving emission from derived classes to this class, since all buildings have some form of emission, and have a virtual ComputeEmission method that each
//derived class overrides however it suits (without invoking the base)
//This includes moving the contents of AddPollution (which are currently similar between PowerPlant_1 and IndustrialBuilding_1) to this class.

public enum BuildingType //TODO add remaining types.
{
    residential, commercial, industrial, infrastructure, undefined //undefined is a logic error guard set as default value for buildings. If the default was another value and
}                                                                   //designer forgot to change it to correct value, it could cause issues elsewhere.

[RequireComponent(typeof(BoxCollider))]
public class Building : MonoBehaviour
{
    public GameObject model {get; private set;}
    [SerializeField] protected BuildingStats stats; //can't use {get; private set;} and have it serialized in the editor at the same time, so leaving at as private and making a getter method.
    public bool isUnderConstruction {get; private set;}
    public uint[] occupiedCell = new uint[2]; //the cell this building is constructed on, set by BuildingsManager (via ProposedBuilding).
    [SerializeField] protected uint budget; //IMPORTANT! budget must always be from stats.minBudget to stats.maxBudget
    [SerializeField] protected BasicResources allocatedResources = new BasicResources(); //These resources will be allocated by the simulation based on availability and priority.
    protected System.Guid uniqueID {get; private set;}
    protected System.DateTime constructionDate;

    BoxCollider buildingCollider;
    GameObject waterSign, powerSign;

    virtual protected void Awake()
    {
        budget = (uint)Mathf.FloorToInt((float)(stats.minBudget + stats.maxBudget) / 2.0f);
        buildingCollider = this.gameObject.GetComponent<BoxCollider>();
    }

    public virtual void ShowDetailsOnViewer()
    {
        BuildingDataViewer.viewerHandler.Show(this, BuildingViewerTemplate.generic);
    }

    public BuildingStats GetStats()
    {
        return stats;
    }

    public uint Budget()
    {
        return budget;
    }

    public void SetBudget(uint newbudget)
    {
        budget = newbudget;
    }

    public BasicResources AllocatedResources()
    {
        return allocatedResources;
    }

    public void AllocateResources(BasicResources resources)
    {
        allocatedResources = resources;
    }

    public System.DateTime ConstructionDate()
    {
        return constructionDate;
    }

    // bool CheckResourcesSufficiency()
    // {
    //     if (allocatedResources.power < stats.requiredResources.power)
    //         return false;
    //     else if (allocatedResources.water < stats.requiredResources.water)
    //         return false;

    //     return true;
    // }

    public virtual bool CheckConstructionResourceRequirements(Cell cell) //this does NOT include checking whether cell is occupied or not, which is handled in BuildingsManager.
    {   
        if (stats.requireResourcesToConstruct)
        {
            //There are two checks here: That the cell is covered by required services (data contained in the cell),
            //and that the available resources for the service are enought (from ResourcesManager)
            if (!cell.isPowered && stats.requiredResources.power > GameManager.resourceMan.AvailablePower()) 
                return false;

            if (!cell.isWatered && stats.requiredResources.water > GameManager.resourceMan.AvailableWater())
                return false;
        }
        
        return true;
    }

    public void BeginConstruction(Cell cell)
    {
        isUnderConstruction = true;
        occupiedCell = new uint[2]{cell.cellID[0], cell.cellID[1]};
        this.transform.localScale = Vector3.zero;
        //StartCoroutine(Construction());
        SimulationManager.onTimeUpdate += ProgressConstruction;
    }

    int constructionTimeElapsed = 0;
    void ProgressConstruction(int hours)
    {
        if (!isUnderConstruction)
        {
            SimulationManager.onTimeUpdate -= ProgressConstruction;
            return;
        }

        constructionTimeElapsed += hours;
        float ratio = Mathf.Min((float)constructionTimeElapsed / (float)stats.constructionTime , 1.0f);
        this.transform.localScale = new Vector3(ratio, ratio, ratio);

        if (ratio >= 0.999f)
            OnConstructionComplete();
    }

    protected virtual void OnConstructionComplete()
    {
        isUnderConstruction = false;
        uniqueID = GameManager.buildingsMan.GetNewGUID();
        GameManager.buildingsMan.AddConstructedBuilding(this);
        SimulationManager.onTimeUpdate -= ProgressConstruction;

        //Instantiate and disable the resources signs object
        float bufferOverColliderHeight = 0.75f;
        powerSign = GameObject.Instantiate(GameManager.gameMan.powerSign,
                                            this.transform.position + new Vector3((float)Grid.cellSize / 4.0f, buildingCollider.size.y + bufferOverColliderHeight, 0.0f),
                                            this.transform.rotation,
                                            this.transform
                                            );
        waterSign = GameObject.Instantiate(GameManager.gameMan.waterSign,
                                            this.transform.position + new Vector3(-1.0f * (float)Grid.cellSize / 4.0f, buildingCollider.size.y + bufferOverColliderHeight, 0.0f),
                                            this.transform.rotation,
                                            this.transform
                                            );

        powerSign.SetActive(false);
        waterSign.SetActive(false);

        constructionDate = GameManager.simMan.date;
    }

    public virtual void UpdateEffectOnNature(int timeWindow)
    {
        
    }

    public void CheckAndShowResourceShortages()
    {
        if (stats.requiredResources.power - allocatedResources.power > 0.01f)
        {
            powerSign.SetActive(true);
            powerSign.transform.Find("Avatar").gameObject.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, 100.0f * (1.0f - allocatedResources.power/stats.requiredResources.power));
        }
        else
        {
            powerSign.SetActive(false);
        }

        if (stats.requiredResources.water - allocatedResources.water > 0.01f)
        {
            waterSign.SetActive(true);
            waterSign.transform.Find("Avatar").gameObject.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(0, 100.0f * (1.0f - allocatedResources.water/stats.requiredResources.water));
        }
        else
        {
            waterSign.SetActive(false);
        }
    }

}

[System.Serializable]
public class BuildingStats
{
    public int id;
    public uint cost = 0;
    public BuildingType type = BuildingType.undefined;
    public float constructionTime = 0.0f; //In hours

    //requirements for operation 
    public bool requireResourcesToConstruct = false; //if set to true, building won't be constructed unless resources are enough, else building will be constructed, but won't operate fully (or at all)
    public BasicResources requiredResources = new BasicResources();

    //IMPORATANT! maxBudget must always be greater than minBudget
    public uint minBudget = 1; //unit of money per unit time
    public uint maxBudget = 2; //unit of money per unit time
    //TODO add remaining requirements.

    //TODO add remaining -universal- parameters here.
    public BuildingStats()
    {

    }

    public BuildingStats(BuildingStats source) //Deep copy
    {
        id = source.id;
        cost = source.cost;
        type = source.type;
        constructionTime = source.constructionTime;
        requireResourcesToConstruct = source.requireResourcesToConstruct;
        minBudget = source.minBudget;
        maxBudget = source.maxBudget;
    }
}

[System.Serializable]
public class BasicResources
{
    public float power = 0.0f; //in unit power per unit time.
    public float water = 0.0f; //in unit volume per unit time.

    public BasicResources()
    {

    }

    public BasicResources(BasicResources source) //deep copy
    {
        power = source.power;
        water = source.water;
    }

    public float CompareToBaseResource(BasicResources baseResources) //used mostly in calculating efficiency, called when this object is the ALLOCATED resources. Returns average of
    {                                                                 //percentages of satisfaction for each resource.
        float percentage = 0.0f;
        int counter = 0;

        //The if statements are to make this method universal (useable in cases even when only part of the basic resources are set)
        if (baseResources.power > 0.001f) //not zero
        {
            percentage += power / baseResources.power;
            counter++;
        }

        if (baseResources.water > 0.001f)
        {
            percentage += water / baseResources.water;
            counter++;
        }

        if (counter > 0)
            percentage = percentage / (float)counter;
        else
            percentage = 1.0f;

        return percentage;
    }
}