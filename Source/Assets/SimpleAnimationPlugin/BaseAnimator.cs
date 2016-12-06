// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class BaseAnimator : MonoBehaviour
{
    public bool playOnAwake = false;
    public int framesPerSecond = 30;
    public string startAnimation;
    public List<SpriteAnimation> animations;

    public UnityEvent onFinish;
    public UnityEvent onStop;
    public UnityEvent onPlay;

    protected bool playing;
    protected bool oneShot;
    protected bool backwards;
    protected bool disableRenderer;
    protected int animationIndex;
    protected int framesInAnimation;
    protected int frameDurationCounter;
    protected float animationTimer;
    protected SpriteAnimation currentAnimation;
    protected Dictionary<SpriteAnimatorEventInfo, SpriteAnimatorEvent> customEvents;

    protected virtual void Awake()
    {
        // Why an animator without animation?
        if (animations.Count == 0)
        {
            Debug.LogError("Sprite animator without animations.", gameObject);
            return;
        }

        // Play the first animation if play on awake
        if (playOnAwake) Play(startAnimation);       
    }

    protected virtual void ChangeFrame(Sprite frame) { }

    private void Update()
    {
        // We do nothing if FPS <= 0
        if (framesPerSecond <= 0) return;

        if (playing)
        {
            animationTimer += Time.deltaTime;

            if (1f / framesPerSecond < animationTimer)
            {
                // Next Frame!
                frameDurationCounter++;
                ChangeFrame(currentAnimation.GetFrame(animationIndex));
                SpriteAnimatorEventInfo frameInfo = new SpriteAnimatorEventInfo(currentAnimation.Name, animationIndex);
                if (customEvents != null && customEvents.ContainsKey(frameInfo))
                    customEvents[frameInfo].Invoke(this);
                animationTimer = 0;

                if (frameDurationCounter >= currentAnimation.FramesDuration[animationIndex])
                {
                    // Change frame only if have passed the desired frames
                    animationIndex = (backwards) ? animationIndex - 1 : animationIndex + 1;
                    frameDurationCounter = 0;
                }

                if (animationIndex >= framesInAnimation)
                {
                    // Last frame, reset index and stop if is one shot
                    animationIndex = (backwards) ? framesInAnimation - 1 : 0;
                    onFinish.Invoke();
                    if (oneShot) Stop();
                }
            }
        }
    }

    public void Play(string name, bool playOneShot = false, bool playBackwards = false)
    {
        SetActiveRenderer(true);

        if (name == "")
            name = animations[0].Name;

        oneShot = playOneShot;
        backwards = playBackwards;

        // If it's the same animation but not playing, reset it, if playing, do nothing.
        if (currentAnimation != null && currentAnimation.Name == name)
        {
            if (!playing)
            {
                Reset();
                Resume();
            }
            return;
        }
        else if (currentAnimation != null && currentAnimation.Name == name && playing)
            return;
        // Look for the animation only if its new or current animation is null
        else if (currentAnimation == null || currentAnimation.Name != name)
            currentAnimation = animations.Find(x => x.Name.Contains(name));

        // If we have an animation to play, flag as playing, reset timer and take frame count
        if (currentAnimation != null)
        {
            onPlay.Invoke();
            Reset();
            playing = true;
            framesInAnimation = currentAnimation.FramesCount;
        }
        else
            Debug.LogError("Animation '" + name + "' not found.", gameObject);
    }

    public void PlayRandom(bool playOneShot = false, bool playBackwards = false)
    {
        int animIndex = Random.Range(0, animations.Count);
        Play(animations[animIndex].Name, playOneShot, playBackwards);
    }

    [ContextMenu("Resume Animation")]
    public void Resume()
    {
        // Resume the animation, this is the same as calling Play with the same name, but more simplificated
        if (currentAnimation != null)
            playing = true;
    }

    [ContextMenu("Stop Animation")]
    public void Stop()
    {
        // Stop the animation
        playing = false;
        onStop.Invoke();
        SetActiveRenderer(!disableRenderer);
    }

    [ContextMenu("Reset Animation")]
    public void Reset()
    {
        //Reset all the animation counters
        animationTimer = 0;
        animationIndex = 0;
        frameDurationCounter = 0;
    }

    public virtual void SetActiveRenderer(bool active) { }

    public virtual void FlipSpriteX(bool flip) { }

    public bool IsPlaying
    {
        get { return playing; }
    }

    public bool DisableRenderOnFinish
    {
        set { disableRenderer = value; }
    }

    public string CurrentAnimation
    {
        get { return currentAnimation.Name; }
    }

    public SpriteAnimatorEvent AddCustomEvent(string animation, int frame)
    {
        if (animation == "") animation = animations[0].Name;
        SpriteAnimatorEventInfo eventInfo = new SpriteAnimatorEventInfo(animation, frame);
        if (customEvents == null)
            customEvents = new Dictionary<SpriteAnimatorEventInfo, SpriteAnimatorEvent>();

        if (!customEvents.ContainsKey(eventInfo))
            customEvents.Add(eventInfo, new SpriteAnimatorEvent());

        return customEvents[eventInfo];
    }

    public SpriteAnimatorEvent GetCustomEvent(string animation, int frame)
    {
        if (animation == "") animation = animations[0].Name;
        SpriteAnimatorEventInfo eventInfo = new SpriteAnimatorEventInfo(animation, frame);

        if (customEvents.ContainsKey(eventInfo))
            return customEvents[eventInfo];
        else
            return null;
    }
}

public struct SpriteAnimatorEventInfo
{
    public string animation;
    public int frame;

    public SpriteAnimatorEventInfo(string animation, int frame)
    {
        this.animation = animation;
        this.frame = frame;
    }
}

[System.Serializable]
public class SpriteAnimatorEvent : UnityEvent<BaseAnimator>{}