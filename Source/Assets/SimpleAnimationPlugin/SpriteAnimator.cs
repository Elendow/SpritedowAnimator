// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;

/// <summary>Animator for Sprite Renderers.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : BaseAnimator
{
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        base.Awake();
    }

    /// <summary>Changes the sprite to the given sprite</summary>
    protected override void ChangeFrame(Sprite frame)
    {
        spriteRenderer.sprite = frame;
    }

    /// <summary>Enable or disable the renderer</summary>
    public override void SetActiveRenderer(bool active)
    {
        spriteRenderer.enabled = active;
    }

    /// <summary>Flip the sprite on the X axis</summary>
    public override void FlipSpriteX(bool flip)
    {
        spriteRenderer.flipX = flip;
    }
}