using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Grid : MonoBehaviour
{
    static public Grid grid = null;

    Vector2Int boundary; //TODO consider removing this. So far, this is only usefull in editor visualization. 
    public Vector2Int noOfCells;
    const int cellSize = 1;
    BoxCollider gridCollider;
    float colliderPadding = 10.0f;
    int[,] gridStatus; //0 = empty, 1 = occupied by building TODO add more status codes.

    //[SerializeField] GridLayerBase[] extraGridLayers;


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

        //initialize gridStatus;
        gridStatus = new int[noOfCells.x, noOfCells.y];//[noOfCells.y];
        for (int i = 0; i < noOfCells.x; i++)
            for (int j = 0; j < noOfCells.y; j++)
                gridStatus[i, j] = 0;
    }

    void UpdateGridBoundary()
    {
        //Update numerical boundary itself
        boundary.x = noOfCells.x * cellSize;
        boundary.y = noOfCells.y * cellSize;

        //update collider boundary
        Vector3 _centre = this.transform.position;
        _centre.y = -0.5f; //half the thickness, so the collider is spawned with its surface (which the raycast will hit matches the sufrace of the grid).
        gridCollider.center = _centre;
        gridCollider.size = new Vector3(boundary.x + colliderPadding, 1.0f, boundary.y + colliderPadding);
    }

    public Cell SampleForCell(Vector3 position)
    {
        Cell cell = new Cell();
        
        Vector3 offset = position - this.transform.position;

        if (Mathf.Abs(offset.x) > (float)(noOfCells.x * cellSize) / 2.0f || Mathf.Abs(offset.z) > (float)(noOfCells.y * cellSize) / 2.0f)
        {
            //print ("Sampled outside boundary");//test
            return null;
        }

        float[] rawDistInCells = {  ((offset.x + (Mathf.Sign(offset.x) * noOfCells.x%2 * (float)cellSize / 2.0f)) / (float)cellSize),
                                    ((offset.z + (Mathf.Sign(offset.z) * noOfCells.y%2 * (float)cellSize / 2.0f)) / (float)cellSize)};

        int[] rawDistSigns = {(int)Mathf.Sign(rawDistInCells[0]), (int)Mathf.Sign(rawDistInCells[1])};

        Vector2Int distInCells = new Vector2Int(Mathf.FloorToInt(Mathf.Abs(rawDistInCells[0])) * rawDistSigns[0],
                                                Mathf.FloorToInt(Mathf.Abs(rawDistInCells[1])) * rawDistSigns[1]);
        
        //Compute position of cell's centre.
        //TODO simplify the eqns bellow.
        cell.cellCentre.x = (float)(distInCells.x * cellSize);
        cell.cellCentre.x += (1 - noOfCells.x%2) * (float)cellSize / 2.0f * rawDistSigns[0]; //special consideration for even numbered cell x count.

        cell.cellCentre.z = (float)(distInCells.y * cellSize);
        cell.cellCentre.z += (1 - noOfCells.y%2) * (float)cellSize / 2.0f * rawDistSigns[1]; //special consideration for even numbered cell y count.

        cell.cellCentre.y = this.transform.position.y;

        //Compute cellID
        cell.cellID[0] = Mathf.FloorToInt((float)noOfCells.x / 2.0f) + distInCells.x;
        cell.cellID[0] += (1 - noOfCells.x%2) * (((1 + rawDistSigns[0]) / 2) - 1); //special consideration for even numbered cell x count.

        cell.cellID[1] = Mathf.FloorToInt((float)noOfCells.y / 2.0f) + distInCells.y;
        cell.cellID[1] += (1 - noOfCells.y%2) * (((1 + rawDistSigns[1]) / 2) - 1); //special consideration for even numbered cell y count.

        GetCellState(ref cell);

        //lastCellCentre = cell.cellCentre; //test
        //print ("dist in cells: " + distInCells + ", or: " + (rawDistInCells[0] * rawDistSigns[0]) + ", " + (rawDistInCells[1] * rawDistSigns[1]) ); //test
        //print ("cellID: " + cell.cellID[0] + ", " +cell.cellID[1]); //test
        return cell;
    }

    void GetCellState(ref Cell cell)
    {   
        switch(gridStatus[cell.cellID[0], cell.cellID[1]])
        {
            case (0):
            cell.isOccupied = false;
                break;
            case (1):
            cell.isOccupied = true;
                break;
            default:
                break;
        }
    }

    public void SetCellOccupiedState (Cell cell, bool isOccupied)
    {
        SetCellState((uint)cell.cellID[0], (uint)cell.cellID[1], isOccupied? 1 : 0);
    }

    public void SetCellOccupiedState (uint cellID_x, uint cellID_y, bool isOccupied)
    {
        SetCellState(cellID_x, cellID_y, isOccupied? 1 : 0);
    }

    void SetCellState(uint cellID_x, uint cellID_y, int state)
    {
        gridStatus[cellID_x, cellID_y] = state;
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
    public Vector3 cellCentre;
    public bool isOccupied = false;
    public int[] cellID = new int[2];   //from 0, 0 to noOfCells.x-1, noOfCells.y-1
                                        //This is dangeours, cellIDs are otherwise treated as uint. I'm gambling here that the calculation of cellID (in SampleForCell())
                                        //will always compute a positive integer.
}

//TODO revisit these classes
[System.Serializable]
public class GridLayerBase
{
    public string name;
}

[System.Serializable]
public class GridLayer<T> : GridLayerBase
{
    public T value;
    public T[,] gridStatus;

    public T GetCellStatus(uint cellID_x, uint cellID_y) //Returns default value of assigned type if index outside array range
    {
        if (cellID_x >= gridStatus.GetLength(0) || cellID_y >= gridStatus.GetLength(1))
            return default(T);
        
        return gridStatus[cellID_x, cellID_y];
    }
}