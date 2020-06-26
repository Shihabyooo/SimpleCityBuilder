using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseDataViewer : MonoBehaviour
{

    Transform baseContent;
    Building viewedBuilding;

    Text buildingName;
    Slider budgetSlider;
    Text budgetCurrent, budgetMin, budgetMax;
    Text powerReq, waterReq;
    Text powerAlloc, waterAlloc;
    Text constructionDate, buildingType;

    Transform activeExtension = null;
    List<Transform> extensionsTransforms = new List<Transform>();

    void Awake()
    {
        
    }

    public void UpdateViewer()
    {
        if (viewedBuilding == null)
            return;

        ResetData();

        if (activeExtension != null)
            activeExtension.GetComponent<ExtendedDataViewer>().SetExtendedData(viewedBuilding);
    }

    public void Initialize()
    {
        Transform extensions = this.transform.Find("Viewport").Find("Content").Find("Extensions");
        foreach (Transform transform in extensions)
        {
            extensionsTransforms.Add(transform);
            transform.gameObject.SetActive(false);
        }

        //build refs to base data fields
        baseContent = this.transform.Find("Viewport").Find("Content").Find("Base");

        buildingName = baseContent.Find("Name").GetComponent<Text>();
        budgetSlider = baseContent.Find("BudgetSlider").GetComponent<Slider>();
        budgetCurrent = baseContent.Find("BudgetCurrent").GetComponent<Text>();
        budgetMin = baseContent.Find("BudgetMin").GetComponent<Text>();
        budgetMax = baseContent.Find("BudgetMax").GetComponent<Text>();
        powerReq = baseContent.Find("PowerReq").GetComponent<Text>();
        waterReq = baseContent.Find("WaterReq").GetComponent<Text>();
        powerAlloc = baseContent.Find("PowerAlloc").GetComponent<Text>();
        waterAlloc = baseContent.Find("WaterAlloc").GetComponent<Text>();
        constructionDate = baseContent.Find("ConstructionDate").GetComponent<Text>();
        buildingType = baseContent.Find("BuildingType").GetComponent<Text>();
    }

    public void SetData(Building building)
    {
        CloseViewer();
        viewedBuilding = building;
        ResetData();
    }

    void ResetData()
    {
        buildingName.text = viewedBuilding.gameObject.name;
        budgetCurrent.text = viewedBuilding.Budget().ToString();
        budgetMin.text = viewedBuilding.GetStats().minBudget.ToString();
        budgetMax.text = viewedBuilding.GetStats().maxBudget.ToString();
        powerReq.text = viewedBuilding.GetStats().requiredResources.power.ToString();
        waterReq.text = viewedBuilding.GetStats().requiredResources.water.ToString();
        powerAlloc.text = viewedBuilding.AllocatedResources().power.ToString();
        waterAlloc.text = viewedBuilding.AllocatedResources().water.ToString();
        constructionDate.text = GetDateString(viewedBuilding);
        buildingType.text = GetBuildingType(viewedBuilding);

        budgetSlider.value = ((float)viewedBuilding.Budget() - (float)viewedBuilding.GetStats().minBudget) / ((float)viewedBuilding.GetStats().maxBudget - (float)viewedBuilding.GetStats().minBudget);
    }

    protected string GetBuildingType(Building building)
    {
        switch (building.GetStats().type)
        {
            case BuildingType.commercial:
                return "Commercial";
            case BuildingType.industrial:
                return "Industrial";
            case BuildingType.infrastructure:
                return "Infrasctructure";
            case BuildingType.residential:
                return "Residential";
            default:
                return "N/A";
        }
    }

    protected string GetDateString(Building building)
    {
        System.DateTime date = building.ConstructionDate();
        return (date.Day.ToString() + "-" + GameManager.uiMan.GetMonthAbbr(date.Month) + "-" + date.Year);
    }

    public void UpdateBudget()
    {
        float budgetPercent = budgetSlider.value;
        uint newBudget = (uint)Mathf.RoundToInt(budgetPercent * (viewedBuilding.GetStats().maxBudget - viewedBuilding.GetStats().minBudget) + viewedBuilding.GetStats().minBudget);
        viewedBuilding.SetBudget(newBudget);
        budgetCurrent.text = newBudget.ToString();
    }

    public void SetExtendedData(string extensionName)
    {
        foreach (Transform transform in extensionsTransforms)
        {
            if (transform.gameObject.name == extensionName)
            {
                activeExtension = transform;
                activeExtension.gameObject.SetActive(true);
                activeExtension.GetComponent<ExtendedDataViewer>().SetExtendedData(viewedBuilding);
                return;
            }
        }
        print ("At SetExtensionData, could not find extension with provided extensionName: " + extensionName);//test
    }

    public void CloseViewer()
    {
        viewedBuilding = null;
        if(activeExtension != null)
            activeExtension.gameObject.SetActive(false);
        activeExtension = null;
    }
}
