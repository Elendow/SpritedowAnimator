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
    protected int animationIndex;
    protected int framesInAnimation;
    protected int frameDurationCounter;
    protected float animationTimer;
    protected SpriteAnimation currentAnimation;

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
                animationTimer = 0;

                if (frameDurationCounter >= currentAnimation.FramesDuration[animationIndex])
                {
                    // Change frame only if have passed the desired frames
                    animationIndex += 1;
                    frameDurationCounter = 0;
                }

                if (animationIndex >= framesInAnimation)
                {
                    // Last frame, reset index and stop if is one shot
                    animationIndex = 0;
                    onFinish.Invoke();
                    if (oneShot) Stop();
                }
            }
        }
    }

    public void Play(string name, bool playOneShot = false)
    {
        oneShot = playOneShot;

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
    }

    [ContextMenu("Reset Animation")]
    public void Reset()
    {
        //Reset all the animation counters
        animationTimer = 0;
        animationIndex = 0;
        frameDurationCounter = 0;
    }

    public bool IsPlaying
    {
        get { return playing; }
    }

    public string CurrentAnimation
    {
        get { return currentAnimation.Name; }
    }
}