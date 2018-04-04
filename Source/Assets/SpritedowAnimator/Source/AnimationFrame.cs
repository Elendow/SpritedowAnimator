// Spritedow Animation Plugin by Elendow
// http://elendow.com

using System;
using UnityEngine;

namespace Elendow.SpritedowAnimator
{
    [Serializable]
    public class AnimationFrame
    {
        private int duration;
        private Sprite frame;

        public AnimationFrame() { }
        public AnimationFrame(Sprite frame, int duration)
        {
            this.duration = duration;
            this.frame = frame;
        }

        public int Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public Sprite Frame
        {
            get { return frame; }
            set { frame = value; }
        }
    }
}