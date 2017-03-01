// Spritedow Animation Plugin by Elendow
// http://elendow.com

using System;
using UnityEngine;

[Serializable]
public class AnimationFrame
{
	private int duration;
	private Sprite frame;

	public AnimationFrame(){}
	public AnimationFrame(Sprite s, int d)
	{
		duration = d;
		frame = s;
	}

	public int Duration
	{
		get { return duration; }
		set { duration = value; }
	}

	public Sprite Frame
	{
		get { return frame; }
		set { frame = value; }
	}
}
