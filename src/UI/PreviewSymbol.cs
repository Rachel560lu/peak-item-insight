using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

internal enum PreviewSymbolKind { Clock, Shoe, Shield }

// Mesh icons stay crisp at every scale and do not rely on emoji font coverage.
internal sealed class PreviewSymbol : MaskableGraphic
{
    internal PreviewSymbolKind Kind;
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (Kind == PreviewSymbolKind.Clock)
        {
            const int segments = 32;
            for (var i = 0; i < segments; i++)
            {
                var a = i * Mathf.PI * 2 / segments;
                var b = (i + 1) * Mathf.PI * 2 / segments;
                Quad(vh, Point(a, .46f), Point(b, .46f), Point(b, .37f), Point(a, .37f));
            }
            Quad(vh, new Vector2(.46f,.47f), new Vector2(.54f,.47f), new Vector2(.54f,.79f), new Vector2(.46f,.79f));
            Quad(vh, new Vector2(.50f,.45f), new Vector2(.76f,.45f), new Vector2(.76f,.53f), new Vector2(.50f,.53f));
        }
        else if (Kind == PreviewSymbolKind.Shoe)
        {
            Quad(vh, new Vector2(.08f,.72f), new Vector2(.39f,.72f), new Vector2(.50f,.35f), new Vector2(.08f,.35f));
            Quad(vh, new Vector2(.08f,.35f), new Vector2(.88f,.35f), new Vector2(.95f,.15f), new Vector2(.08f,.15f));
        }
        else
            Polygon(vh, new[] { new Vector2(.5f,.04f), new Vector2(.1f,.42f), new Vector2(.1f,.84f),
                new Vector2(.5f,.96f), new Vector2(.9f,.84f), new Vector2(.9f,.42f) });
    }
    private static Vector2 Point(float angle, float radius) => new Vector2(.5f + Mathf.Cos(angle) * radius, .5f + Mathf.Sin(angle) * radius);
    private void Vert(VertexHelper vh, Vector2 p)
    {
        var r = rectTransform.rect;
        vh.AddVert(new Vector2(r.xMin + p.x * r.width, r.yMin + p.y * r.height), color, Vector2.zero);
    }
    private void Quad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        var start = vh.currentVertCount;
        Vert(vh,a); Vert(vh,b); Vert(vh,c); Vert(vh,d);
        vh.AddTriangle(start,start+1,start+2); vh.AddTriangle(start,start+2,start+3);
    }
    private void Polygon(VertexHelper vh, Vector2[] points)
    {
        var start = vh.currentVertCount;
        Vert(vh,new Vector2(.5f,.5f));
        foreach (var point in points) Vert(vh,point);
        for (var i=0;i<points.Length;i++) vh.AddTriangle(start,start+1+i,start+1+(i+1)%points.Length);
    }
}
