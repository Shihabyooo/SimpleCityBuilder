using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This graph generator is specificly written for Timeseries. i.e. the X-axis is time (in days).

//TODO modify the code to render to a texture, then set texture as an image in appropriate place inside the bigger graph window (itself a mix of images, text, sliders, buttons, etc)

public class GraphGenerator : MonoBehaviour
{
    [SerializeField] Material lineMaterial;
    [SerializeField] Material bgMaterial;
    [SerializeField] Material decorationMaterial;
    Vector2 graphSize;
    [SerializeField] Vector2 padding;

    [SerializeField] float graphLineThickness = 2.0f; //in pixels.
    [SerializeField] float axesLineThickness = 3.5f; //in pixels.
    [SerializeField] float gridLineThickness = 1.5f; //in pixels.
    [SerializeField] int maxGridCountX = 25;
    [SerializeField] int maxGridCountY = 10;

    Vector2 centre;
    GraphData activeGraphData = null;
    bool isShown = false;
    Transform content;
    RawImage viewport;
    Text title, xAxis, yAxis;
    ResourcesHistory.DataType currentGraphDataType = ResourcesHistory.DataType.undefined;
    GameObject labelTemplate;
    Transform labelsContainer;
    List<GameObject> labelsX, labelsY;

    void Awake()
    {
        //Build references
        content = this.transform.Find("Content");
        viewport = content.Find("GraphViewport").gameObject.GetComponent<RawImage>();
        title = content.Find("GraphTitle").gameObject.GetComponent<Text>();
        xAxis = content.Find("GraphXAxis").gameObject.GetComponent<Text>();
        yAxis = content.Find("GraphYAxis").gameObject.GetComponent<Text>();
        labelTemplate = content.Find("LabelTemplate").gameObject;
        labelsContainer = content.Find("Labels");

        //Set graphSize
        graphSize = new Vector2(viewport.gameObject.GetComponent<RectTransform>().rect.width,
                                viewport.gameObject.GetComponent<RectTransform>().rect.height);

        //Hide content
        content.gameObject.SetActive(false);
        

        //Spawn Labels and hide them
        labelsX = new List<GameObject>();
        labelsY = new List<GameObject>();
        labelTemplate.SetActive(false); //Would cause the labels to be spawned disabled.

        for (int i = 0; i < maxGridCountX; i++)
            labelsX.Add(GameObject.Instantiate(labelTemplate, this.transform.position, xAxis.transform.rotation, labelsContainer));
        
        
        for (int i = 0; i <= maxGridCountY; i++) //IMPORTANT! Note that in Y axis, the labels are maxGridCountY + 1, to give label to upper bound of graph
            labelsY.Add(GameObject.Instantiate(labelTemplate, this.transform.position, xAxis.transform.rotation, labelsContainer));
        

    }

    public bool ShowGraph (ResourcesHistory.DataType historyDataType)
    {
        TimeSeries<float> timeSeries = GameManager.resourceMan.GetTimeSeries(historyDataType);
        switch (historyDataType)
        {
            case ResourcesHistory.DataType.treasury:
                currentGraphDataType = ResourcesHistory.DataType.treasury;
                return ShowGraph(timeSeries, "Treasury", "Date", "Funds - $");
            //TODO Add (relevant) remaining datatypes.
            default:
                return false;
        }
    }

    bool ShowGraph(TimeSeries<float> data, string _title, string _xAxis, string _yAxis)
    {
        if (!PrepareGraphData(data))
            return false;

        content.gameObject.SetActive(true);

        SetGraphDetails(_title, _xAxis, _yAxis);
        DrawGraphWindow();
        isShown = true;
        
        SimulationManager.onNewDay += UpdateCurrentTimeseries;

        return true;
    }

    public void CloseGraph()
    {
        if (!isShown)
            return;

        isShown = false;
        content.gameObject.SetActive(false);
        SimulationManager.onNewDay -= UpdateCurrentTimeseries;
    }

    bool PrepareGraphData(TimeSeries<float> data)
    {
        GraphData graphData = new GraphData(data);
        
        if (graphData.data == null)
        {
            print ("Error, in creating GraphData");
            return false;   
        }
        
        activeGraphData = graphData;
        return true;
    }

    void SetGraphDetails(string _title, string _xAxis, string _yAxis)
    {
        title.text = _title;
        xAxis.text = _xAxis;
        yAxis.text = _yAxis;
    }

    void UpdateCurrentTimeseries(System.DateTime date) //to be added to simulation onNewDay
    {
        if (currentGraphDataType == ResourcesHistory.DataType.undefined)
        {
            print ("WARNING! Attempted to update a graph with data type of undefined. This shouldn't happen");
            return;
        }

        //Get Last added data of type and add them to graphData
        activeGraphData.AddToTimeSeries(date, GameManager.resourceMan.GetLastHistoryEntry(currentGraphDataType));
        
        //redraw graph
        DrawGraphWindow();
    }

    void DrawGraphWindow()
    {
        if(activeGraphData == null)
            return;

        viewport.texture = ReDrawGraph();

    }

    public Texture2D ReDrawGraph()
    {
        //Texture prep
        //Source for method used to rendering to texture: https://forum.unity.com/threads/rendering-gl-to-a-texture2d-immediately-in-unity4.158918/
        int texWidth = Mathf.CeilToInt(graphSize.x);
        int texHeight = Mathf.CeilToInt(graphSize.y);
        RenderTexture renderTexture = RenderTexture.GetTemporary(texWidth, texHeight);
        RenderTexture.active = renderTexture;
        GL.Clear(false, true, Color.magenta);

        //Other prep
        centre = graphSize / 2.0f;

        //GL prep and exec
        GL.PushMatrix();
        GL.LoadOrtho();

        DrawBackground();
        DrawContent(activeGraphData);

        GL.PopMatrix();

        //Finish up the render-to-texture stuff
        Texture2D outputTexture = new Texture2D(texWidth, texHeight);
        outputTexture.ReadPixels(new Rect(0, 0, texWidth, texHeight), 0, 0);
        outputTexture.Apply(false);
        outputTexture.Compress(true);
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);
        return outputTexture;
    }

    void DrawBackground()
    {
        bgMaterial.SetPass(0);
        GL.Begin(GL.QUADS);
        
        GL.TexCoord2(0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 0.0f);
        GL.TexCoord2(0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);
        GL.TexCoord2(1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 0.0f);
        GL.TexCoord2(1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 0.0f);

        GL.End();
    }

    void DrawContent(GraphData data)
    {
        Vector2 workingDimensions = graphSize - (padding * 2.0f);
        Vector2 origin = (centre - workingDimensions / 2.0f);
        
        DrawDecoration(data, workingDimensions, origin);
        DrawLine(data, workingDimensions, origin);
    }

    void DrawDecoration(GraphData data, Vector2 workingDimensions, Vector2 origin) //TODO refactor this
    {
        decorationMaterial.SetPass(0);
        
        //Cache some repeating calculations
        float relOriginX = origin.x / graphSize.x;
        float relOriginY = origin.y / graphSize.y;
        float relThicknessHalfX = axesLineThickness / (graphSize.x * 2.0f);
        float relThicknessHalfY = axesLineThickness / (graphSize.y * 2.0f);

        //Draw Axes
        GL.Begin(GL.QUADS);
        
        //Y axis always begins at left edge        
        GL.Vertex3(relOriginX + relThicknessHalfX,  relOriginY, 0.0f );
        GL.Vertex3(relOriginX - relThicknessHalfX,  relOriginY, 0.0f );
        GL.Vertex3(relOriginX - relThicknessHalfX,  (origin.y + workingDimensions.y) / graphSize.y, 0.0f );
        GL.Vertex3(relOriginX + relThicknessHalfX,  (origin.y + workingDimensions.y) / graphSize.y, 0.0f );

        //The X axis is clamped to the zero value (if the values oscilate between positive and negative), or is the min (for all positive) or max (for all negative) value.
        float yCoord;
        if (data.isOscillatingAroundYAxis) //compute the position of y=zero.
            yCoord = (origin.y + workingDimensions.y * (0.0f - data.minY) / data.rangeY) / graphSize.y;
        else //compute position depending on whether all positive or negative
            yCoord = (origin.y + Mathf.Min((Mathf.Sign(data.Value(0))) * workingDimensions.y , 0.0f)) / graphSize.y;
        
        GL.Vertex3(relOriginX, yCoord - relThicknessHalfY, 0.0f);
        GL.Vertex3(relOriginX, yCoord + relThicknessHalfY, 0.0f);
        GL.Vertex3(relOriginX + workingDimensions.x / graphSize.x, yCoord + relThicknessHalfY, 0.0f);
        GL.Vertex3(relOriginX + workingDimensions.x / graphSize.x, yCoord - relThicknessHalfY, 0.0f);

        //Draw Grid
        //modify cached reThicknessHalfX and Y to use gridLineThickness
        relThicknessHalfX = gridLineThickness / (graphSize.x * 2.0f);
        relThicknessHalfY = gridLineThickness / (graphSize.y * 2.0f);

        int xGridCount = Mathf.Min(data.Length(), maxGridCountX);

        //The code bellow allows for dynamic fitting of the grid, the end segment will be varrying in size, but the preceding ones will be fixed.
        int daysPerGridSpacing = Mathf.CeilToInt(data.Length() / (float)xGridCount);
        float xDistPerDay = (workingDimensions.x / (float)data.Length()) / graphSize.x;

        for (int i = daysPerGridSpacing; i < data.Length(); i += daysPerGridSpacing)
        {
            GL.Vertex3(relOriginX + (i * xDistPerDay) + relThicknessHalfX, relOriginY, 0.0f);
            GL.Vertex3(relOriginX + (i * xDistPerDay) - relThicknessHalfX, relOriginY, 0.0f);
            GL.Vertex3(relOriginX + (i * xDistPerDay) - relThicknessHalfX, (origin.y + workingDimensions.y) / graphSize.y, 0.0f);
            GL.Vertex3(relOriginX + (i * xDistPerDay) + relThicknessHalfX, (origin.y + workingDimensions.y) / graphSize.y, 0.0f);
        }

        float yGridSpacing = workingDimensions.y / (float)maxGridCountY / graphSize.y;
        for (int i = 0; i <= maxGridCountY; i++)
        {
            GL.Vertex3(relOriginX, relOriginY + (i * yGridSpacing) - relThicknessHalfY, 0.0f);
            GL.Vertex3(relOriginX, relOriginY + (i * yGridSpacing) + relThicknessHalfY, 0.0f);
            GL.Vertex3((origin.x + workingDimensions.x) / graphSize.x, relOriginY + (i * yGridSpacing) + relThicknessHalfY, 0.0f);
            GL.Vertex3((origin.x + workingDimensions.x) / graphSize.x, relOriginY + (i * yGridSpacing) - relThicknessHalfY, 0.0f);
        }

        GL.End();

        //Spawn labels as Gameobjects with Text components (make them children of this transform)
        for (int i = 0; i < xGridCount; i++)
        {
            //Set visibility on
            labelsX[i].SetActive(true);

            //Position at grid line
            labelsX[i].GetComponent<RectTransform>().anchoredPosition = this.gameObject.GetComponent<RectTransform>().anchoredPosition - workingDimensions / 2.0f
                                                                        + new Vector2(i * daysPerGridSpacing * workingDimensions.x / data.Length(), -1.0f * padding.y);
            
            //Set text value/date
            labelsX[i].GetComponent<Text>().text = UIManager.DateToString(activeGraphData.DateByOffset(i * daysPerGridSpacing), true);
        }

        for (int i = 0; i <= maxGridCountY; i++)
        {
            //set visibility on
            labelsY[i].SetActive(true);

            //position at grid line
            labelsY[i].GetComponent<RectTransform>().anchoredPosition = this.gameObject.GetComponent<RectTransform>().anchoredPosition - workingDimensions / 2.0f
                                                                        + new Vector2(-1.0f * padding.x, i * workingDimensions.y / (float)maxGridCountY + 10.0f);

            //set text vale/date
            labelsY[i].GetComponent<Text>().text = (i * activeGraphData.rangeY / (maxGridCountY) + activeGraphData.minY).ToString();
        }


        //disable unused labels 
        for (int i = xGridCount; i < labelsX.Count; i++)
        {
            labelsX[i].SetActive(false);
        }
    }

    void DrawLine(GraphData data, Vector2 workingDimensions, Vector2 origin)
    {
        if (data == null || data.data == null)
            return;

        lineMaterial.SetPass(0);

        for (int i = 1; i < data.Length(); i++)
        {
            GL.Begin(GL.LINES);

            for (int j = i - 1; j <= i ; j++)
            {
                //Compute X relative position
                float xCoord =  (origin.x + workingDimensions.x * ((float)data.DaysSinceStart(j)) / (float)data.rangeX) / graphSize.x;
                float yCoord = (origin.y + workingDimensions.y * (data.Value(j) - data.minY) / data.rangeY) / graphSize.y;
                
                //Set the vertex
                GL.Vertex3(xCoord, yCoord, 0.0f);
            }
            GL.End();
        }
    }
}

[System.Serializable]
public class GraphData
{
    public TimeSeries<float> data = null;
    public float minY, maxY;
    public int rangeX; //in days
    public float rangeY;
    public bool isOscillatingAroundYAxis = false;


    public GraphData(TimeSeries<float> _data)
    {
        if (_data.Length() < 2)
            return;
        
        data = _data;

        ComputeMinMaxY();
        ComputeRanges();

        if (minY * maxY < 0)
            isOscillatingAroundYAxis = true;
    }

    void ComputeMinMaxY()
    {
        minY = maxY = data.Value(0);

        for (int i = 1; i < data.Length(); i++)
        {
            minY = data.Value(i) < minY? data.Value(i) : minY;
            maxY = data.Value(i) > maxY? data.Value(i) : maxY;
        }
    }

    void ComputeRanges()
    {
        rangeX = data.End().Subtract(data.Start()).Days; //Technically speaking, assuming gap-less TS, rangeX should always be equal to Length.
        rangeY = maxY - minY;
    }

    public void AddToTimeSeries(System.DateTime date, float value)
    {
        data.AddToTimeSeries(date, value);

        ComputeMinMaxY();
        ComputeRanges();

        if (minY * maxY < 0)
            isOscillatingAroundYAxis = true;
    }

    public int Length()
    {
        return data.Length();
    }

    public int DaysSinceStart(int order)
    {
        return data.Date(order).Subtract(data.Start()).Days;
    }

    public System.DateTime DateByOffset(int offset)
    {
        return data.Date(offset);
    }

    public float Value(int order)
    {
        return data.Value(order);
    }
}

public class TimeSeries<T>
{
    public struct TimePoint<T0>
    {
        public System.DateTime date;
        public T0 value;

        public TimePoint(System.DateTime _date, T0 _value)
        {
            date = _date;
            value = _value;
        }
    }

    //public TimePoint<T>[] series;
    List<TimePoint<T>> series;

    const int minTimeSeriesEntries = 2;

    public TimeSeries(TimePoint<T>[] _series) //If the length of _series is less the minTimeSeriesEntries, TimeSeries' data will remain NULL.
    {
        if (_series.GetLength(0) < minTimeSeriesEntries)
        {
            series = null;
            return;
        }

        series = new List<TimePoint<T>>(_series);
    }

    public TimeSeries(List<System.DateTime> dates, List<T> values) //Will use the shortest of the two Lists. If shortest length less the minTimeSeriesEntries, TimeSeries' data will remain NULL.
    {
        int minLength = Mathf.Min(dates.Count, values.Count);

        if (minLength < minTimeSeriesEntries)
        {
            series = null;
            return;
        }

        //series = new TimePoint<T>[minLength];
        series = new List<TimePoint<T>>();

        for (int i = 0; i < minLength; i++)
        {
            // series[i].date = dates[i];
            // series[i].value = values[i];
            TimePoint<T> newPoint = new TimeSeries<T>.TimePoint<T>(dates[i], values[i]);
            series.Add(newPoint);
        }

    }

    public int Length()
    {
        if (series == null)
            return 0;

        //return series.GetLength(0);
        return series.Count;
    }

    public T Value(int order)
    {
        //if (order >= series.GetLength(0))
        if (order >= series.Count)
            return default(T);
        
        return series[order].value;
    }

    public System.DateTime Date(int order)
    {
        //if (series == null || order >= series.GetLength(0))
        if (series == null || order >= series.Count)
            return new System.DateTime();
        
        return series[order].date;
    }

    public System.DateTime Start()
    {
        return Date(0);
    }

    public System.DateTime End()
    {
        //return Date(series.GetLength(0) - 1);
        return Date(series.Count - 1);
    }

    public void AddToTimeSeries(System.DateTime date, T value)
    {
        series.Add(new TimePoint<T>(date, value));
    }

}