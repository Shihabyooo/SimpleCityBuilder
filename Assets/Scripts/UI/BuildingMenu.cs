using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingMenu : MonoBehaviour
{
    [SerializeField] Vector3 hiddenPosition, shownPosition;
    [SerializeField] bool isShown = false;
    [SerializeField] bool isSwitchingState = false;
    [SerializeField] float switchingTime = 1.5f;
    [SerializeField] int referenceUIResolutionWidth = 1920;

    public void SwitchMenuState()
    {
        if (isSwitchingState)
            return;

        isSwitchingState = true;
        if (isShown)
            HideMenu();
        else
            ShowMenu();
    }

    void ShowMenu()
    {
        isShown = true;
        Vector2 targetPosition = this.gameObject.GetComponent<RectTransform>().rect.position;
        targetPosition.x = (-1 * referenceUIResolutionWidth / 2) + (this.GetComponent<RectTransform>().rect.width / 2);
        StartCoroutine(ChangePosition(false));
    }

    void HideMenu()
    {
        isShown = false;
        StartCoroutine(ChangePosition(true));
    }

    IEnumerator ChangePosition(bool hide)
    {
        Vector2 position;// = this.gameObject.GetComponent<RectTransform>().anchoredPosition;
        float helperTimer = 0.0f;
        float xDifference = hiddenPosition.x - shownPosition.x;
        float baseX;

        if (hide)
            baseX = shownPosition.x;   
        else
        {
            baseX = hiddenPosition.x;
            xDifference = xDifference * -1.0f;
        }
                
        while (helperTimer < switchingTime)
        {
            helperTimer += Time.deltaTime;
            position = this.gameObject.GetComponent<RectTransform>().anchoredPosition;
            position.x = baseX + Mathf.Min(helperTimer / switchingTime, 1.0f) * xDifference;

            this.gameObject.GetComponent<RectTransform>().anchoredPosition = position;
            yield return new WaitForEndOfFrame();
        }

        yield return isSwitchingState = false;
    }

   
   public void SelectBuilding(int id)
   {
       if (isSwitchingState)
            return;

       //the code to translate the ID into a building goes here. For now, we hardcode block spawning just to test it out.
       GameManager.gameMan.SwitchToBuildingPlacement(id);
   }
}
