// Spritedow Animation Plugin by Elendow
// https://elendow.com

using UnityEngine;
using UnityEngine.UI;
using System;

namespace Elendow.SpritedowAnimator
{
    /// <summary>Animator for Image from the Unity UI system.</summary>
    [AddComponentMenu("Spritedow/UI Image Animator")]
    [RequireComponent(typeof(Image))]
    public class UIAnimator : BaseAnimator
    {
        #region Attributes
        public bool preserveAspectRatio;
        private Image imageRenderer;
        #endregion

        protected override void Awake()
        {
            imageRenderer = GetComponent<Image>();
            imageRenderer.preserveAspect = preserveAspectRatio;
            base.Awake();
        }

        /// <summary>Changes the sprite to the given sprite.</summary>
        protected override void ChangeFrame(Sprite frame)
        {
            base.ChangeFrame(frame);
            if (frame != null)
            {
                imageRenderer.sprite = frame;
                imageRenderer.enabled = true;
            }
            else
            {
                imageRenderer.enabled = false;
            }
            
        }

        /// <summary>Enable or disable the renderer.</summary>
        public override void SetActiveRenderer(bool active)
        {
            if (imageRenderer == null)
            {
                imageRenderer = GetComponent<Image>();
            }
            imageRenderer.enabled = active;
        }
    }
}