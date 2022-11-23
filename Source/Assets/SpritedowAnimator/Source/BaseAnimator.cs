// Spritedow Animation Plugin by Elendow
// https://elendow.com

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
        // Fields used in the inspector
        [SerializeField]
        private bool playOnAwake = false;
        [SerializeField]
        private bool ignoreTimeScale = false;
        [SerializeField]
        private bool delayBetweenLoops = false;
        [SerializeField]
        private bool oneShot = false;
        [SerializeField]
        private bool backwards = false;
        [SerializeField]
        private bool randomAnimation = false;
        [SerializeField]
        private bool enableRenderOnPlay = true;
        [SerializeField]
        private bool disableRendererOnFinish = false;
        [SerializeField]
        private bool startAtRandomFrame = false;
        [SerializeField]
        private float minDelayBetweenLoops = 0f;
        [SerializeField]
        private float maxDelayBetweenLoops = 2f;
        [SerializeField]
        private LoopType loopType = LoopType.repeat;
        [SerializeField]
        private LoopType fallbackLoopType;
        [SerializeField]
        private SpriteAnimation startAnimation;
        [SerializeField]
        private SpriteAnimation fallbackAnimation;
        [SerializeField]
        private SpriteAnimation[] randomAnimationList;

        // Fields used at runtime
        private bool waitingLoop = false;
        private bool randomStartFrameApplied = false;
        private bool currentOneShot = false;
        private bool currentBackwards = false;
        private bool useAnimatorFramerate = false;
        private int frameIndex = 0;
        private int stopAtFrame = -1;
        private int framesInAnimation = 0;
        private int frameDurationCounter = 0;
        private int startingFrame = -1;
        private int currentFramerate = 60;
        private float animationTimer = 0f;
        private float currentAnimationTime = 0f;
        private float loopTimer = 0f;
        private float timePerFrame = 0f;
        private LoopType currentLoopType;
        private LoopType currentFallbackLoopType;
        private SpriteAnimation currentAnimation;
        private SpriteAnimation currentFallbackAnimation;
        private UnityAction onPlay;
        private UnityAction onStop;
        private UnityAction onFinish;
        private UnityAction<SpriteAnimationAction, SpriteAnimation> animationAction;
        private Dictionary<SpriteAnimatorEventInfo, SpriteAnimatorEvent> customEvents;
        #endregion

        #region Methods
        protected virtual void Awake()
        {
            // Play the first animation if play on awake
            if (playOnAwake && startAnimation != null)
            {
                PlayOnAwake();
            }
        }

        #region Updates
        private void Update()
        {
            // We do nothing if the current FPS <= 0
            if (currentAnimation == null || currentFramerate <= 0)
            {
                return;
            }

            if (!waitingLoop)
            {
                PlayUpdate();
            }
            else
            {
                LoopUpdate();
            }
        }

        private void PlayUpdate()
        {
            // Add the delta time to the timer and the total time
            if (!ignoreTimeScale)
            {
                animationTimer += Time.deltaTime;
                currentAnimationTime = (!currentBackwards) ?
                    currentAnimationTime + Time.deltaTime :
                    currentAnimationTime - Time.deltaTime;
            }
            else
            {
                animationTimer += Time.unscaledDeltaTime;
                currentAnimationTime = (!currentBackwards) ?
                    currentAnimationTime + Time.unscaledDeltaTime :
                    currentAnimationTime - Time.unscaledDeltaTime;
            }

            if (animationTimer >= timePerFrame)
            {
                // Check frame skips
                while (animationTimer >= timePerFrame)
                {
                    frameDurationCounter++;
                    animationTimer -= timePerFrame;
                }

                // Change frame only if have passed the desired frames
                if (frameDurationCounter >= currentAnimation.Frames[frameIndex].Duration)
                {
                    while (frameDurationCounter >= currentAnimation.Frames[frameIndex].Duration)
                    {
                        frameDurationCounter -= currentAnimation.Frames[frameIndex].Duration;
                        frameIndex = (currentBackwards) ? frameIndex - 1 : frameIndex + 1;

                        // Check last or first frame
                        if (CheckLastFrame())
                        {
                            // Last frame, reset index and stop if is one shot
                            onFinish?.Invoke();

                            if (currentOneShot)
                            {
                                Stop();
                                return;
                            }
                            else
                            {
                                waitingLoop = true;
                                loopTimer = 0;

                                // Check delay between loops
                                if (delayBetweenLoops)
                                {
                                    SetActiveRenderer(!disableRendererOnFinish);
                                    if (maxDelayBetweenLoops > 0)
                                    {
                                        loopTimer = Random.Range(minDelayBetweenLoops, maxDelayBetweenLoops);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Change sprite
                            ChangeFrame(currentAnimation.GetFrame(frameIndex));

                            if (frameIndex == stopAtFrame)
                            {
                                Stop();
                            }
                        }
                    }
                }
            }
        }

        private void LoopUpdate()
        {
            // Continue looping if enough time have passed
            loopTimer -= Time.deltaTime;
            if (loopTimer <= 0)
            {
                if (currentLoopType == LoopType.yoyo)
                {
                    currentBackwards = !currentBackwards;
                }

                waitingLoop = false;
                if (randomAnimation)
                {
                    // Pick a random animation
                    PlayRandom(currentOneShot, currentBackwards);
                    return;
                }
                else
                {
                    // Continue playing the same animation
                    animationTimer = 0;
                    currentAnimationTime = (currentBackwards) ? currentAnimation.AnimationDuration * timePerFrame : 0;
                    frameIndex = (currentBackwards) ? framesInAnimation - 1 : 0;

                    SetActiveRenderer(true);
                    ChangeFrame(currentAnimation.GetFrame(frameIndex));
                    CheckEvents(frameIndex);
                }
            }
        }
        #endregion

        #region Play Methods
        private void PlayOnAwake()
        {
            currentOneShot = oneShot;
            currentBackwards = backwards;
            currentFallbackLoopType = fallbackLoopType;
            currentFallbackAnimation = fallbackAnimation;

            if (!currentOneShot)
            {
                currentLoopType = loopType;
                if (delayBetweenLoops)
                {
                    waitingLoop = true;
                    if (maxDelayBetweenLoops.Equals(minDelayBetweenLoops))
                    {
                        loopTimer = minDelayBetweenLoops;
                    }
                    else
                    {
                        loopTimer = Random.Range(minDelayBetweenLoops, maxDelayBetweenLoops);
                    }
                }
                else
                {
                    waitingLoop = false;
                    loopTimer = 0;
                }
            }

            // Pick the selected animation or a random one.
            SpriteAnimation anim = null;
            if (randomAnimation && randomAnimationList.Length > 0)
            {
                anim = randomAnimationList[Random.Range(0, randomAnimationList.Length)];
            }
            else if (startAnimation != null)
            {
                anim = startAnimation;
            }
    
            if (anim != null)
            {
                Play(anim, currentOneShot, currentBackwards, currentLoopType);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogWarning($"Animator {gameObject.name} trying to play on awake without animations", gameObject);
            }
#endif
        }

        private void StartPlay()
        {
            // If we have an animation to play, flag as playing, reset timer and take frame count
            if (currentAnimation != null)
            {
                // Failsafe for old animations without the total animation duration calculated.
                if (currentAnimation.AnimationDuration == -1)
                {
                    currentAnimation.Setup();
                }

                if (!useAnimatorFramerate)
                {
                    currentFramerate = currentAnimation.FPS;
                }

                timePerFrame = 1f / currentFramerate;
                framesInAnimation = currentAnimation.FramesCount;
                currentAnimationTime = currentBackwards ? currentAnimation.AnimationDuration * timePerFrame : 0;

                frameDurationCounter = 0;
                animationTimer = 0;
                loopTimer = 0;

                // Check if the animation have frames. Show warning if not.
                if (framesInAnimation == 0)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning($"Animation '{name}' has no frames.", gameObject);
#endif
                    enabled = false;
                    return;
                }

                Restart();

                // The first loop will have a random start frame if desired
                if (startAtRandomFrame && !randomStartFrameApplied)
                {
                    randomStartFrameApplied = true;
                    frameIndex = Random.Range(0, framesInAnimation - 1);
                }
                else if (startingFrame != -1)
                {
                    frameIndex = startingFrame;
                    if (CheckLastFrame())
                    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        Debug.LogWarning($"Starting frame '{frameIndex}' out of bounds.", gameObject);
#endif
                        frameIndex = 0;
                    }
                    startingFrame = -1;
                }
                else
                {
                    frameIndex = currentBackwards ? framesInAnimation - 1 : 0;
                }

                onPlay?.Invoke();
                enabled = true;

                if (!waitingLoop)
                {
                    ChangeFrame(currentAnimation.GetFrame(frameIndex));
                    CheckEvents(frameIndex);
                }
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogError($"Animation '{name}' not found.", gameObject);
            }
#endif
        }

        /// <summary>Plays the specified animation.</summary>
        /// <param name="animation">The animation to play.</param>
        /// <param name="playOneShot">If true, the animation will play only once.</param>
        /// <param name="playBackwards">If true, the animation will play backwards.</param>
        /// <param name="loopType">The type of the loop, repeat or yoyo.</param>
        public void Play(SpriteAnimation animation, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            if(enableRenderOnPlay)
            {
                SetActiveRenderer(true);
            }

            currentOneShot = playOneShot;
            currentBackwards = playBackwards;
            currentLoopType = loopType;

            // If it's the same animation but not playing, reset it, if playing, do nothing.
            if (currentAnimation != null && currentAnimation.Equals(animation))
            {
                if (enabled)
                {
                    Restart();
                    Resume();
                }
                else
                {
                    return;
                }
            }
            // If the animation is new, save it as current animation and play it
            else
            {
                currentAnimation = animation;
            }

            StartPlay();
        }

        /// <summary>Plays a random animation of the random animation list.</summary>
        /// <param name="playOneShot">If true, the animation will play only once.</param>
        /// <param name="playBackwards">If true, the animation will play backwards.</param>
        /// <param name="loopType">The type of the loop, repeat or yoyo.</param>
        public void PlayRandom(bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            if (randomAnimationList.Length > 0)
            {
                // Get a random animation and plays it
                int animIndex = Random.Range(0, randomAnimationList.Length);
                Play(randomAnimationList[animIndex], playOneShot, playBackwards, loopType);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogWarning($"Trying to play a random animation with an empty list.", gameObject);
            }
#endif
        }

        /// <summary>Plays an animation starting at the specified frame.</summary>
        /// <param name="animation">The animation to play.</param>
        /// <param name="frame">The frame to start the animation on.</param>
        /// <param name="playOneShot">If true, the animation will play only once.</param>
        /// <param name="playBackwards">If true, the animation will play backwards.</param>
        /// <param name="loopType">The type of the loop, repeat or yoyo.</param>
        public void PlayStartingAtFrame(SpriteAnimation animation, int frame, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            if (animation != null)
            {
                startingFrame = frame;
                Play(animation, playOneShot, playBackwards, loopType);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogWarning($"Trying to play null animation.", gameObject);
            }
#endif
        }

        /// <summary>Plays an animation starting at the specified time (in seconds).</summary>
        /// <param name="animation">The animation to play.</param>
        /// <param name="time">The time of the animation to start playing in seconds.</param>
        /// <param name="playOneShot">If true, the animation will play only once.</param>
        /// <param name="playBackwards">If true, the animation will play backwards.</param>
        /// <param name="loopType">The type of the loop, repeat or yoyo.</param>
        public void PlayStartingAtTime(SpriteAnimation animation, float time, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            if (animation != null)
            {
                Play(animation, playOneShot, playBackwards, loopType);
                SetAnimationTime(time);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogWarning($"Trying to play null animation.", gameObject);
            }
#endif
        }

        /// <summary>Plays an animation starting at the specified normalized time (between 0 and 1).</summary>
        /// <param name="animation">The animation to play.</param>
        /// <param name="normalizedTime">The normalized time (between 0 and 1) of the animation to start playing.</param>
        /// <param name="playOneShot">If true, the animation will play only once.</param>
        /// <param name="playBackwards">If true, the animation will play backwards.</param>
        /// <param name="loopType">The type of the loop, repeat or yoyo.</param>
        public void PlayStartingAtNormalizedTime(SpriteAnimation animation, float normalizedTime, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            if (animation != null)
            {
                Play(animation, playOneShot, playBackwards, loopType);
                SetAnimationNormalizedTime(normalizedTime);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                Debug.LogWarning($"Trying to play null animation.", gameObject);
            }
#endif
        }
        #endregion

        /// <summary>Resumes the animation.</summary>
        public void Resume()
        {
            if (currentAnimation != null)
            {
                enabled = true;
            }
        }

        /// <summary>Stops when reaches the desired frame. If the desired frame has already passed and the animation is not looped it will stop at the end of the animation anyway.</summary>
        /// <param name="frame">The frame to stop the animation with.</param>
        public void StopAtFrame(int frame)
        {
            stopAtFrame = frame;
        }

        /// <summary>Stops the animation.</summary>
        public void Stop()
        {
            stopAtFrame = -1;
            randomStartFrameApplied = false;
            enabled = false;
            onStop?.Invoke();
            SetActiveRenderer(!disableRendererOnFinish);

            if (currentFallbackAnimation != null)
            {
                Play(currentFallbackAnimation, false, false, currentFallbackLoopType);
            }
        }

        /// <summary>Sets the fallback animation to play and its loop type.</summary>
        /// <param name="animation">The animation to play after the current animation ends.</param>
        /// <param name="loopType">The loop type of the fallback animation, restart or yoyo.</param>
        public void SetFallbackAnimation(SpriteAnimation animation, LoopType loopType)
        {
            currentFallbackAnimation = animation;
            currentFallbackLoopType = loopType;
        }

        /// <summary>Sets the animator FPS overriding the FPS of the animation.</summary>
        /// <param name="frameRate">The framerate to play the animations.</param>
        public void UseAnimatorFPS(int frameRate)
        {
            currentFramerate = frameRate;
            timePerFrame = 1f / currentFramerate;
            useAnimatorFramerate = true;
        }

        /// <summary>Sets de animator FPS to the current animation FPS.</summary>
        public void UseAnimationFPS()
        {
            if (currentAnimation != null)
            {
                currentFramerate = currentAnimation.FPS;
                timePerFrame = 1f / currentFramerate;
                useAnimatorFramerate = false;
            }
        }

        /// <summary>Removes the fallback animation.</summary>
        public void RemoveFallbackAnimation()
        {
            currentFallbackAnimation = null;
        }

        /// <summary>Restarts the animation. If the animation is not playing the effects will apply when starts playing.</summary>
        public void Restart()
        {
            animationTimer = 0;
            frameIndex = (currentBackwards) ? framesInAnimation - 1 : 0;
            frameDurationCounter = 0;
            ChangeFrame(currentAnimation.GetFrame(frameIndex));
        }

        #region Custom actions methods
        /// <summary>Checks if there is an action on the specified index. If there is, the action is triggered.</summary>
        /// <param name="frameIndex">The index of the frame to check.</param>
        private void CheckActions(int frameIndex)
        {
            if (animationAction == null || currentAnimation.Actions == null)
            {
                return;
            }

            for (int i = 0; i < currentAnimation.Actions.Count; i++)
            {
                if (currentAnimation.Actions[i].Frame == frameIndex)
                {
                    animationAction?.Invoke(currentAnimation.Actions[i], currentAnimation);
                }
            }
        }
        #endregion

        #region Custom event methods
        /// <summary>Check if there is an event on the specified index. If there is, the event is triggered.</summary>
        /// <param name="frameIndex">The index of the frame to check.</param>
        private void CheckEvents(int frameIndex)
        {
            if (customEvents != null)
            {
                SpriteAnimatorEventInfo frameInfo = new SpriteAnimatorEventInfo(currentAnimation, frameIndex);
                if (customEvents.TryGetValue(frameInfo, out SpriteAnimatorEvent e))
                {
                    e.Invoke(this);
                }
            }
        }

        /// <summary>Adds a custom event to specified animation on a certain frame.</summary>
        /// <returns>The event created. Null if the animation is null or doesn't have enough frames.</returns>  
        /// <param name="animation">The animation to trigger the event.</param>
        /// <param name="frame">The frame to trigger the event.</param>
        public SpriteAnimatorEvent AddCustomEvent(SpriteAnimation animation, int frame)
        {
            if (animation == null || animation.FramesCount <= frame)
            {
                return null;
            }

            SpriteAnimatorEventInfo eventInfo = new SpriteAnimatorEventInfo(animation, frame);

            if (customEvents == null)
            {
                customEvents = new Dictionary<SpriteAnimatorEventInfo, SpriteAnimatorEvent>();
            }

            if (!customEvents.ContainsKey(eventInfo))
            {
                customEvents.Add(eventInfo, new SpriteAnimatorEvent());
            }

            return customEvents[eventInfo];
        }

        /// <summary>Adds a custom event to specified animation on the last frame.</summary>
        /// <returns>The event created. Null if the animation is null.</returns>  
        /// <param name="animation">The animation to trigger the event.</param>
        public SpriteAnimatorEvent AddCustomEventAtEnd(SpriteAnimation animation)
        {
            return AddCustomEvent(animation, animation.FramesCount - 1);
        }

        /// <summary>Gets the custom event of an animation on a certain frame.</summary>
        /// <returns>The event of the specified animation on the selected frame. Null if not found.</returns> 
        /// <param name="animation">The animation to get the event from.</param>
        /// <param name="frame">The frame where the event is set.</param>
        public SpriteAnimatorEvent GetCustomEvent(SpriteAnimation animation, int frame)
        {
            if (animation == null || animation.FramesCount <= frame)
            {
                return null;
            }

            SpriteAnimatorEventInfo eventInfo = new SpriteAnimatorEventInfo(animation, frame);

            if (customEvents.TryGetValue(eventInfo, out SpriteAnimatorEvent e))
            {
                return e;
            }

            return null;
        }

        /// <summary>Gets the custom event of an animation on the last frame.</summary>
        /// <returns>The event of the specified animation on the last frame. Null if not found.</returns>  
        /// <param name="animation">The animation to get the event from.</param>
        public SpriteAnimatorEvent GetCustomEventAtEnd(SpriteAnimation animation)
        {
            if (animation == null)
            {
                return null;
            }

            return GetCustomEvent(animation, animation.FramesCount - 1);
        }
        #endregion

        #region Render methods
        /// <summary>Changes the renderer to the given sprite.</summary>
        /// <param name="frame">The new sprite to render.</param>
        protected virtual void ChangeFrame(Sprite frame) 
        {
            CheckEvents(frameIndex);
            CheckActions(frameIndex);
        }

        /// <summary>Enable or disable the renderer.</summary>
        /// <param name="active">True to enable the renderer.</param>
        public virtual void SetActiveRenderer(bool active) { }

        /// <summary>Flip the sprite on the X axis.</summary>
        /// <param name="flip">True if the renderer must flip on the X axis.</param>
        public virtual void FlipSpriteX(bool flip) { }

        /// <summary>Flip the sprite on the Y axis.</summary>
        /// <param name="flip">True if the renderer must flip on the Y axis.</param>
        public virtual void FlipSpriteY(bool flip) { }
        #endregion

        /// <summary>Sets a random delay between loops.</summary>
        /// <param name="min">Minimum delay in seconds.</param>
        /// <param name="max">Maximum delay in seconds.</param>
        public void SetRandomDelayBetweenLoops(float min, float max)
        {
            delayBetweenLoops = true;
            minDelayBetweenLoops = min;
            maxDelayBetweenLoops = max;
        }

        /// <summary>Sets a delay between loops.</summary>
        /// <param name="delay">The delay in seconds.</param>
        public void SetDelayBetweenLoops(float delay)
        {
            delayBetweenLoops = true;
            minDelayBetweenLoops = delay;
            maxDelayBetweenLoops = delay;
        }

        /// <summary>Sets the animation time to the specified time, updating de sprite to the correspondent frame at that time.</summary>
        /// <param name="time">Time in seconds.</param>
        public void SetAnimationTime(float time)
        {
            if (currentAnimation != null)
            {
                float timePerFrame = 1f / currentFramerate;
                float totalAnimationTime = currentAnimation.AnimationDuration * timePerFrame;

                if (time >= totalAnimationTime)
                {
                    currentAnimationTime = totalAnimationTime;
                    animationTimer = timePerFrame;
                    frameIndex = framesInAnimation - 1;
                    frameDurationCounter = currentAnimation.Frames[frameIndex].Duration - 1;
                }
                else if (time <= 0)
                {
                    animationTimer = 0;
                    frameIndex = 0;
                    frameDurationCounter = 0;
                    currentAnimationTime = 0;
                }
                else
                {
                    frameIndex = 0;
                    frameDurationCounter = 0;
                    currentAnimationTime = time;

                    while (time >= timePerFrame)
                    {
                        time -= timePerFrame;
                        frameDurationCounter++;

                        if (frameDurationCounter >= currentAnimation.Frames[frameIndex].Duration)
                        {
                            frameIndex++;
                            frameDurationCounter = 0;
                        }
                    }

                    if (frameIndex >= framesInAnimation)
                    {
                        frameIndex = framesInAnimation - 1;
                    }

                    animationTimer = time;
                }

                ChangeFrame(currentAnimation.GetFrame(frameIndex));
            }
        }

        /// <summary>Sets the animation time to the specified normalized time (between 0 and 1), updating de sprite to the correspondent frame at that time.</summary>
        /// <param name="normalizedTime">Time normalized (between 0 and 1).</param>
        public void SetAnimationNormalizedTime(float normalizedTime)
        {
            if (currentAnimation != null)
            {
                normalizedTime = Mathf.Clamp(normalizedTime, 0f, 1f);
                SetAnimationTime(currentAnimation.AnimationDuration * timePerFrame * normalizedTime);
            }
        }

        /// <summary>Check the last frame (backwards or not).</summary>
        /// <returns>If the animation is on the last frame.</returns>
        private bool CheckLastFrame()
        {
            if((!currentBackwards && frameIndex > framesInAnimation - 1))
            {
                frameIndex = framesInAnimation - 1;
                return true;
            }
            else if (currentBackwards && frameIndex < 0)
            {
                frameIndex = 0;
                return true;
            }

            return false;
        }
        #endregion

        #region Properties
        /// <summary>Gets if the animator is playing an animation or not.</summary>
        public bool IsPlaying
        {
            get => enabled;
        }

        /// <summary>If true, the animator will disable the renderer when the animation ends.</summary>
        public bool DisableRendererOnFinish
        {
            set => disableRendererOnFinish = value;
        }

        /// <summary>If true the animator will get a random animation after every loop cycle.</summary>
        public bool RandomAnimation
        {
            set => randomAnimation = value;
        }

        /// <summary>If true a delay will be made between loops.</summary>
        public bool DelayBetweenLoops
        {
            set => delayBetweenLoops = value;
        }

        /// <summary>If true, the timescale of the game will be ignored.</summary>
        public bool IgnoreTimeScale
        {
            set => ignoreTimeScale = value;
        }

        /// <summary>The animation will start at a random frame if this is true.</summary>
        public bool StartAtRandomFrame
        {
            set => startAtRandomFrame = value;
        }

        /// <summary>The current frame of the animation.</summary>
        public int CurrentFrame
        {
            get => frameIndex;
        }

        /// <summary>The current FPS of the animator (it could be the animation FPS or an overrided FPS).</summary>
        public int CurrentFrameRate
        {
            get => currentFramerate;
        }

        /// <summary>The current time in seconds of the playing animation.</summary>
        public float CurrentAnimationTime
        {
            get => currentAnimationTime;
        }

        /// <summary>The current time of the playing animation normalized (between 0 and 1).</summary>
        public float CurrentAnimationTimeNormalized
        {
            get => Mathf.InverseLerp(0, currentAnimationTime, currentAnimation.GetAnimationDurationInSeconds());
        }

        /// <summary>The currently playing animation.</summary>
        public SpriteAnimation PlayingAnimation
        {
            get => currentAnimation;
        }

        /// <summary>This event is called when the animation finishes.</summary>
        public UnityAction OnFinish
        {
            get => onFinish;
            set => onFinish = value;
        }

        /// <summary>This event is called when the animation starts playing.</summary>
        public UnityAction OnPlay
        {
            get => onPlay;
            set => onPlay = value;
        }

        /// <summary>This event is called when the animation is stopped.</summary>
        public UnityAction OnStop
        {
            get => onStop;
            set => onStop = value;
        }

        /// <summary>This action invokes the custom actions defined on the Sprite Animation window.</summary>
        public UnityAction<SpriteAnimationAction, SpriteAnimation> OnAnimationAction
        {
            get => animationAction;
            set => animationAction = value;
        }
        #endregion
    }
}