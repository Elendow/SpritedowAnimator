// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIAnimator : BaseAnimator
{
    private Image imageRenderer;

    protected override void Awake()
    {
        base.Awake();
        imageRenderer = GetComponent<Image>();
    }

    protected override void ChangeFrame(Sprite frame)
    {
        imageRenderer.sprite = frame;
    }
}