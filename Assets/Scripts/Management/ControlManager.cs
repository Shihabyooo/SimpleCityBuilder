using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlMode
{
    freeMode, buildingPlacement, menu, cellInspection
}

public class ControlManager : MonoBehaviour
{
    [SerializeField] ControlMode currentCursorMode; //TODO remove serialization after testing is done.
    [SerializeField] LayerMask gridLayer;
    [SerializeField] LayerMask ground;
    [SerializeField] LayerMask freeModeSelectables;
    const float maxRayCastDistance = 1000.0f;
    BuildingsManager.BuildingProposal buildingToPlace = null;
    Vector3 outOfViewPosition = new Vector3(0.0f, -10.0f, 0.0f); //Planned constructions will be moved to this location when the cursor is pointed outside allowable construction zone (outside grid boundaries)

    CameraControl cameraControl;
    [SerializeField] float mousePanDeadZone = 0.01f; //dead zone distance (in percentage of screen pixels of each access) inside which no mouse movement is considered for panning camera
    [SerializeField] float mousePanMaxRange = 0.1f; //in percentage of screen resolution, the distance from centre of right click where no more speed is gained for panning.


    void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
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
            case ControlMode.buildingPlacement:
                BuildingPlacementControl();
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
    public float rightClickHoldTime = 0.0f;
    [SerializeField] float rightClickHoldToPanTime = 0.5f;
    Vector3 lastMiddleClickLocation;
    Vector3 lastRightClickLocation;
    Vector3 rotationOrigin = new Vector3();

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
            //save the last mouse position (which will be used to calculate rotation angles)
            lastMiddleClickLocation = Input.mousePosition;
            
            //compute rotation origin
            RaycastHit hit = cameraControl.CastRay(gridLayer | ground, maxRayCastDistance);
            if (hit.collider != null)
                rotationOrigin = hit.point;
            else
            {
                print("Rotating about Grid centre"); //test
                rotationOrigin = Grid.grid.gameObject.transform.position;
            }
        }
        else if (Input.GetMouseButton(2))
        {
            CameraRotate(rotationOrigin);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            rightClickHoldTime = 0.0f;
            lastRightClickLocation = Input.mousePosition;
        }
        else if (Input.GetMouseButton(1))
        {
            rightClickHoldTime += Time.deltaTime;
            if (rightClickHoldTime >= rightClickHoldToPanTime)
            {
                CameraDrag();
            }
        }        
        else
        {
            //here we can safely allow zooming or move camera by moving mouse to screen edge.
            CameraScrollZoom();
            CameraEdgeMove();
        }
    }
    
    [SerializeField] float timeBetweenScrollRotation = 0.05f;
    float scrollHelperTimer = 1.0f;
    void BuildingPlacementControl()
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
                    return;
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
        else if (Input.mouseScrollDelta.y > 0.1f || Input.mouseScrollDelta.y < -0.1f)
        {
            if (scrollHelperTimer >= timeBetweenScrollRotation)
            {
                scrollHelperTimer = 0.0f;
                buildingToPlace.RotatePlan(Input.mouseScrollDelta.y);
            }
        }

        scrollHelperTimer += Time.deltaTime;
    }

    void MenuControl()
    {

    }

    void CellInspectionControl()
    {
        //TODO switch this class to store the cell ID, which is then used to regularily sample the grid for the cell's stats (to get the newest value in grid).

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
    void CameraScrollZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta > 0.1f || scrollDelta < -0.1f)
        {
            scrollDelta = scrollDelta / Mathf.Abs(scrollDelta); //normalize the value
            cameraControl.Zoom(scrollDelta);
        }
    }

    void CameraEdgeMove()
    {
        //TODO implement this
    }

    void CameraDrag()
    {
        Vector3 difference = Input.mousePosition - lastRightClickLocation;
        
        if (Mathf.Abs(difference.x) < mousePanDeadZone * Screen.width && Mathf.Abs(difference.y) < mousePanDeadZone * Screen.height)
            return;

        Vector2 velocity = new Vector2();
        velocity.x = Mathf.Sign(difference.x) * Mathf.Min(Mathf.Abs(difference.x), mousePanMaxRange * Screen.width) /  (mousePanMaxRange * Screen.width);
        velocity.y = Mathf.Sign(difference.y) * Mathf.Min(Mathf.Abs(difference.y), mousePanMaxRange * Screen.height) / (mousePanMaxRange * Screen.height);

        cameraControl.Pan(velocity);
    }

    void CameraRotate(Vector3 rotationOrigin)
    {
        //then compute rotation angle
        Vector3 mouseDir = Input.mousePosition - lastMiddleClickLocation;
        mouseDir.Normalize();

        cameraControl.RotateView(mouseDir, rotationOrigin);
        lastMiddleClickLocation = Input.mousePosition;
    }

//Control Utilities
    //Vector3 direction; //test. For vizualization in editor.
    RaycastHit CastRay(LayerMask mask)
    {
        RaycastHit hit;
        Vector3 mousePosition = Input.mousePosition;       
        mousePosition.z = 1.0f;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Ray ray = new Ray (Camera.main.transform.position, (mousePosition - Camera.main.transform.position));
        Physics.Raycast(ray, out hit, maxRayCastDistance, mask);
        
        return hit;
    }

//Other Utilities
    public void SwitchToBuildingPlacement(BuildingsManager.BuildingProposal building)
    {
        if (currentCursorMode != ControlMode.freeMode) //can only switch to building placement from freemode.
            return;

        currentCursorMode = ControlMode.buildingPlacement;
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
    bool showCellValue = false;
    Cell inspectedCell;
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;

        GUI.Label(new Rect(10, 20, 100, 20), ("current control mode: " + ((int)currentCursorMode).ToString()), style);
        
        if (UnityEditor.EditorApplication.isPlaying && showCellValue)
        {
            int lineHeight = 22;
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

                GUI.Label(nextLine, text, style);
                
                nextLine.y += lineHeight + 5;
                text = "GW Capacity: " + inspectedCell.groundwaterCapacity.ToString();
                GUI.Label(nextLine, text, style);

                nextLine.y += lineHeight + 5;
                text = "GW Volume: " + inspectedCell.groundwaterVolume.ToString();
                GUI.Label(nextLine, text, style);

                nextLine.y += lineHeight + 5;
                text = "GW Recharge: " + inspectedCell.groundwaterRecharge.ToString();
                GUI.Label(nextLine, text, style);

                nextLine.y += lineHeight + 5;
                text = "Wind Direction: " + inspectedCell.windDirection.ToString();
                GUI.Label(nextLine, text, style);

                nextLine.y += lineHeight + 5;
                text = "Wind Speed: " + inspectedCell.windSpeed.ToString();
                GUI.Label(nextLine, text, style);

                nextLine.y += lineHeight + 5;
                text = "Pollution: " + inspectedCell.pollution.ToString();
                GUI.Label(nextLine, text, style);

                nextLine.y += lineHeight + 5;
                text = "Rainfall: " + inspectedCell.rainfall.ToString();
                GUI.Label(nextLine, text, style);


            }
            else
                GUI.Label(rect, "NULL", style);
        }
    }

}
