using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphGenerator : MonoBehaviour
{
    [SerializeField] Material lineMaterial;
    [SerializeField] Material bgMaterial;
    [SerializeField] Image graphBG;
    [SerializeField] Vector2 graphSize;
    [SerializeField] Vector2 referenceCanvasResolution;

    void Awake()
    {
        CameraControl.postRender += TestDraw;
    }

    public Vector3 end1, end2, end3;

    void TestDraw()
    {
        Vector2 centre = this.gameObject.GetComponent<RectTransform>().anchoredPosition;
        centre += referenceCanvasResolution / 2.0f;
        GL.PushMatrix();
        GL.LoadOrtho();

        DrawBackground(centre, graphSize);


        lineMaterial.SetPass(0);
        GL.Begin(GL.LINES);

        GL.Vertex3(end1.x, end1.y, end1.z);
        GL.Vertex3(end2.x, end2.y, end2.z);
        GL.Vertex3(end3.x, end3.y, end3.z);

        GL.End();

        GL.PopMatrix();
    }

    void DrawBackground(Vector2 centre, Vector2 graphSize)
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

    void DrawLine(Vector2 centre, Vector2 graphSize, GraphData data)
    {
        for (int i = 0; i < data.length; i++)
        {
            //TODO implement graph vertices estimation here.
        }
    }
}


public struct GraphData
{
    public float[,] data;
    public float minX, maxX;
    public float minY, maxY;
    public int length;
    
    public GraphData(float[,] _data) //_data must be a n by 2 array, where first column is x data, second column is y data. n must be > 1
    {
        if (_data.GetLength(0) < 2 || _data.GetLength(1) < 2)
        {
            data = null;
            minX = minY = maxX = maxY = 0.0f;
            length = 0;
            return;
        }

        data = _data;
        minX = minY = maxX = maxY = 0.0f;
        length = data.GetLength(0);

        ComputeMinMax();
    }

    void ComputeMinMax()
    {
        minX = maxX = data[0,0];
        minY = maxY = data[0,1];
        for (int i = 1; i < length; i++)
        {
            minX = data[i,1] < minX? data[i, 0] : minX;
            maxX = data[i,1] > maxX? data[i, 0] : minX;

            minY = data[i,1] < minY? data[i, 1] : minY;
            maxY = data[i,1] > maxY? data[i, 1] : minY;
        }
    }
}