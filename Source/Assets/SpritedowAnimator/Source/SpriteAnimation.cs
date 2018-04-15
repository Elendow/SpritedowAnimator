// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using System.Collections.Generic;

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// Asset class to store the animations.
    /// </summary>
    public class SpriteAnimation : ScriptableObject
    {
        [SerializeField]
        private int fps = 30;
        [SerializeField]
        private List<Sprite> frames;
        [SerializeField]
        private List<int> framesDuration;

        /// <summary>
        /// Creates an empty animation.
        /// </summary>
        public SpriteAnimation()
        {
            frames = new List<Sprite>();
            framesDuration = new List<int>();
        }

        /// <summary>
        /// Returns the sprite on the selected frame.
        /// </summary>
        public Sprite GetFrame(int index)
        {
        	return frames[index];
        }

        /// <summary>
        /// Returns the duration (in frames) of the selected frame.
        /// </summary>
        public int GetFrameDuration(int index)
        {
            return framesDuration[index];
        }

        /// <summary>
        /// Name property.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// FPS property
        /// </summary>
        public int FPS
        {
            get { return fps; }
            set { fps = value; }
        }

        /// <summary>
        /// Frames on this animation.
        /// </summary>
        public int FramesCount
        {
            get { return frames.Count; }
        }

        /// <summary>
        /// List of sprites.
        /// </summary>
        public List<Sprite> Frames
        {
            get { return frames; }
            set { frames = value; }
        }

        /// <summary>
        /// List with the duration of each frame.
        /// </summary>
        public List<int> FramesDuration
        {
            get { return framesDuration; }
            set { framesDuration = value; }
        }
    }
}