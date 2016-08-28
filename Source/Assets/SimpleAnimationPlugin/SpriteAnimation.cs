// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using System.Collections.Generic;

public class SpriteAnimation : ScriptableObject
{
    [SerializeField]
    private string _animationName;
    [SerializeField]
    private List<Sprite> _frames;
    [SerializeField]
    private List<int> _framesDuration;

    public SpriteAnimation()
    {
        _frames = new List<Sprite>();
        _framesDuration = new List<int>();
    }

    public Sprite GetFrame(int index)
    {
	    return _frames[index];
    }

    public int GetFrameDuration(int index)
    {
        return _framesDuration[index];
    }

    public string Name
    {
	    get { return _animationName; }
	    set { _animationName = value; }
    }

    public int FramesCount
    {
	    get { return _frames.Count; }
    }

    public List<Sprite> Frames
    {
	    get { return _frames; }
	    set { _frames = value; }
    }

    public List<int> FramesDuration
    {
        get { return _framesDuration; }
        set { _framesDuration = value; }
    }
}