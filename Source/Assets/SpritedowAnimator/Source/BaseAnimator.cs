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
        // Still don't know how to serialize correctly a list without making it public...
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
        private bool disableRendererOnFinish;
        [SerializeField]
        private bool startAtRandomFrame = false;
        [SerializeField]
        private float minDelayBetweenLoops = 0f;
        [SerializeField]
        private float maxDelayBetweenLoops = 2f;
        [SerializeField]
        private string startAnimation = "";
        [SerializeField]
        private LoopType loopType = LoopType.repeat;
        [SerializeField]
        private SpriteAnimation fallbackAnimation;
        [SerializeField]
        private LoopType fallbackLoopType;

        // Fields used at runtime
        private bool playing = false;
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
        private SpriteAnimation currentAnimation;
        private Dictionary<SpriteAnimatorEventInfo, SpriteAnimatorEvent> customEvents;
        #endregion

        #region Methods
        protected virtual void Awake()
        {
            // Event check for runtime instantiation
            if (onPlay == null) onPlay = new UnityEvent();
            if (onFinish == null) onFinish = new UnityEvent();
            if (onStop == null) onStop = new UnityEvent();

            // Play the first animation if play on awake
            if (playOnAwake && animations.Count > 0)
            {
                currentOneShot = oneShot;
                currentBackwards = backwards;

                if(!currentOneShot)
                {
                    currentLoopType = loopType;
                    if (delayBetweenLoops)
                    {
                        waitingLoop = true;
                        if (maxDelayBetweenLoops.Equals(minDelayBetweenLoops))
                            loopTimer = minDelayBetweenLoops;
                        else
                            loopTimer = Random.Range(minDelayBetweenLoops, maxDelayBetweenLoops);
                    }
                    else
                    {
                        waitingLoop = false;
                        loopTimer = 0;
                    }
                }

                if (startAnimation.Contains("null") || startAnimation.Equals(""))
                    startAnimation = animations[0].Name;

                // Pick the selected animation or a random one.
                if (!randomAnimation)
                    Play(startAnimation, currentOneShot, currentBackwards, currentLoopType);
                else
                    PlayRandom(currentOneShot, currentBackwards, currentLoopType);
            }
        }

        /// <summary>
        /// Manually initialize the animator. Useful and NECESSARY if the animator was instanced on runtime.
        /// </summary>
        /// <param name="playOnAwake">Will play after this init?</param>
        /// <param name="animations">Animation list</param>
        /// <param name="startAnimation">Start animation to play if play on awake is true</param>
        public void Initialize(bool playOnAwake, List<SpriteAnimation> animations, string startAnimation)
        {
            this.animations = new List<SpriteAnimation>(animations);
            this.playOnAwake = playOnAwake;
            this.startAnimation = startAnimation;

            Awake();
        }

        private void Update()
        {
            // We do nothing if the current FPS <= 0
            if (currentAnimation == null || currentFramerate <= 0 || !playing)
                return;

            // Add the delta time to the timer and the total time
            if (!ignoreTimeScale)
            {
                animationTimer += Time.deltaTime;
                currentAnimationTime = (!currentBackwards) ? currentAnimationTime + Time.deltaTime : currentAnimationTime - Time.deltaTime;
            }
            else
            {
                animationTimer += Time.unscaledDeltaTime;
                currentAnimationTime = (!currentBackwards) ? currentAnimationTime + Time.unscaledDeltaTime : currentAnimationTime - Time.unscaledDeltaTime;
            }

            if (!waitingLoop && animationTimer >= timePerFrame)
            {
                // Check frames duration
                frameDurationCounter++;
                animationTimer -= timePerFrame;

                // Double check animation frame index 
                if (CheckLastFrame())
                    Restart();

                if (frameDurationCounter >= currentAnimation.FramesDuration[frameIndex])
                {
                    // Change frame only if have passed the desired frames
                    frameIndex = (currentBackwards) ? frameIndex - 1 : frameIndex + 1;
                    frameDurationCounter = 0;

                    // Check last or first frame
                    if (CheckLastFrame())
                    {
                        // Last frame, reset index and stop if is one shot
                        onFinish.Invoke();

                        if (currentOneShot)
                        {
                            Stop();
                            return;
                        }
                        else
                        {
                            if (!waitingLoop)
                            {
                                waitingLoop = true;
                                loopTimer = 0;

                                // Check delay between loops
                                if (delayBetweenLoops)
                                {
                                    SetActiveRenderer(!disableRendererOnFinish);
                                    if (maxDelayBetweenLoops > 0)
                                        loopTimer = Random.Range(minDelayBetweenLoops, maxDelayBetweenLoops);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Change sprite
                        ChangeFrame(currentAnimation.GetFrame(frameIndex));

                        if(frameIndex == stopAtFrame)
                        {
                            Stop();
                        }
                    }

                    // Check events
                    if(customEvents != null)
                    {
                        SpriteAnimatorEventInfo frameInfo = new SpriteAnimatorEventInfo(currentAnimation, frameIndex);
                        if (customEvents.ContainsKey(frameInfo))
                            customEvents[frameInfo].Invoke(this);
                    }
                }
            }

            if (waitingLoop)
            {
                // Continue looping if enought time have passed
                loopTimer -= Time.deltaTime;
                if (loopTimer <= 0)
                {
                    if (currentLoopType == LoopType.yoyo)
                        currentBackwards = !currentBackwards;

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
                        SetActiveRenderer(true);
                        animationTimer = 0;
                        currentAnimationTime = (currentBackwards) ? currentAnimation.AnimationDuration * timePerFrame : 0;
                        frameIndex = (currentBackwards) ? framesInAnimation - 1 : 0;
                        ChangeFrame(currentAnimation.GetFrame(frameIndex));
                    }
                }
            }
        }

        public void Play(SpriteAnimation animation, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            SetActiveRenderer(true);

            currentOneShot = playOneShot;
            currentBackwards = playBackwards;
            currentLoopType = loopType;

            // If it's the same animation but not playing, reset it, if playing, do nothing.
            if (currentAnimation != null && currentAnimation.Equals(animation))
            {
                if (!playing)
                {
                    Restart();
                    Resume();
                }
                else
                    return;
            }
            // If the animation is new, save it as current animation and play it
            else
            {
                currentAnimation = animation;
            }

            StartPlay();
        }

        /// <summary>
        /// Plays the first animation of the animation list.
        /// </summary>
        public void Play(bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            Play(animations[0].Name, playOneShot, playBackwards, loopType);
        }

        /// <summary>
        /// Plays an animation.
        /// </summary>
        public void Play(string animation, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            SetActiveRenderer(true);

            currentOneShot = playOneShot;
            currentBackwards = playBackwards;
            currentLoopType = loopType;

            // If it's the same animation but not playing, reset it, if playing, do nothing.
            if (currentAnimation != null && currentAnimation.Name.Equals(animation))
            {
                if (!playing)
                {
                    Restart();
                    Resume();
                }
                else
                    return;
            }
            // Look for the animation only if its new or current animation is null
            else if (currentAnimation == null || !currentAnimation.Name.Equals(animation))
                currentAnimation = GetAnimation(animation);

            StartPlay();
        }

        private void StartPlay()
        {
            // If we have an animation to play, flag as playing, reset timer and take frame count
            if (currentAnimation != null)
            {
                if (!useAnimatorFramerate)
                    currentFramerate = currentAnimation.FPS;
                timePerFrame = 1f / currentFramerate;
                framesInAnimation = currentAnimation.FramesCount;
                currentAnimationTime = (currentBackwards) ? currentAnimation.AnimationDuration * timePerFrame : 0;

                // Check if the animation have frames. Show warning if not.
                if (framesInAnimation == 0)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning("Animation '" + name + "' has no frames.", gameObject);
#endif
                    playing = false;
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
                        Debug.LogWarning("Starting frame out of bounds.", gameObject);
#endif
                        frameIndex = 0;
                    }
                    startingFrame = -1;
                }

                onPlay.Invoke();
                playing = true;

                if (!waitingLoop)
                    ChangeFrame(currentAnimation.GetFrame(frameIndex));
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
                Debug.LogError("Animation '" + name + "' not found.", gameObject);
#endif
        }

        /// <summary>
        /// Plays a random animation of the animation list.
        /// </summary>
        public void PlayRandom(bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            // Get a random animation and plays it
            int animIndex = Random.Range(0, animations.Count);
            Play(animations[animIndex].Name, playOneShot, playBackwards, loopType);
        }

        /// <summary>
        /// Plays the first animation of the animation list starting at the specified frame. 
        /// </summary>
        public void PlayStartingAtFrame(int frame, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            startingFrame = frame;
            Play(playOneShot, playBackwards, loopType);
        }

        /// <summary>
        /// Plays an animation starting at the specified frame.
        /// </summary>
        public void PlayStartingAtFrame(string animation, int frame, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            startingFrame = frame;
            Play(animation, playOneShot, playBackwards, loopType);
        }

        /// <summary>
        /// Plays an animation starting at the specified frame.
        /// </summary>
        public void PlayStartingAtFrame(SpriteAnimation animation, int frame, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            startingFrame = frame;
            Play(animation, playOneShot, playBackwards, loopType);
        }

        /// <summary>
        /// Plays the first animation of the animation list starting at the specified time (in seconds). 
        /// </summary>
        public void PlayStartingAtTime(float time, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            Play(playOneShot, playBackwards, loopType);
            SetAnimationTime(time);
        }

        /// <summary>
        /// Plays an animation starting at the specified time (in seconds).
        /// </summary>
        public void PlayStartingAtTime(string animation, float time, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            Play(animation, playOneShot, playBackwards, loopType);
            SetAnimationTime(time);
        }

        /// <summary>
        /// Plays an animation starting at the specified time (in seconds).
        /// </summary>
        public void PlayStartingAtFrame(SpriteAnimation animation, float time, bool playOneShot = false, bool playBackwards = false, LoopType loopType = LoopType.repeat)
        {
            Play(animation, playOneShot, playBackwards, loopType);
            SetAnimationTime(time);
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
        /// Stops when reaches the desired frame. If the desired frame has already passed and the animation is not looped it will stop at the end of the animation anyway.
        /// </summary>
        public void StopAtFrame(int frame)
        {
            stopAtFrame = frame;
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            stopAtFrame = -1;
            randomStartFrameApplied = false;
            playing = false;
            onStop.Invoke();
            SetActiveRenderer(!disableRendererOnFinish);

            if (fallbackAnimation != null)
            {
                Play(fallbackAnimation.Name, false, false, fallbackLoopType);
            }
        }

        /// <summary>
        /// Sets the fallback animation to play and its loop type
        /// </summary>
        public void SetFallbackAnimation(string animation, LoopType loopType)
        {
            fallbackAnimation = GetAnimation(animation);
            fallbackLoopType = loopType;
        }

        /// <summary>
        /// Sets the animator FPS overriding the FPS of the animation.
        /// </summary>
        public void UseAnimatorFPS(int frameRate)
        {
            currentFramerate = frameRate;
            timePerFrame = 1f / currentFramerate;
            useAnimatorFramerate = true;
        }

        /// <summary>
        /// Sets de animator FPS to the current animation FPS.
        /// </summary>
        public void UseAnimationFPS()
        {
            if (currentAnimation != null)
            {
                currentFramerate = currentAnimation.FPS;
                timePerFrame = 1f / currentFramerate;
                useAnimatorFramerate = false;
            }
        }

        /// <summary>
        /// Removes the fallback animation
        /// </summary>
        public void RemoveFallbackAnimation()
        {
            fallbackAnimation = null;
        }

        /// <summary>
        /// Restarts the animation. If the animation is not playing the effects will apply when starts playing.
        /// </summary>
        public void Restart()
        {
            animationTimer = 0;
            frameIndex = (currentBackwards) ? framesInAnimation - 1 : 0;
            frameDurationCounter = 0;
            ChangeFrame(currentAnimation.GetFrame(frameIndex));
        }

        /// <summary>
        /// Adds a custom event to the first animation on the list on a certain frame.
        /// </summary>
        /// <returns>
        /// The event created. Null if the animation list is empty or doesn't have enough frames.
        /// </returns>  
        public SpriteAnimatorEvent AddCustomEvent(int frame)
        {
            if (animations.Count == 0)
                return null;
            return AddCustomEvent(animations[0], frame);
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
            return AddCustomEvent(anim, frame);
        }

        /// <summary>
        /// Adds a custom event to specified animation on a certain frame.
        /// </summary>
        /// <returns>
        /// The event created. Null if the animation is null or doesn't have enough frames.
        /// </returns>  
        public SpriteAnimatorEvent AddCustomEvent(SpriteAnimation animation, int frame)
        {
            if (animation == null || animation.FramesCount <= frame)
                return null;

            SpriteAnimatorEventInfo eventInfo = new SpriteAnimatorEventInfo(animation, frame);

            if (customEvents == null)
                customEvents = new Dictionary<SpriteAnimatorEventInfo, SpriteAnimatorEvent>();

            if (!customEvents.ContainsKey(eventInfo))
                customEvents.Add(eventInfo, new SpriteAnimatorEvent());

            return customEvents[eventInfo];
        }

        /// <summary>
        /// Adds a custom event to specified animation on the list on the last frame.
        /// </summary>
        /// <returns>
        /// The event created. Null if the animation is not found or doesn't have enough frames.
        /// </returns>  
        public SpriteAnimatorEvent AddCustomEventAtEnd()
        {
            if (animations.Count == 0)
                return null;
            return AddCustomEvent(animations[0], animations[0].FramesCount - 1);
        }

        /// <summary>
        /// Adds a custom event to specified animation on the list on the last frame.
        /// </summary>
        /// <returns>
        /// The event created. Null if the animation is not found.
        /// </returns>  
        public SpriteAnimatorEvent AddCustomEventAtEnd(string animation)
        {
            SpriteAnimation anim = GetAnimation(animation);
            return AddCustomEvent(animation, anim.FramesCount - 1);
        }

        /// <summary>
        /// Adds a custom event to specified animation on the last frame.
        /// </summary>
        /// <returns>
        /// The event created. Null if the animation is null.
        /// </returns>  
        public SpriteAnimatorEvent AddCustomEventAtEnd(SpriteAnimation animation)
        {
            return AddCustomEvent(animation, animation.FramesCount - 1);
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
            SpriteAnimation anim = GetAnimation(animation);
            return GetCustomEvent(anim, frame);
        }

        /// <summary>
        /// Gets the custom event of an animation on a certain frame.
        /// </summary>
        /// <returns>
        /// The event of the specified animation on the selected frame. Null if not found.
        /// </returns>  
        public SpriteAnimatorEvent GetCustomEvent(SpriteAnimation animation, int frame)
        {
            if (animation == null || animation.FramesCount <= frame)
                return null;

            SpriteAnimatorEventInfo eventInfo = new SpriteAnimatorEventInfo(animation, frame);
            if (customEvents.ContainsKey(eventInfo))
                return customEvents[eventInfo];
            else
                return null;
        }

        /// <summary>
        /// Gets the custom event of the first animation of the animation list on the last frame.
        /// </summary>
        /// <returns>
        /// The event of the first animation of the animation list on the last frame. Null if not found.
        /// </returns> 
        public SpriteAnimatorEvent GetCustomEventAtEnd()
        {
            if (animations.Count == 0)
                return null;
            return GetCustomEvent(animations[0], animations[0].FramesCount - 1);
        }

        /// <summary>
        /// Gets the custom event of an animation on the last frame.
        /// </summary>
        /// <returns>
        /// The event of the specified animation on the last frame. Null if not found.
        /// </returns>  
        public SpriteAnimatorEvent GetCustomEventAtEnd(string animation)
        {
            SpriteAnimation anim = GetAnimation(animation);
            return GetCustomEvent(anim, anim.FramesCount - 1);
        }

        /// <summary>
        /// Gets the custom event of an animation on the last frame.
        /// </summary>
        /// <returns>
        /// The event of the specified animation on the last frame. Null if not found.
        /// </returns>  
        public SpriteAnimatorEvent GetCustomEventAtEnd(SpriteAnimation animation)
        {
            if (animation == null)
                return null;
            return GetCustomEvent(animation, animation.FramesCount - 1);
        }

        /// <summary>
        /// Search an animation with the given name.
        /// </summary>
        /// <returns>
        /// The animation. Null if not found.
        /// </returns>  
        private SpriteAnimation GetAnimation(string animationName)
        {
            return animations.Find(x => x.Name.Equals(animationName));
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

        /// <summary>
        /// Flip the sprite on the Y axis
        /// </summary>
        public virtual void FlipSpriteY(bool flip) { }

        /// <summary>
        /// Sets a random delay between loops
        /// </summary>
        public void SetRandomDelayBetweenLoops(float min, float max)
        {
            delayBetweenLoops = true;
            minDelayBetweenLoops = min;
            maxDelayBetweenLoops = max;
        }

        /// <summary>
        /// Sets a delay between loops
        /// </summary>
        public void SetDelayBetweenLoops(float delay)
        {
            delayBetweenLoops = true;
            minDelayBetweenLoops = delay;
            maxDelayBetweenLoops = delay;
        }

        /// <summary>
        /// Sets the animation time to the specified time, updating de sprite to the correspondent frame at that time.
        /// </summary>
        /// <param name="time">Time in seconds</param>
        public void SetAnimationTime(float time)
        {
            if (currentAnimation != null)
            {
                currentAnimationTime = time;

                float timePerFrame = 1f / currentFramerate;
                float totalAnimationTime = currentAnimation.AnimationDuration * timePerFrame;

                if (time >= totalAnimationTime)
                {
                    animationTimer = timePerFrame;
                    frameIndex = framesInAnimation - 1;
                    frameDurationCounter = currentAnimation.FramesDuration[frameIndex] - 1;
                }
                else if (time == 0)
                {
                    animationTimer = 0;
                    frameIndex = 0;
                    frameDurationCounter = 0;
                }
                else
                {
                    frameIndex = 0;
                    frameDurationCounter = 0;

                    while (time >= timePerFrame)
                    {
                        time -= timePerFrame;
                        frameDurationCounter++;

                        if (frameDurationCounter >= currentAnimation.FramesDuration[frameIndex])
                        {
                            frameIndex++;
                            frameDurationCounter = 0;
                        }
                    }

                    animationTimer = time;
                }

                ChangeFrame(currentAnimation.GetFrame(frameIndex));
            }
        }

        /// <summary>
        /// Check the last frame (backwards or not)
        /// </summary>
        private bool CheckLastFrame()
        {
            return (!currentBackwards && frameIndex > framesInAnimation - 1) || (currentBackwards && frameIndex < 0);
        }
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
            set { disableRendererOnFinish = value; }
        }

        /// <summary>
        /// If true the animator will get a random animation after every loop cicle
        /// </summary>
        public bool RandomAnimation
        {
            set { randomAnimation = value; }
        }

        /// <summary>
        /// If true a delay will be made between loops
        /// </summary>
        public bool DelayBetweenLoops
        {
            set { delayBetweenLoops = value; }
        }

        /// <summary>
        /// If true, the timescale of the game will be ignored
        /// </summary>
        public bool IgnoreTimeScale
        {
            set { ignoreTimeScale = value; }
        }

        /// <summary>
        /// The current frame of the animation.
        /// </summary>
        public int CurrentFrame
        {
            get { return frameIndex; }
        }

        /// <summary>
        /// The current FPS of the animator (it could be the animation FPS or an overrided FPS)
        /// </summary>
        public int CurrentFrameRate
        {
            get { return currentFramerate; }
        }

        /// <summary>
        /// The current time in seconds of the playing animation
        /// </summary>
        public float CurrentAnimationTime
        {
            get { return currentAnimationTime; }
        }

        /// <summary>
        /// The currently playing animation name.
        /// </summary>
        public string CurrentAnimation
        {
            get { return (currentAnimation != null) ? currentAnimation.Name : string.Empty; }
        }

        /// <summary>
        /// The currently playing animation.
        /// </summary>
        public SpriteAnimation PlayingAnimation
        {
            get { return currentAnimation; }
        }

        /// <summary>
        /// The animation will start at a random frame if this is true
        /// </summary>
        public bool StartAtRandomFrame
        {
            set { startAtRandomFrame = value; }
        }

        public SpriteAnimation FallbackAnimation
        {
            get { return fallbackAnimation; }
            set { fallbackAnimation = value; }
        }

        /// <summary>
        /// The animation to play with Play On Awake active and Random Animation disabled
        /// </summary>
        public string StartAnimation
        {
            get { return startAnimation; }
            set { startAnimation = value; }
        }
        #endregion
    }
}