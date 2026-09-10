using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

// Code-native mesh: no copied artwork or runtime texture allocation.
internal sealed class RoundedCard : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var r = rectTransform.rect;
        Draw(vh, r, 10, new Color(.88f, .88f, .79f, .85f), false);
        var outer = 1;
        r.xMin += 1; r.yMin += 1; r.xMax -= 1; r.yMax -= 1;
        var inner = vh.currentVertCount + 1;
        Draw(vh, r, 9, new Color(.88f, .88f, .79f, .85f), false);
        // Outline ring only; a second opaque full rectangle would defeat transparency.
        for (var i = 0; i < 36; i++)
        {
            var next = (i + 1) % 36;
            vh.AddTriangle(outer + i, inner + i, inner + next);
            vh.AddTriangle(outer + i, inner + next, outer + next);
        }
        Draw(vh, r, 9, color, true);
    }
    private static void Draw(VertexHelper vh, Rect r, float radius, Color tint, bool fill)
    {
        var start = vh.currentVertCount;
        vh.AddVert(r.center, tint, Vector2.zero);
        const int perCorner = 8;
        for (var corner = 0; corner < 4; corner++)
        {
            var centre = new Vector2(corner == 0 || corner == 3 ? r.xMax - radius : r.xMin + radius,
                corner < 2 ? r.yMax - radius : r.yMin + radius);
            for (var j = 0; j <= perCorner; j++)
            {
                var angle = (corner * 90f + j * 90f / perCorner) * Mathf.Deg2Rad;
                vh.AddVert(centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, tint, Vector2.zero);
            }
        }
        var count = 4 * (perCorner + 1);
        if (fill) for (var i = 0; i < count; i++) vh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % count);
    }
}
