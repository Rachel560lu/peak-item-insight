using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

// A clipped-tip triangle, not a font glyph: works with every game language.
internal sealed class PreviewDeltaArrow : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var rect = rectTransform.rect;
        var points = new[] { new Vector2(.44f, 1), new Vector2(.56f, 1), new Vector2(1, .12f),
            new Vector2(.94f, 0), new Vector2(.06f, 0), new Vector2(0, .12f) };
        vh.AddVert(rect.center, color, Vector2.zero);
        foreach (var p in points)
            vh.AddVert(new Vector2(rect.xMin + p.x * rect.width, rect.yMin + p.y * rect.height), color, Vector2.zero);
        for (var i = 0; i < points.Length; i++) vh.AddTriangle(0, i + 1, (i + 1) % points.Length + 1);
    }
}
