// Spritedow Animation Plugin by Elendow
// https://elendow.com

using UnityEngine;
using System.Collections.Generic;

namespace Elendow.SpritedowAnimator
{
    /// <summary>Asset class to store the animations.</summary>
    public class SpriteAnimation : ScriptableObject
    {
        [SerializeField]
        private int fps = 30;
        [SerializeField]
        private int totalDuration = -1;
        [SerializeField]
        private List<SpriteAnimationFrame> frames = new List<SpriteAnimationFrame>();
        [SerializeField]
        private List<SpriteAnimationAction> actions = new List<SpriteAnimationAction>();

        /// <summary>Initialize values.</summary>
        public void Setup()
        {
            totalDuration = 0;
            for (int i = 0; i < frames.Count; i++)
            {
                if (frames[i] == null)
                {
                    frames[i] = new SpriteAnimationFrame();
                }

                totalDuration += frames[i].Duration;
            }
        }

        /// <summary>Creates an empty animation.</summary>
        public SpriteAnimation()
        {
            frames = new List<SpriteAnimationFrame>();
        }

        /// <summary>Returns the sprite on the selected frame.</summary>
        /// <param name="index">The index of the frame.</param>
        /// <returns>The sprite of the selected frame.</returns>
        public Sprite GetFrame(int index)
        {
            return frames[index].Sprite;
        }

        /// <summary>Returns the duration (in frames) of the selected frame.</summary>
        /// <param name="index">The index of the frame.</param>
        /// <returns>The duration in frames of the selected frame.</returns>
        public int GetFrameDuration(int index)
        {
            return frames[index].Duration;
        }

        /// <summary>Get the frame at the specified time using the animation frame rate.</summary>
        /// <param name="time">The time in seconds of the frame.</param>
        /// <returns>The index of the frame at the desierd time.</returns>
        public int GetFrameAtTime(float time)
        {
            return GetFrameAtTime(time, fps);
        }

        /// <summary>Get the frame at the specified time using the specified frame rate.</summary>
        /// <param name="time">The time in seconds of the frame.</param>
        /// <param name="frameRate">The framerate to calculate the time.</param>
        /// <returns>The index of the frame at the desired time and framerate.</returns>
        public int GetFrameAtTime(float time, int frameRate)
        {
            int index = 0;
            float timePerFrame = 1f / frameRate;
            float totalAnimationTime = totalDuration * timePerFrame;

            if (time >= totalAnimationTime)
            {
                index = frames.Count - 1;
            }
            else if (time <= 0)
            {
                index = 0;
            }
            else
            {
                int frameDurationCounter = 0;

                while (time >= timePerFrame)
                {
                    time -= timePerFrame;
                    frameDurationCounter++;

                    if (frameDurationCounter >= frames[index].Duration)
                    {
                        index++;
                        frameDurationCounter = 0;
                    }
                }

                if (index >= frames.Count)
                {
                    index = frames.Count - 1;
                }
            }

            return index;
        }

        /// <summary>Get the total duration of the animation in seconds using the animation frame rate.</summary>
        /// <returns>The animation duration in seconds.</returns>
        public float GetAnimationDurationInSeconds()
        {
            return GetAnimationDurationInSeconds(fps);
        }

        /// <summary>Get the total duration of the animation in seconds using the specified frame rate.</summary>
        /// <param name="frameRate">The framerate to calculate the time.</param>
        /// <returns>The animation duration in seconds.</returns>
        public float GetAnimationDurationInSeconds(int frameRate)
        {
            return (float)AnimationDuration / frameRate;
        }

        /// <summary>Get the frame index at the specified normalized time (between 0 and 1) using the animation frame rate.</summary>
        /// <param name="normalizedTime">The normalized time (bestween 0 and 1) of the desired frame.</param>
        /// <returns>The frame index of the frame.</returns>
        public int GetFrameAtNormalizedTime(float normalizedTime)
        {
            normalizedTime = Mathf.Clamp(normalizedTime, 0f, 1f);
            return GetFrameAtTime(totalDuration * (1f / fps) * normalizedTime, fps);
        }

        /// <summary>Get the frame index at the specified  normalized time (between 0 and 1) using the specified frame rate.</summary>
        /// <param name="normalizedTime">The normalized time (bestween 0 and 1) of the desired frame.</param>
        /// <param name="frameRate">The framerate to calculate the time.</param>
        /// <returns>The frame index of the frame.</returns>
        public int GetFrameAtNormalizedTime(float normalizedTime, int frameRate)
        {
            normalizedTime = Mathf.Clamp(normalizedTime, 0f, 1f);
            return GetFrameAtTime(totalDuration * (1f / frameRate) * normalizedTime, frameRate);
        }

        #region Properties
        /// <summary>The framerate of the animation.</summary>
        public int FPS
        {
            get => fps;
            set => fps = value;
        }

        /// <summary>Frames on this animation.</summary>
        public int FramesCount
        {
            get => frames.Count;
        }

        /// <summary>List of frames.</summary>
        public List<SpriteAnimationFrame> Frames
        {
            get => frames;
            set => frames = value;
        }

        /// <summary>List of custom actions added to the animation.</summary>
        public List<SpriteAnimationAction> Actions
        {
            get => actions;
        }

        /// <summary>The total duration of the animation (in frames).</summary>
        public int AnimationDuration
        {
            get => totalDuration;
        }
        #endregion
    }
}