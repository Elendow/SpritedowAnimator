// Spritedow Animation Plugin by Elendow
// https://elendow.com

using System;
using UnityEngine;

namespace Elendow.SpritedowAnimator
{
    [Serializable]
    public class SpriteAnimationFrame
    {
        #region Atributes
        [SerializeField]
        private int duration;
        [SerializeField]
        private Sprite sprite;
        #endregion

        public SpriteAnimationFrame() { }
        
        public SpriteAnimationFrame(Sprite sprite, int duration)
        {
            this.duration = duration;
            this.sprite = sprite;
        }

        #region Properties
        public int Duration
        {
            get => duration;
            set => duration = value;
        }

        public Sprite Sprite
        {
            get => sprite;
            set => sprite = value;
        }
        #endregion
    }
}