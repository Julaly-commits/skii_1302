using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sizes a background image so it always covers its parent, cropping the
/// overflow instead of squashing the picture. Needed because the menu art is
/// portrait while the screen is landscape - a plain stretch would distort it.
/// The parent is expected to clip, via a RectMask2D or a Mask.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class BackgroundCover : MonoBehaviour
{
    [SerializeField]
    private Image image;

    private RectTransform rect;

    void OnEnable()
    {
        Fit();
    }

    void OnRectTransformDimensionsChange()
    {
        Fit();
    }

    void Update()
    {
        // The parent resizes when the game window does, which raises no event
        // on this object, so keep checking. The work is a couple of divisions.
        Fit();
    }

    private void Fit()
    {
        if (rect == null)
            rect = (RectTransform)transform;

        if (image == null)
            image = GetComponent<Image>();

        if (image == null || image.sprite == null)
            return;

        if (rect.parent is not RectTransform parent)
            return;

        var area = parent.rect.size;
        var native = image.sprite.rect.size;

        if (area.x <= 0f || area.y <= 0f || native.x <= 0f || native.y <= 0f)
            return;

        // Cover: take the larger of the two ratios so neither axis is short.
        float scale = Mathf.Max(area.x / native.x, area.y / native.y);
        var wanted = native * scale;

        if (rect.sizeDelta != wanted)
            rect.sizeDelta = wanted;

        if (rect.anchoredPosition != Vector2.zero)
            rect.anchoredPosition = Vector2.zero;
    }
}
