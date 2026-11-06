using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UILineRenderer : Graphic
{
    [HideInInspector] public Vector2[] points = new Vector2[3]; 
    public float thickness = 15f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (points == null || points.Length < 2) return;

        for (int i = 0; i < points.Length - 1; i++)
        {
            CreateLineSegment(points[i], points[i + 1], vh);
        }
    }

    void CreateLineSegment(Vector2 start, Vector2 end, VertexHelper vh)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness / 2);

        int vCount = vh.currentVertCount;

        vh.AddVert(start + normal, color, Vector2.zero);
        vh.AddVert(start - normal, color, Vector2.zero);
        vh.AddVert(end - normal, color, Vector2.zero);
        vh.AddVert(end + normal, color, Vector2.zero);

        vh.AddTriangle(vCount, vCount + 1, vCount + 2);
        vh.AddTriangle(vCount, vCount + 2, vCount + 3);
    }

    void Update() => SetAllDirty();
}
