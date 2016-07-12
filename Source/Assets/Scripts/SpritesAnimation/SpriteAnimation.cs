using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class SpriteAnimation : ScriptableObject
{
	[SerializeField]
	private string animationName;
	[SerializeField]
	private List<Sprite> frames;

	public SpriteAnimation()
	{
		frames = new List<Sprite>();
	}

	public Sprite GetFrame(int index)
	{
		return frames[index];
	}

	public string Name
	{
		get { return animationName; }
		set { animationName = value; }
	}

	public int FramesCount
	{
		get { return frames.Count; }
	}

	public List<Sprite> Frames
	{
		get { return frames; }
		set { frames = value; }
	}
}