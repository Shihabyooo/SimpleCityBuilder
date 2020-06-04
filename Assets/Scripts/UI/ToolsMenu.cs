using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsMenu : MonoBehaviour
{
    public void StartTool (int toolID)
    {
        switch(toolID)
        {
            case 1:
            GameManager.gameMan.SwitchToCellInspector();
                break;
            default:
            print ("!!!!!!!!!!!!!!!!!!!!!! Warning! Attempting to start an unknown tool");
                break;
        }
    }
}
