using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO decouple extra grid layer from base grid (i.e. other layers can have coarser grids, to save memory/performance.)

public enum InfrastructureService
{
    none = 0, water = 1, power = 2, gas = 4, education = 8, health = 16, 
}

[RequireComponent(typeof(BoxCollider))]
public class Grid : MonoBehaviour
{
    static public Grid grid = null;

    Vector2Int boundary; //TODO consider removing this. So far, this is only usefull in editor visualization. 
    public Vector2Int noOfCells; //TODO switch this to uint[2];
    public const int cellSize = 1;
    BoxCollider gridCollider;
    float colliderPadding = 10.0f;
    int[,] cellsStatus; //Main grid layer, basically tells whether the cell is empty or not. 0 = empty, 1 = occupied by building. TODO add more status codes.

    //Extra grid layers, each layer targets specific element (e.g. water supply, pollution, health coverage, education coverage, etc)
    //Services and infrastructure
    public GridLayer<InfrastructureService> infrastructureLayer {get; private set;} //Uses bitmask to mark the reach of an infrastructure service, check InfrastructureService enum.
    //natural resources
    public GridLayer<float> groundWaterCapacityLayer {get; private set;} //max capacity, unit volume
    public GridLayer<float> groundWaterVolumeLayer {get; private set;} //current quantity, unit volume
    public GridLayer<float> groundWaterRechargeLayer {get; private set;} //unit volume per unit time.
    public GridLayer<float> windSpeedLayer {get; private set;} //cell size per unit time.
    public GridLayer<uint> windDirectionLayer {get;private set;} //stored as a degree angle starting from the north (positive y) and going clockwise.
    //Others layer
    public GridLayer<float> pollutionLayer {get; private set;}


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

        InitializeGridLayers();
        
    }

    void InitializeGridLayers()
    {
        cellsStatus = new int[noOfCells.x, noOfCells.y];

        infrastructureLayer = new GridLayer<InfrastructureService>((uint)noOfCells.x, (uint)noOfCells.y); // 
        InitializeNaturalResourcesLayers();
        InitializeOthersLayers();
    }

    void InitializeNaturalResourcesLayers()
    {
        //TODO implement this after figuring out how maps are going to be designed/saved.public GridLayer<float> groundWaterCapacityLayer {get; private set;} //max capacity, unit volume
        groundWaterCapacityLayer = new GridLayer<float>((uint)noOfCells.x, (uint)noOfCells.y);
        groundWaterVolumeLayer = new GridLayer<float>((uint)noOfCells.x, (uint)noOfCells.y);
        groundWaterRechargeLayer = new GridLayer<float>((uint)noOfCells.x, (uint)noOfCells.y);
        windSpeedLayer  = new GridLayer<float>((uint)noOfCells.x, (uint)noOfCells.y);
        windDirectionLayer  = new GridLayer<uint>((uint)noOfCells.x, (uint)noOfCells.y);

    }

    void InitializeOthersLayers()
    {
        pollutionLayer = new GridLayer<float>((uint)noOfCells.x, (uint)noOfCells.y);
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
        //IMPORTANT! You're gambling that the calculations above WILL NEVER produce negative numbers. They should, but you need to double check that...
        cell.cellID[0] = (uint)(Mathf.FloorToInt((float)noOfCells.x / 2.0f) + distInCells.x);
        cell.cellID[0] += (uint)((1 - noOfCells.x%2) * (((1 + rawDistSigns[0]) / 2) - 1)); //special consideration for even numbered cell x count.

        cell.cellID[1] = (uint)(Mathf.FloorToInt((float)noOfCells.y / 2.0f) + distInCells.y);
        cell.cellID[1] += (uint)((1 - noOfCells.y%2) * (((1 + rawDistSigns[1]) / 2) - 1)); //special consideration for even numbered cell y count.

        GetAllCellStates(ref cell);

        //lastCellCentre = cell.cellCentre; //test
        //print ("dist in cells: " + distInCells + ", or: " + (rawDistInCells[0] * rawDistSigns[0]) + ", " + (rawDistInCells[1] * rawDistSigns[1]) ); //test
        //print ("cellID: " + cell.cellID[0] + ", " +cell.cellID[1]); //test
        return cell;
    }

    void GetAllCellStates(ref Cell cell)
    {
        GetCellOccupationState(ref cell);
        GetCellInfrastructureStates(ref cell);
        GetCellNaturalResourcesStates(ref cell);
        GetOtherCellStates(ref cell);
    }

    void GetCellOccupationState(ref Cell cell)
    {   
        switch(cellsStatus[cell.cellID[0], cell.cellID[1]])
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

    void GetCellInfrastructureStates(ref Cell cell)
    {
        InfrastructureService cellServices = infrastructureLayer.GetCellValue(cell.cellID[0], cell.cellID[1]);
        cell.isEducated = IsInfrastructureSet(InfrastructureService.education, cellServices);
        cell.isHealthed = IsInfrastructureSet(InfrastructureService.health, cellServices);
        cell.isPowered = IsInfrastructureSet(InfrastructureService.power, cellServices);
        cell.isWatered = IsInfrastructureSet(InfrastructureService.water, cellServices);    
    }
    void GetCellNaturalResourcesStates(ref Cell cell)
    {
        cell.groundwaterCapacity = groundWaterCapacityLayer.GetCellValue(cell.cellID[0], cell.cellID[1]);
        cell.groundwaterRecharge = groundWaterRechargeLayer.GetCellValue(cell.cellID[0], cell.cellID[1]);
        cell.groundwaterVolume = groundWaterVolumeLayer.GetCellValue(cell.cellID[0], cell.cellID[1]);
        cell.windDirection = windDirectionLayer.GetCellValue(cell.cellID[0], cell.cellID[1]);
        cell.windSpeed = windSpeedLayer.GetCellValue(cell.cellID[0], cell.cellID[1]);
    }

    void GetOtherCellStates(ref Cell cell)
    {
        cell.pollution = pollutionLayer.GetCellValue(cell.cellID[0], cell.cellID[1]);
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
        cellsStatus[cellID_x, cellID_y] = state;
    }

//helper methods:
    bool IsInfrastructureSet(InfrastructureService service, InfrastructureService cellServices)
    {
        if ((cellServices & service) == service) //TODO check this.
            return true;
        
        return false;
    }

    //TODO fix the function bellow. While for most of the radius it works correctly, it fails at the poles (@ymin and ymax), only painting a single cell each.
    public void SetInfrastructureState(InfrastructureService service, uint cellID_x, uint cellID_y, uint radius) 
    {
        //Equation of circle: (x-a)^2 + (y-b)^2 = r^2
        //a = cellID_x, b = cellID_y, r = radius
        
        uint minY = (uint)Mathf.RoundToInt(Mathf.Clamp((long)cellID_y - (long)radius, 0, noOfCells.y - 1));
        uint maxY = (uint)Mathf.RoundToInt(Mathf.Clamp((long)cellID_y + (long)radius, 0, noOfCells.y - 1));
        
        for (uint i = minY; i <= maxY; i++)
        {
            long sqrtVal = Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow((long)i - (long)cellID_y, 2))); //cache this calculation, since its result will be used twice.

            uint minX = (uint)Mathf.FloorToInt(Mathf.Clamp( (long)cellID_x - sqrtVal, 0, noOfCells.x - 1));
            uint maxX = (uint)Mathf.CeilToInt(Mathf.Clamp( (long)cellID_x + sqrtVal, 0, noOfCells.x - 1));

            for (uint j = minX; j <= maxX; j++)
            {
                infrastructureLayer.GetCellRef(j, i) = infrastructureLayer.GetCellValue(j, i) | service;
            }
        }

    }
    

//testing visualization code
    Vector3 lastCellCentre = new Vector3(0.0f, -10.0f, 0.0f); //test
    public bool visualizeWaterInfra = false;
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

        if (visualizeWaterInfra)
        {
            Gizmos.color = Color.cyan;
            for (uint i = 0; i < noOfCells.x; i++)
            {
                for (uint j = 0; j < noOfCells.y; j++)
                {
                    if (IsInfrastructureSet(InfrastructureService.water, infrastructureLayer.GetCellValue(i, j)))
                    {

                        cornerSW = new Vector3(_pos.x - boundary.x / 2.0f + i * cellSize, _pos.y, _pos.z - boundary.y / 2.0f + j * cellSize);
                        cornerNW = new Vector3(_pos.x - boundary.x / 2.0f + i * cellSize, _pos.y, _pos.z - boundary.y / 2.0f + j * cellSize + cellSize);
                        cornerNE = new Vector3(_pos.x - boundary.x / 2.0f + i * cellSize + cellSize, _pos.y, _pos.z - boundary.y / 2.0f + j * cellSize + cellSize);
                        cornerSE = new Vector3(_pos.x - boundary.x / 2.0f + i * cellSize + cellSize, _pos.y, _pos.z - boundary.y / 2.0f + j * cellSize);

                        Gizmos.DrawLine(cornerSW, cornerNW);
                        Gizmos.DrawLine(cornerNW, cornerNE);
                        Gizmos.DrawLine(cornerNE, cornerSE);
                        Gizmos.DrawLine(cornerSE, cornerSW);
                        //Vector3 _shift = new Vector3( i *cellSize, 0.0f, j * cellSize);
                        //Gizmos.DrawRay(cornerSW + _shift, cornerSW + _shift + new Vector3(cellSize, 0.0f, 0.0f));
                        //Gizmos.DrawLine(cornerSW + _shift, cornerSE + _shift + new Vector3(0.0f, 0.0f, cellSize));
                    }
                }
            }
        }
    }
}

public class Cell
{
    public uint[] cellID = new uint[2];   //from 0, 0 to noOfCells.x-1, noOfCells.y-1
    public Vector3 cellCentre;
    public bool isOccupied = false;
    //Infrastructure services reach
    public bool isPowered = false;
    public bool isWatered = false;
    public bool isEducated = false;
    public bool isHealthed = false;
    //natural resources
    public float groundwaterVolume = 0.0f;
    public float groundwaterCapacity = 0.0f;
    public float groundwaterRecharge = 0.0f;
    public float windSpeed = 0.0f;
    public uint windDirection = 0;
    //others
    public float pollution = 0.0f;


    
}

//TODO revisit these classes

[System.Serializable]
public class GridLayer<T>
{
    public T[,] gridStatus;

    public GridLayer(uint width, uint height)
    {
        gridStatus = new T[width, height];
    }

    virtual public T GetCellValue(uint cellID_x, uint cellID_y) //Returns default value of assigned type if index outside array range
    {
        if (cellID_x >= gridStatus.GetLength(0) || cellID_y >= gridStatus.GetLength(1))
            return default(T);
        
        return gridStatus[cellID_x, cellID_y];
    }

    virtual public ref T GetCellRef(uint cellID_x, uint cellID_y) //used primarily for the infrastructureLayer in Grid class, since these will be processed with bitwise ops.
    {
        return ref gridStatus[cellID_x, cellID_y];
    }

    virtual public void SetCellValue(uint cellID_x, uint cellID_y, T value)
    {
        if (cellID_x >= gridStatus.GetLength(0) || cellID_y >= gridStatus.GetLength(1))
            return;
        gridStatus[cellID_x, cellID_y] = value;
    }
    public void SetMultipleCellsValueConst(uint centralCellID_x, uint centralCellID_y, uint radius, T value)
    {
        //TODO implement this
    }
    public void SetMultipleCellsValueLinearGradient(uint centralCellID_x, uint centralCellID_y, uint radius, T value) //central cell assigns full value, cells at radius + 1 has zero, interpolate in between linearaly.
    {
        //TODO implement this
    }

    public void SetMultipleCellsValueExponentialGradient() 
    {
        //TODO add arguments and implement this.
    }
}
