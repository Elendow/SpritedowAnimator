// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using UnityEngine.UI;

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// Animator for Image from the Unity UI system.
    /// </summary>
    [AddComponentMenu("Spritedow/UI Image Animator")]
    [RequireComponent(typeof(Image))]
    public class UIAnimator : BaseAnimator
    {
        public bool adaptPivot;
        private float wDiff;
        private float hDiff;
        private Vector2 initSize;
        private Vector2 firstFrameSize;
        private Image imageRenderer;

        protected override void Awake()
        {
            imageRenderer = GetComponent<Image>();
            initSize = imageRenderer.rectTransform.sizeDelta;
            if (imageRenderer.sprite != null)
            {
                firstFrameSize = imageRenderer.sprite.rect.size;
                wDiff = firstFrameSize.x / initSize.x;
                hDiff = firstFrameSize.y / initSize.y;
            }
            base.Awake();
        }

        /// <summary>
        /// Changes the sprite to the given sprite.
        /// </summary>
        protected override void ChangeFrame(Sprite frame)
        {
            if (frame != null)
            {
                // Unity UI system doesn't change the rect size to the sprite rect size, we do it manually using the initial size as a reference.
                float newWDiff = frame.rect.size.x / initSize.x;
                float newHDiff = frame.rect.size.y / initSize.y;
                imageRenderer.rectTransform.sizeDelta = new Vector2(initSize.x * (newWDiff / wDiff), initSize.y * (newHDiff / hDiff));
                imageRenderer.sprite = frame;

                if (adaptPivot)
                    imageRenderer.rectTransform.pivot = new Vector2(frame.pivot.x / frame.rect.size.x, frame.pivot.y / frame.rect.size.y);

                imageRenderer.enabled = true;
            }
            else
                imageRenderer.enabled = false;
        }

        /// <summary>
        /// Enable or disable the renderer
        /// </summary>
        public override void SetActiveRenderer(bool active)
        {
            if (imageRenderer == null)
                imageRenderer = GetComponent<Image>();
            imageRenderer.enabled = active;
        }

        /// <summary>
        /// Flip the sprite on the X axis
        /// </summary>
        public override void FlipSpriteX(bool flip)
        {
            //TODO Use the rect transform of the Image to flip the sprite.
        }
    }
}