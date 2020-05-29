using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorHandler : MonoBehaviour
{
    
    public LayerMask gridLayer;
    const float maxRayCastDistance = 1000.0f;
    
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    void FixedUpdate()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        SampleCurrenMousePosition();
    }

    Vector3 direction; //test. For vizualization in editor.
    void SampleCurrenMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;       
        mousePosition.z = 1.0f;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Ray ray = new Ray (Camera.main.transform.position, (mousePosition - Camera.main.transform.position));
        RaycastHit hit;
        Physics.Raycast(ray, out hit, maxRayCastDistance, gridLayer);
        
        direction = mousePosition - Camera.main.transform.position; //test. For vizualization in editor.
        lastHitPosition = hit.point; //test. For vizualization in editor.

        Grid.grid.SampleForCell(hit.point);
    }

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
