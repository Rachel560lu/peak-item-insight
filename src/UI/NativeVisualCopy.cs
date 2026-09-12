using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

// Recreate visuals while inactive; never instantiate a HUD/gameplay controller.
// Unity serialization preserves Image subclasses, sprite/material references and
// private serialized appearance fields without mutating any shared asset.
internal static class NativeVisualCopy
{
    internal static RectTransform Create(RectTransform source, Transform parent, Transform? exclude = null)
    {
        var go = new GameObject(source.name, typeof(RectTransform));
        go.SetActive(false);
        var copy = go.GetComponent<RectTransform>();
        copy.SetParent(parent, false);
        copy.anchorMin = source.anchorMin; copy.anchorMax = source.anchorMax;
        copy.pivot = source.pivot; copy.sizeDelta = source.sizeDelta;
        copy.anchoredPosition = source.anchoredPosition;
        copy.localRotation = source.localRotation; copy.localScale = source.localScale;
        var image = source.GetComponent<Image>();
        if (image != null) CopyImage(image, copy);
        var mask = source.GetComponent<Mask>();
        if (mask != null) copy.gameObject.AddComponent<Mask>().showMaskGraphic = mask.showMaskGraphic;
        if (source.GetComponent<RectMask2D>() != null) copy.gameObject.AddComponent<RectMask2D>();
        foreach (Transform child in source)
            if (child is RectTransform rect && child != exclude && !child.name.StartsWith("PeakItemInsight"))
                Create(rect, copy, exclude);
        go.SetActive(source.gameObject.activeSelf);
        return copy;
    }

    internal static Image CopyImage(Image source, RectTransform destination)
    {
        var active = destination.gameObject.activeSelf;
        destination.gameObject.SetActive(false);
        var image = (Image)destination.gameObject.AddComponent(source.GetType());
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), image);
        // Original shadows/glows use UniformModifier. Without it ProceduralImage
        // silently creates FreeModifier, changing native rounded corners.
        foreach (var modifier in source.GetComponents<UnityEngine.UI.ProceduralImage.ProceduralImageModifier>())
        {
            var copy = destination.gameObject.AddComponent(modifier.GetType());
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(modifier), copy);
        }
        image.raycastTarget = false;
        destination.gameObject.SetActive(active);
        return image;
    }

    internal static RectTransform? Find(RectTransform originalRoot, RectTransform copyRoot, RectTransform? original)
    {
        if (original == null) return null;
        if (original == originalRoot) return copyRoot;
        var path = original.name;
        for (var node = original.parent; node != originalRoot; node = node.parent)
        {
            if (node == null) return null;
            path = node.name + "/" + path;
        }
        return copyRoot.Find(path) as RectTransform;
    }
}
