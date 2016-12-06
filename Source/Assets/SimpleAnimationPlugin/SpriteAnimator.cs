// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : BaseAnimator
{
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        base.Awake();
    }

    protected override void ChangeFrame(Sprite frame)
    {
        spriteRenderer.sprite = frame;
    }

    public override void SetActiveRenderer(bool active)
    {
        spriteRenderer.enabled = active;
    }

    public override void FlipSpriteX(bool flip)
    {
        spriteRenderer.flipX = flip;
    }
}