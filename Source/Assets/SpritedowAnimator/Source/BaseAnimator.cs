// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// Base class for the animation system. Controls the sprites and events.
    /// </summary>
    [AddComponentMenu("")] // This makes this class invisible on the components menu
    public class BaseAnimator : MonoBehaviour
    {
        #region Attributes
        public bool playOnAwake = false;
        public bool ignoreTimeScale = false;
        public string startAnimation;
        public List<SpriteAnimation> animations;

        /// <summary>
        /// Called when the animation ends.
        /// </summary>
        public UnityEvent onFinish;
        /// <summary>
        /// Called when the animation is stopped.
        /// </summary>
        public UnityEvent onStop;
        /// <summary>
        /// Called when the animation starts playing.
        /// </summary>
        public UnityEvent onPlay;

        [SerializeField]
        private bool oneShot;
        [SerializeField]
        private bool backwards;
        [SerializeField]
        private bool randomAnimation = false;

        private bool playing;
        private bool disableRenderer;
        private int animationIndex;
        private int framesInAnimation;
        private int frameDurationCounter;
        private float animationTimer;
        private SpriteAnimation currentAnimation;
        private Dictionary<SpriteAnimatorEventInfo, SpriteAnimatorEvent> customEvents;
        #endregion

        #region Methods
        protected virtual void Awake()
        {
            // Why an animator without animation?
            if (animations.Count == 0)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Sprite animator without animations.", gameObject);
#endif
                enabled = false;
                return;
            }

            // Play the first animation if play on awake
            if (playOnAwake)
            {
                if (!randomAnimation)
                    Play(startAnimation, oneShot, backwards);
                else
                    PlayRandom(oneShot, backwards);
            }
        }

        private void Update()
        {
            // We do nothing if FPS <= 0
            if (currentAnimation == null || currentAnimation.FPS <= 0)
                return;

            if (playing)
            {
                if (!ignoreTimeScale)
                    animationTimer += Time.deltaTime;
                else
                    animationTimer += Time.unscaledDeltaTime;

                if (1f / currentAnimation.FPS < animationTimer)
                {
                    // Check frames duration
                    frameDurationCounter++;
                    animationTimer = 0;

                    if (frameDurationCounter >= currentAnimation.FramesDuration[animationIndex])
                    {
                        // Change frame only if have passed the desired frames
                        animationIndex = (backwards) ? animationIndex - 1 : animationIndex + 1;
                        frameDurationCounter = 0;

                        // Check last or first frame
                        if (animationIndex >= framesInAnimation || animationIndex < 0)
                        {
                            // Last frame, reset index and stop if is one shot
                            onFinish.Invoke();

                            if (oneShot)
                            {
                                Stop();
                                return;
                            }
                            else if (randomAnimation)
                                PlayRandom(oneShot, backwards);
                            else
                                animationIndex = (backwards) ? framesInAnimation - 1 : 0;
                        }

                        // Change sprite
                        ChangeFrame(currentAnimation.GetFrame(animationIndex));

                        // Check events
                        SpriteAnimatorEventInfo frameInfo = new SpriteAnimatorEventInfo(currentAnimation.Name, animationIndex);
                        if (customEvents != null && customEvents.ContainsKey(frameInfo))
                            customEvents[frameInfo].Invoke(this);
                    }
                }
            }
        }

        /// <summary>
        /// Plays the first animation of the animation list.
        /// </summary>
        public void Play(bool playOneShot = false, bool playBackwards = false)
        {
            Play(animations[0].Name, playOneShot, playBackwards);
        }

        /// <summary>
        /// Plays an animation.
        /// </summary>
        public void Play(string name, bool playOneShot = false, bool playBackwards = false)
        {
            SetActiveRenderer(true);

            oneShot = playOneShot;
            backwards = playBackwards;

            // If it's the same animation but not playing, reset it, if playing, do nothing.
            if (currentAnimation != null && currentAnimation.Name == name)
            {
                if (!playing)
                {
                    Restart();
                    Resume();
                }
                return;
            }
            else if (currentAnimation != null && currentAnimation.Name == name && playing)
                return;
            // Look for the animation only if its new or current animation is null
            else if (currentAnimation == null || currentAnimation.Name != name)
                currentAnimation = GetAnimation(name);

            // If we have an animation to play, flag as playing, reset timer and take frame count
            if (currentAnimation != null)
            {
                // Check if the animation have frames. Show warning if not.
                if (currentAnimation.FramesCount == 0)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning("Animation '" + name + "' has no frames.", gameObject);
#endif
                    playing = false;
                    return;
                }

                onPlay.Invoke();
                Restart();
                playing = true;
                animationIndex = (backwards) ? framesInAnimation - 1 : 0;
                framesInAnimation = currentAnimation.FramesCount;
                ChangeFrame(currentAnimation.GetFrame(animationIndex));
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
                Debug.LogError("Animation '" + name + "' not found.", gameObject);
#endif
        }

        /// <summary>
        /// Plays a random animation of the animation list.
        /// </summary>
        public void PlayRandom(bool playOneShot = false, bool playBackwards = false)
        {
            // Get a random animation a plays it
            int animIndex = Random.Range(0, animations.Count);
            Play(animations[animIndex].Name, playOneShot, playBackwards);
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        public void Resume()
        {
            if (currentAnimation != null)
                playing = true;
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            playing = false;
            onStop.Invoke();
            SetActiveRenderer(!disableRenderer);
        }

        /// <summary>
        /// Restarts the animation. If the animation is not playing the effects will apply when starts playing.
        /// </summary>
        public void Restart()
        {
            animationTimer = 0;
            animationIndex = (backwards) ? currentAnimation.FramesCount - 1 : 0;
            frameDurationCounter = 0;
        }

        /// <summary>
        /// Adds a custom event to the first animation on the list on a certain frame.
        /// </summary>
        /// <returns>
        /// The event created. Null if the animation is not found or doesn't have enough frames.
        /// </returns>  
        public SpriteAnimatorEvent AddCustomEvent(int frame)
        {
            return AddCustomEvent(animations[0].Name, frame);
        }

        /// <summary>
        /// Adds a custom event to specified animation on the list on a certain frame.
        /// </summary>
        /// <returns>
        /// The event created. Null if the animation is not found or doesn't have enough frames.
        /// </returns>  
        public SpriteAnimatorEvent AddCustomEvent(string animation, int frame)
        {
            SpriteAnimation anim = GetAnimation(animation);
            if (anim == null || anim.FramesCount <= frame)
                return null;

            SpriteAnimatorEventInfo eventInfo = new SpriteAnimatorEventInfo(animation, frame);

            if (customEvents == null)
                customEvents = new Dictionary<SpriteAnimatorEventInfo, SpriteAnimatorEvent>();

            if (!customEvents.ContainsKey(eventInfo))
                customEvents.Add(eventInfo, new SpriteAnimatorEvent());

            return customEvents[eventInfo];
        }

        /// <summary>
        /// Gets the custom event of the first animation on the list on a certain frame.
        /// </summary>
        /// <returns>
        /// The event of the first animation on the selected frame. Null if not found.
        /// </returns>  
        public SpriteAnimatorEvent GetCustomEvent(int frame)
        {
            if (animations.Count == 0)
                return null;

            return GetCustomEvent(animations[0].Name, frame);
        }

        /// <summary>
        /// Gets the custom event of an animation on a certain frame.
        /// </summary>
        /// <returns>
        /// The event of the specified animation on the selected frame. Null if not found.
        /// </returns>  
        public SpriteAnimatorEvent GetCustomEvent(string animation, int frame)
        {
            if (animations.Count == 0)
                return null;

            SpriteAnimation anim = GetAnimation(animation);
            if (anim == null || anim.FramesCount <= frame)
                return null;

            SpriteAnimatorEventInfo eventInfo = new SpriteAnimatorEventInfo(animation, frame);
            if (customEvents.ContainsKey(eventInfo))
                return customEvents[eventInfo];
            else
                return null;
        }

        /// <summary>
        /// Search an animation with the given name.
        /// </summary>
        /// <returns>
        /// The animation. Null if not found.
        /// </returns>  
        private SpriteAnimation GetAnimation(string name)
        {
            return animations.Find(x => x.Name.Contains(name));
        }

        /// <summary>
        /// Changes the renderer to the given sprite
        /// </summary>
        protected virtual void ChangeFrame(Sprite frame) { }

        /// <summary>
        /// Enable or disable the renderer
        /// </summary>
        public virtual void SetActiveRenderer(bool active) { }

        /// <summary>
        /// Flip the sprite on the X axis
        /// </summary>
        public virtual void FlipSpriteX(bool flip) { }
        #endregion

        #region Properties
        /// <summary>
        /// Gets if the animator is playing an animation or not.
        /// </summary>
        public bool IsPlaying
        {
            get { return playing; }
        }

        /// <summary>
        /// If true, the animator will disable the renderer when the animation ends.
        /// </summary>
        public bool DisableRendererOnFinish
        {
            set { disableRenderer = value; }
        }


        /// <summary>
        /// If true the animator will get a random animation after every loop cicle
        /// </summary>
        public bool RandomAnimation
        {
            set { randomAnimation = value; }
        }

        /// <summary>
        /// The currently playing animation name.
        /// </summary>
        public string CurrentAnimation
        {
            get { return currentAnimation.Name; }
        }
        #endregion
    }
}