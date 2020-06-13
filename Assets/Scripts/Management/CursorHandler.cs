using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO rename this file/script to something more general (e.g. PlayerControls)
public enum ControlMode
{
    freeMode, buildingDrag, menu, cellInspection
}

public class CursorHandler : MonoBehaviour
{
    [SerializeField] ControlMode currentCursorMode; //TODO remove serialization after testing is done.
    public LayerMask gridLayer;
    public LayerMask ground;
    public LayerMask freeModeSelectables;
    const float maxRayCastDistance = 1000.0f;
    BuildingsManager.BuildingProposal buildingToPlace = null;
    Vector3 outOfViewPosition = new Vector3(0.0f, -10.0f, 0.0f); //Planned constructions will be moved to this location when the cursor is pointed outside allowable construction zone (outside grid boundaries)

    CameraControl cameraControl;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        //currentCursorMode = ControlMode.freeMode; 
        cameraControl = Camera.main.gameObject.GetComponent<CameraControl>();
    }

    void Update()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        switch(currentCursorMode)
        {
            case ControlMode.freeMode:
                FreeModeControl();
                break;
            case ControlMode.buildingDrag:
                BuildingDragControl();
                break;
            case ControlMode.menu:
                MenuControl();
                break;
            case ControlMode.cellInspection:
                CellInspectionControl();
                break;
            default:
                break;
        }
    }


//Controls
    //float middleButtonHoldTime = 0.0f;
    //[SerializeField] float middleMoustHoldToPanTime = 0.5f;
    Vector3 lastMiddleClickLocation;



    void FreeModeControl()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            //Cast a ray and check if it hits something we handle.
            RaycastHit hit = CastRay(freeModeSelectables);
            if (hit.collider != null)
                print ("hit: " + hit.collider.gameObject.name);
        }
        else if (Input.GetMouseButtonDown(2))
        {
            lastMiddleClickLocation = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            CameraRotate();
        }
    }

    void BuildingDragControl()
    {
        RaycastHit hit = CastRay(gridLayer);
        Cell cell = Grid.grid.SampleForCell(hit.point);
        
        if (cell != null)
        {
            buildingToPlace.MovePlan(cell.cellCentre);

            if (Input.GetMouseButtonDown(0))
            {
                if (buildingToPlace.Construct(cell))
                {
                    buildingToPlace = null;
                    SwitchToFreeMode();
                }
            }
        }
        else //hide building..
            buildingToPlace.MovePlan(outOfViewPosition);

        if (Input.GetMouseButtonDown(1))
        {
            buildingToPlace.Cancel();
            buildingToPlace = null;
            SwitchToFreeMode();
        }
    }

    void MenuControl()
    {

    }

    void CellInspectionControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = CastRay(gridLayer);
            inspectedCell = Grid.grid.SampleForCell(hit.point);
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            showCellValue = false;
            inspectedCell = null;
            SwitchToFreeMode();
        }
    }


//Camera controls

    void zoom()
    {

    }

    void CameraDrag()
    {

    }

    void CameraRotate()
    {

        //first compute rotation origin
        
        Vector3 rotationOrigin;
        RaycastHit hit = CastRay(gridLayer | ground);
        //if (hit.collider != null)
            //rotationOrigin = hit.point;
        //else
        //{
            //print("Rotating about Grid centre"); //test
            rotationOrigin = Grid.grid.gameObject.transform.position;
        //}

        //then compute rotation angle
        Vector3 mouseDir = Input.mousePosition - lastMiddleClickLocation;
        mouseDir.Normalize();

        cameraControl.RotateView(mouseDir, rotationOrigin);
        lastMiddleClickLocation = Input.mousePosition;
    }

//Control Utilities
    Vector3 direction; //test. For vizualization in editor.
    RaycastHit CastRay(LayerMask mask)
    {
        RaycastHit hit;
        Vector3 mousePosition = Input.mousePosition;       
        mousePosition.z = 1.0f;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Ray ray = new Ray (Camera.main.transform.position, (mousePosition - Camera.main.transform.position));
        Physics.Raycast(ray, out hit, maxRayCastDistance, mask);
        
        direction = mousePosition - Camera.main.transform.position; //test. For vizualization in editor.
        lastHitPosition = hit.point; //test. For vizualization in editor.
        return hit;
    }



//Other Utilities
    public void SwitchToBuildingPlacement(BuildingsManager.BuildingProposal building)
    {
        if (currentCursorMode != ControlMode.freeMode) //can only switch to building placement from freemode.
            return;

        currentCursorMode = ControlMode.buildingDrag;
        buildingToPlace = building;
    }

    public void SwitchToFreeMode()
    {
        currentCursorMode = ControlMode.freeMode;
    }

    public void SwitchToCellInspection()
    {
        showCellValue = true;
        currentCursorMode = ControlMode.cellInspection;
    }

    public ControlMode CurrentCursorMode()
    {
        return currentCursorMode;
    }
//Other testing methods

    Vector3 lastHitPosition = new Vector3(10.0f, 2.0f, 5.0f);
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(lastHitPosition, 0.3f);
        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(mousePosition, 0.15f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(Camera.main.transform.position, direction * maxRayCastDistance);
    }


    bool showCellValue = false;
    Cell inspectedCell;
    void OnGUI()
    {
        
        GUI.Label(new Rect(10, 20, 100, 20), ("current control mode: " + currentCursorMode.ToString()));
        
        if (UnityEditor.EditorApplication.isPlaying && showCellValue)
        {
            int lineHeight = 20;
            Rect rect = new Rect(250, lineHeight, 500, lineHeight);
            if (inspectedCell != null)
            {
                Rect nextLine = rect;
                
                string text = "Infrastructure state: ";
                if (inspectedCell.isPowered)
                    text += "Powered, ";
                if (inspectedCell.isWatered)
                    text += "Watered, ";
                if (inspectedCell.isEducated)
                    text += "Educated, ";
                if (inspectedCell.isHealthed)
                    text += "Healthed, ";
                if (inspectedCell.isOccupied)
                    text += "Occupied, ";

                GUI.Label(nextLine, text);
                
                nextLine.y += lineHeight + 5;
                text = "GW Capacity: " + inspectedCell.groundwaterCapacity.ToString();
                GUI.Label(nextLine, text);

                nextLine.y += lineHeight + 5;
                text = "GW Volume: " + inspectedCell.groundwaterVolume.ToString();
                GUI.Label(nextLine, text);

                nextLine.y += lineHeight + 5;
                text = "GW Recharge: " + inspectedCell.groundwaterRecharge.ToString();
                GUI.Label(nextLine, text);

                nextLine.y += lineHeight + 5;
                text = "Wind Direction: " + inspectedCell.windDirection.ToString();
                GUI.Label(nextLine, text);

                nextLine.y += lineHeight + 5;
                text = "Wind Speed: " + inspectedCell.windSpeed.ToString();
                GUI.Label(nextLine, text);

            }
            else
                GUI.Label(rect, "NULL");
        }
    }

}
