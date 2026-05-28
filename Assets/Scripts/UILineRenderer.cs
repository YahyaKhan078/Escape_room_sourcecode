using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public float thickness = 4f;

    private Vector2 start;
    private Vector2 end;
    private bool hasPoints = false;

    public void SetPoints(Vector2 s, Vector2 e)
    {
        start = s;
        end = e;
        hasPoints = true;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!hasPoints) return;

        int segments = 24;
        float dist = Mathf.Max(Mathf.Abs(end.x - start.x) * 0.5f, 80f);
        Vector2 c1 = start + new Vector2(dist, 0);
        Vector2 c2 = end - new Vector2(dist, 0);

        Vector2 prev = start;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 curr = Bezier(start, c1, c2, end, t);
            AddQuad(vh, prev, curr);
            prev = curr;
        }
    }

    void AddQuad(VertexHelper vh, Vector2 a, Vector2 b)
    {
        Vector2 dir = (b - a).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        int idx = vh.currentVertCount;
        UIVertex v = UIVertex.simpleVert;
        v.color = color;

        v.position = a - perp; vh.AddVert(v);
        v.position = a + perp; vh.AddVert(v);
        v.position = b + perp; vh.AddVert(v);
        v.position = b - perp; vh.AddVert(v);

        vh.AddTriangle(idx, idx + 1, idx + 2);
        vh.AddTriangle(idx, idx + 2, idx + 3);
    }

    Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
    }
}