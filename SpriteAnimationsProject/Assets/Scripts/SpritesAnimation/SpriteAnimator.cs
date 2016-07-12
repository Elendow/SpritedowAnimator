using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour {

	public bool playOnAwake = false;
	public int framesPerSecond = 30;
	public List<SpriteAnimation> animations;

	private bool _playing;
	private bool _oneShot;
	private int _animationIndex;
	private int _framesInAnimation;
	private float _animationTimer;
	private SpriteAnimation _currentAnimation;
	private SpriteRenderer _spriteRenderer;

	public void Awake()
	{
		_spriteRenderer = GetComponent<SpriteRenderer>();

		//Why an animator without animation?
		if(animations.Count == 0)
		{
			Debug.LogError("Sprite animator without animations.", gameObject);
			return;
		}

		//Play the first animation if play on awake
		//TODO choose the play on awake animation
		if(playOnAwake)
			Play(animations[0].Name);

		_currentAnimation = animations[0];
	}

	public void Update()
	{
		//We do nothing if FPS = 0
		if(framesPerSecond == 0)
			return;
		
		if(_playing)
		{
			_animationTimer += Time.deltaTime;

			if(1f / framesPerSecond < _animationTimer)
			{
				_animationTimer			= 0;
				_spriteRenderer.sprite 	= _currentAnimation.GetFrame(_animationIndex);
				_animationIndex        += 1;
				if(_animationIndex >= _framesInAnimation)
				{
					_animationIndex = 0;
					if(_oneShot)
						Stop();
				}
			}
		}
	}

	public void PlayOneShot(string name)
	{
		//Play the animation only one time
		_oneShot = true;
		Play(name);
	}

	public void Play(string name)
	{
		if(_currentAnimation != null)
		{
			//Do nothing if this animation is already playing
			if(_currentAnimation.Name == name && _playing)
				return;
			//Look for the animation only if its new
			else if(_currentAnimation.Name != name)
				_currentAnimation = animations.Find(x => x.Name.Contains(name));
		}

		//If we have an animation to play, flag as playing, reset timer and take frame count
		if(_currentAnimation != null)
		{
			//We don't reset the animation index because if we do we can't resume the animation
			//The animation index will reset alone if it's necessary
			_animationTimer 	= 0;
			_playing 			= true;
			_framesInAnimation 	= _currentAnimation.FramesCount;
		}
		else
			Debug.LogError("Animation '" + name + "' not found.", gameObject);
	}

	public void Stop()
	{
		//Stop the animation
		_playing = false;
	}

	public void Reset()
	{
		//Reset all the animation counters
		_animationTimer 	= 0;
		_animationIndex		= 0;
		_playing 			= true;
	}
}