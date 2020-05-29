using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public class Grid : MonoBehaviour
{
    static public Grid grid = null;

    Vector2Int boundary;
    public Vector2Int noOfCells;
    const int cellSize = 1;
    BoxCollider gridCollider;
    float colliderPadding = 10.0f;

    void Awake()
    {
        if (grid == null)
        {
            grid = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        gridCollider = this.gameObject.GetComponent<BoxCollider>();
        UpdateGridBoundary();
    }

    void UpdateGridBoundary()
    {
        boundary.x = noOfCells.x * cellSize;
        boundary.y = noOfCells.y * cellSize;
        gridCollider.center = this.transform.position;
        gridCollider.size = new Vector3(boundary.x + colliderPadding, 1.0f, boundary.y + colliderPadding);
    }

    public Cell SampleForCell(Vector3 _position)
    {
        Cell cell = new Cell();
        
        Vector3 position = _position;
        Vector3 offset = position - this.transform.position;

        if (Mathf.Abs(offset.x) > (float)(noOfCells.x * cellSize) / 2.0f || Mathf.Abs(offset.z) > (float)(noOfCells.y * cellSize) / 2.0f)
        {
            print ("Sampled outside boundary");//test
            return null;
        }

        Vector2Int distInCells = new Vector2Int(Mathf.FloorToInt(offset.x / (float)cellSize - (noOfCells.x%2 * (float)cellSize / 2.0f)), Mathf.FloorToInt(offset.z / (float)cellSize - (noOfCells.y%2 * (float)cellSize / 2.0f)));
        
        Vector3 cellCentre = this.transform.position; //to save copying the y value manually.

        cellCentre.x = (float)(distInCells.x * cellSize) + (float)cellSize / 2.0f;
        cellCentre.x += noOfCells.x%2 * (float)cellSize / 2.0f;

        cellCentre.z = (float)(distInCells.y * cellSize) + (float)cellSize / 2.0f;
        cellCentre.z += noOfCells.y%2 * (float)cellSize / 2.0f;

        lastCellCentre = cellCentre; //test
        return cell;
    }


    Vector3 lastCellCentre = new Vector3(0.0f, -10.0f, 0.0f); //test
    void OnDrawGizmos() //unefficient, but not meant for production anyway...
    {
        //UpdateGridBoundary();
        boundary.x = noOfCells.x * cellSize;
        boundary.y = noOfCells.y * cellSize;

        Gizmos.color = Color.blue;
        Vector3 _pos = this.transform.position;

        Vector3 cornerSW = new Vector3(_pos.x - boundary.x / 2.0f, _pos.y, _pos.z - boundary.y / 2.0f);
        Vector3 cornerNW = new Vector3(_pos.x - boundary.x / 2.0f, _pos.y, _pos.z + boundary.y / 2.0f);
        Vector3 cornerNE = new Vector3(_pos.x + boundary.x / 2.0f, _pos.y, _pos.z + boundary.y / 2.0f);
        Vector3 cornerSE = new Vector3(_pos.x + boundary.x / 2.0f, _pos.y, _pos.z - boundary.y / 2.0f);

        Gizmos.DrawLine(cornerSW, cornerNW);
        Gizmos.DrawLine(cornerNW, cornerNE);
        Gizmos.DrawLine(cornerNE, cornerSE);
        Gizmos.DrawLine(cornerSE, cornerSW);

        Gizmos.color = Color.red;
        for (int i = 1; i < noOfCells.x; i++)
        {
            Vector3 _shift = new Vector3(i * cellSize ,0.0f, 0.0f);
            Gizmos.DrawLine(cornerSW + _shift, cornerNW + _shift);
        }

         for (int i = 1; i < noOfCells.y; i++)
        {
            Vector3 _shift = new Vector3(0.0f ,0.0f, i * cellSize);
            Gizmos.DrawLine(cornerSW + _shift, cornerSE + _shift);
        }

        Gizmos.DrawCube(lastCellCentre, new Vector3(0.5f, 0.5f, 0.5f));

    }
}

public class Cell
{
    Vector3 cellCentre;
    bool isOccupied = false;
}