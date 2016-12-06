// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIAnimator : BaseAnimator
{
    private float wDiff;
    private float hDiff;
    private Vector2 initSize;
    private Vector2 firstFrameSize;
    private Image imageRenderer;

    protected override void Awake()
    {
        imageRenderer = GetComponent<Image>();
        initSize = imageRenderer.rectTransform.sizeDelta;
        firstFrameSize = imageRenderer.sprite.rect.size;
        wDiff = firstFrameSize.x / initSize.x;
        hDiff = firstFrameSize.y / initSize.y;
        base.Awake();
    }

    protected override void ChangeFrame(Sprite frame)
    {
        float newWDiff = frame.rect.size.x / initSize.x;
        float newHDiff = frame.rect.size.y / initSize.y;
        imageRenderer.rectTransform.sizeDelta = new Vector2(initSize.x * (newWDiff / wDiff), initSize.y * (newHDiff / hDiff));
        imageRenderer.sprite = frame;
    }

    public override void SetActiveRenderer(bool active)
    {
        imageRenderer.enabled = active;
    }
}