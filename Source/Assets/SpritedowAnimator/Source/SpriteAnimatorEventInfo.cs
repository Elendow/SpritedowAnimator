// Spritedow Animation Plugin by Elendow
// http://elendow.com

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// Struct used on the events dictionary to store animation and frame.
    /// </summary>
    public struct SpriteAnimatorEventInfo
    {
        public SpriteAnimation animation;
        public int frame;

        public SpriteAnimatorEventInfo(SpriteAnimation animation, int frame)
        {
            this.animation = animation;
            this.frame = frame;
        }
    }
}