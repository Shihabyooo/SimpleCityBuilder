using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericBuilding : BaseDataViewer
{
    Slider budgetSlider;
    Text budgetCurrent, budgetMin, budgetMax;
    Text powerReq, waterReq;
    Text powerAlloc, waterAlloc;
    Text constructionDate, buildingType;
    
    protected override void Awake()
    {
        base.Awake();
        budgetSlider = content.Find("BudgetSlider").GetComponent<Slider>();
        budgetCurrent = content.Find("BudgetCurrent").GetComponent<Text>();
        budgetMin = content.Find("BudgetMin").GetComponent<Text>();
        budgetMax = content.Find("BudgetMax").GetComponent<Text>();
        powerReq = content.Find("PowerReq").GetComponent<Text>();
        waterReq = content.Find("WaterReq").GetComponent<Text>();
        powerAlloc = content.Find("PowerAlloc").GetComponent<Text>();
        waterAlloc = content.Find("WaterAlloc").GetComponent<Text>();
        constructionDate = content.Find("ConstructionDate").GetComponent<Text>();
        buildingType = content.Find("BuildingType").GetComponent<Text>();
    }

    public override void SetData(Building building)
    {
        base.SetData(building);
        budgetCurrent.text = building.Budget().ToString();
        budgetMin.text = building.GetStats().minBudget.ToString();
        budgetMax.text = building.GetStats().maxBudget.ToString();
        powerReq.text = building.GetStats().requiredResources.power.ToString();
        waterReq.text = building.GetStats().requiredResources.water.ToString();
        powerAlloc.text = building.AllocatedResources().power.ToString();
        waterAlloc.text = building.AllocatedResources().water.ToString();
        constructionDate.text = GetDateString(building);
        buildingType.text = GetBuildingType(building);

        budgetSlider.value = ((float)building.Budget() - (float)building.GetStats().minBudget) / ((float)building.GetStats().maxBudget - (float)building.GetStats().minBudget);
    }

    public override void UpdateBudget()
    {
        float budgetPercent = budgetSlider.value;
        uint newBudget = (uint)Mathf.RoundToInt(budgetPercent * (viewedBuilding.GetStats().maxBudget - viewedBuilding.GetStats().minBudget) + viewedBuilding.GetStats().minBudget);
        viewedBuilding.SetBudget(newBudget);
        budgetCurrent.text = newBudget.ToString();
    }
}
