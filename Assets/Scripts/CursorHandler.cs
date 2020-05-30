using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ControlMode
{
    freeMode, buildingDrag, menu
}

public class CursorHandler : MonoBehaviour
{
    [SerializeField] ControlMode currentCursorMode; //TODO remove serialization after testing is done.
    public LayerMask gridLayer;
    public LayerMask freeModeSelectables;
    const float maxRayCastDistance = 1000.0f;
    BuildingsManager.BuildingProposal buildingToPlace = null;


    void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        //currentCursorMode = ControlMode.freeMode; 
    }

    void FixedUpdate() //Should move to Update?
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
            default:
                break;
        }
    }


//Controls
    void FreeModeControl()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            //Cast a ray and check if it hits something we handle.
            RaycastHit hit = CastRay(freeModeSelectables);
            if (hit.collider != null)
                print ("hit: " + hit.collider.gameObject.name);

        }
    }

    void BuildingDragControl()
    {
        RaycastHit hit = CastRay(gridLayer);
        Cell cell = Grid.grid.SampleForCell(hit.point);
        
        buildingToPlace.MovePlan(cell.cellCentre);

        if (Input.GetMouseButtonDown(0))
        {
            if (buildingToPlace.Construct(cell))
            {
                buildingToPlace = null;
                currentCursorMode = ControlMode.freeMode;
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            buildingToPlace.Cancel();
            buildingToPlace = null;
        }

    }

    void MenuControl()
    {

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
        currentCursorMode = ControlMode.buildingDrag;
        buildingToPlace = building;
    }


//Other
    Vector3 lastHitPosition = new Vector3(10.0f, 2.0f, 5.0f);
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(lastHitPosition, 0.3f);
        //Gizmos.color = Color.green;
        //Gizmos.DrawSphere(mousePosition, 0.15f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(Camera.main.transform.position, direction * maxRayCastDistance);
    }
}
