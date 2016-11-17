// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using System.Collections.Generic;

public class SpriteAnimation : ScriptableObject
{
    [SerializeField]
    private string animationName;
    [SerializeField]
    private List<Sprite> frames;
    [SerializeField]
    private List<int> framesDuration;

    public SpriteAnimation()
    {
        frames = new List<Sprite>();
        framesDuration = new List<int>();
    }

    public Sprite GetFrame(int index)
    {
        return frames[index];
    }

    public int GetFrameDuration(int index)
    {
        return framesDuration[index];
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

    public List<int> FramesDuration
    {
        get { return framesDuration; }
        set { framesDuration = value; }
    }
}