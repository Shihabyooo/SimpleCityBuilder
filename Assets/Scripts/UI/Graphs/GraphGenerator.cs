using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//This graph generator is specificly written for Timeseries. i.e. the X-axis is time (in days).

public class GraphGenerator : MonoBehaviour
{
    [SerializeField] Material lineMaterial;
    [SerializeField] Material bgMaterial;
    //[SerializeField] Image graphBG;
    [SerializeField] Vector2 graphSize;
    [SerializeField] Vector2 referenceCanvasResolution;
    [SerializeField] Vector2 padding;

    Vector2 centre;
    GraphData activeGraphData = null;
    [SerializeField] bool isShown = false;

    void Awake()
    {
    }

    public bool ShowGraph(TimeSeries<float> data)
    {
        if (!PrepareGraphData(data))
            return false;

        CameraControl.postRender += DrawGraph;
        isShown = true;
        return true;
    }

    public void CloseGraph()
    {
        if (!isShown)
            return;

        CameraControl.postRender -= DrawGraph;
        isShown = false;
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

    void DrawGraph()
    {
        if(activeGraphData == null)
            return;

        centre = this.gameObject.GetComponent<RectTransform>().anchoredPosition;
        centre += referenceCanvasResolution / 2.0f;
        GL.PushMatrix();
        GL.LoadOrtho();

        DrawBackground();
        DrawLine(activeGraphData);

        GL.PopMatrix();
    }

    void DrawBackground()
    {
        bgMaterial.SetPass(0);
        GL.Begin(GL.QUADS);
        
        GL.TexCoord2(0.0f, 0.0f);
        GL.Vertex3((centre.x - graphSize.x / 2.0f) / referenceCanvasResolution.x, (centre.y - graphSize.y / 2.0f) / referenceCanvasResolution.y, 0.0f);
        GL.TexCoord2(0.0f, 1.0f);
        GL.Vertex3((centre.x - graphSize.x / 2.0f) / referenceCanvasResolution.x, (centre.y + graphSize.y / 2.0f) / referenceCanvasResolution.y, 0.0f);
        GL.TexCoord2(1.0f, 1.0f);
        GL.Vertex3((centre.x + graphSize.x / 2.0f) / referenceCanvasResolution.x, (centre.y + graphSize.y / 2.0f) / referenceCanvasResolution.y, 0.0f);
        GL.TexCoord2(1.0f, 0.0f);
        GL.Vertex3((centre.x + graphSize.x / 2.0f) / referenceCanvasResolution.x, (centre.y - graphSize.y / 2.0f) / referenceCanvasResolution.y, 0.0f);

        GL.End();
    }

    void DrawLine(GraphData data)
    {
        if (data == null || data.data == null)
            return;

        Vector2 workingDimensions = graphSize - (padding * 2.0f);
        Vector2 origin = (centre - workingDimensions / 2.0f);

        lineMaterial.SetPass(0);

        for (int i = 1; i < data.Length(); i++)
        {
            //print ("===========================Drawing: ");
            GL.Begin(GL.LINES);
            for (int j = i - 1; j <= i ; j++)
            {
                //print (data.DaysSinceStart(j) + ", " + data.Value(j));

                //Compute X relative position
                float xCoord =  (origin.x + workingDimensions.x * ((float)data.DaysSinceStart(j)) / (float)data.rangeX) / referenceCanvasResolution.x;
                float yCoord = (origin.y + workingDimensions.y * (data.Value(j) - data.minY) / data.rangeY) / referenceCanvasResolution.y;
                
                //Set the vertex
                GL.Vertex3(xCoord, yCoord, 0.0f);
            }
            //print ("===========================End: ");
            GL.End();
        }
    }

    // //test
    // public Vector2[] testData;
    // void OnValidate()
    // {
    //     if (UnityEditor.EditorApplication.isPlaying && testData != null)
    //     {

    //         List<System.DateTime> dates = new List<System.DateTime>();
    //         List<float> values = new List<float>();
            
    //         for (int i = 0; i < testData.GetLength(0); i++)
    //         {
    //            dates.Add(new System.DateTime(2000, 1, (int)testData[i].x));
    //            values.Add(testData[i].y);
    //         }

    //         TimeSeries<float> testSeries = new TimeSeries<float>(dates, values);

    //         ShowGraph(testSeries);
    //     }
    // }
}


[System.Serializable]
public class GraphData
{
    public TimeSeries<float> data = null;
    public float minY, maxY;
    public int rangeX; //in days
    public float rangeY;
    
    public GraphData(TimeSeries<float> _data)
    {
        if (_data.Length() < 2)
            return;
        
        data = _data;

        ComputeMinMaxY();
        ComputeRanges();
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

    public int Length()
    {
        return data.Length();
    }

    public int DaysSinceStart(int order)
    {
        return data.Date(order).Subtract(data.Start()).Days;
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

    public TimePoint<T>[] series;

    const int minTimeSeriesEntries = 2;

    public TimeSeries(TimePoint<T>[] _series) //If the length of _series is less the minTimeSeriesEntries, TimeSeries' data will remain NULL.
    {
        if (_series.GetLength(0) < minTimeSeriesEntries)
        {
            series = null;
            return;
        }

        series = _series;
    }

    public TimeSeries(List<System.DateTime> dates, List<T> values) //Will use the shortest of the two Lists. If shortest length less the minTimeSeriesEntries, TimeSeries' data will remain NULL.
    {
        int minLength = Mathf.Min(dates.Count, values.Count);

        if (minLength < minTimeSeriesEntries)
        {
            series = null;
            return;
        }

        series = new TimePoint<T>[minLength];

        for (int i = 0; i < minLength; i++)
        {
            series[i].date = dates[i];
            series[i].value = values[i];
        }

    }

    public int Length()
    {
        return series.GetLength(0);
    }

    public T Value(int order)
    {
        if (order >= series.GetLength(0))
            return default(T);
        
        return series[order].value;
    }

    public System.DateTime Date(int order)
    {
        if (series == null || order >= series.GetLength(0))
            return new System.DateTime();
        
        return series[order].date;
    }

    public System.DateTime Start()
    {
        return Date(0);
    }

    public System.DateTime End()
    {
        return Date(series.GetLength(0) - 1);
    }
}