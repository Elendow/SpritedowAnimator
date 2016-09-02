// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    public bool playOnAwake = false;
    public int framesPerSecond = 30;
    public string startAnimation;
    public List<SpriteAnimation> animations;

    public UnityEvent onFinish;
    public UnityEvent onStop;
    public UnityEvent onPlay;

    private bool _playing;
    private bool _oneShot;
    private bool _backwards;
    private int _animationIndex;
    private int _framesInAnimation;
    private int _frameDurationCounter;
    private float _animationTimer;
    private SpriteAnimation _currentAnimation;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Why an animator without animation?
        if (animations.Count == 0)
        {
            Debug.LogError("Sprite animator without animations.", gameObject);
            return;
        }

        // Play the start animation if play on awake
        if (playOnAwake)
            Play(startAnimation);
    }

    private void Update()
    {
        // We do nothing if FPS <= 0
        if (framesPerSecond <= 0)
            return;

        if (_playing)
        {
            _animationTimer += Time.deltaTime;

            if (1f / framesPerSecond < _animationTimer)
            {
                // Next Frame!
                _frameDurationCounter++;
                _spriteRenderer.sprite = _currentAnimation.GetFrame(_animationIndex);
                _animationTimer = 0;

                if (_frameDurationCounter >= _currentAnimation.FramesDuration[_animationIndex])
                {
                    // Change frame only if have passed the desired frames
                    _animationIndex = (_backwards) ? _animationIndex - 1 : _animationIndex + 1;
                    _frameDurationCounter = 0;
                }

                if (_animationIndex >= _framesInAnimation || _animationIndex < 0)
                {
                    // Last frame, reset index and stop if is one shot
                    _animationIndex = (_backwards) ? _framesInAnimation - 1 : 0;
                    onFinish.Invoke();
                    if (_oneShot)
                        Stop();
                }
            }
        }
    }

    public void Play(string name, bool oneShot = false, bool backwards = false)
    {
        _oneShot = oneShot;
        _backwards = backwards;

        // If it's the same animation but not playing, reset it, if playing, do nothing.
        if (_currentAnimation != null && _currentAnimation.Name == name)
        {
            if (!_playing)
            {
                Reset();
                Resume();
            }
            return;
        }
       
        // Look for the animation only if its new or current animation is null
        if (_currentAnimation == null || _currentAnimation.Name != name)
            _currentAnimation = animations.Find(x => x.Name.Contains(name));

       // Animation not found
        if (_currentAnimation == null)
            Debug.LogError("Animation '" + name + "' not found.", gameObject);

        // If we have an animation to play, flag as playing, reset timer and take frame count
        onPlay.Invoke();
        Reset();
        _playing = true;
        _framesInAnimation = _currentAnimation.FramesCount;
    }

    [ContextMenu("Resume Animation")]
    public void Resume()
    {
        // Resume the animation, this is the same as calling Play with the same name, but more simplificated
        if (_currentAnimation != null)
            _playing = true;
    }

    [ContextMenu("Stop Animation")]
    public void Stop()
    {
        // Stop the animation
        _playing = false;
        onStop.Invoke();
    }

    [ContextMenu("Reset Animation")]
    public void Reset()
    {
        //Reset all the animation counters
        _animationTimer = 0;
        _animationIndex = 0;
        _frameDurationCounter = 0;
    }

    public bool IsPlaying
    {
        get { return _playing; }
    }

    public string CurrentAnimation
    {
        get { return _currentAnimation.Name; }
    }
}