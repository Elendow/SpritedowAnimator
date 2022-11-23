// Spritedow Animation Plugin by Elendow
// https://elendow.com

using UnityEngine;
using System;

namespace Elendow.SpritedowAnimator
{
    /// <summary>Animator for Sprite Renderers.</summary>
    [AddComponentMenu("Spritedow/Sprite Animator")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : BaseAnimator
    {
        #region Attributes
        [NonSerialized]
        private SpriteRenderer spriteRenderer;
        #endregion

        protected override void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            base.Awake();
        }

        /// <summary>Changes the sprite to the given sprite.</summary>
        /// <param name="frame">The new sprite to render.</param>
        protected override void ChangeFrame(Sprite frame)
        {
            base.ChangeFrame(frame);
            spriteRenderer.sprite = frame;
        }

        /// <summary>Enable or disable the renderer.</summary>
        /// <param name="active">True to enable the renderer.</param>
        public override void SetActiveRenderer(bool active)
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = active;
        }

        /// <summary>Flip the sprite on the X axis.</summary>
        /// <param name="flip">True if the renderer must flip on the X axis.</param>
        public override void FlipSpriteX(bool flip)
        {
            spriteRenderer.flipX = flip;
        }

        /// <summary>Flip the sprite on the Y axis.</summary>
        /// <param name="flip">True if the renderer must flip on the Y axis.</param>
        public override void FlipSpriteY(bool flip)
        {
            spriteRenderer.flipY = flip;
        }

        #region Properties
        /// <summary>The bounds of the Sprite Renderer.</summary>
        public Bounds SpriteBounds
        {
            get => spriteRenderer.bounds;
        }

        /// <summary>The Sprite Renderer used to render.</summary>
        public SpriteRenderer SpriteRenderer
        {
            get => spriteRenderer;
        }
        #endregion
    }
}