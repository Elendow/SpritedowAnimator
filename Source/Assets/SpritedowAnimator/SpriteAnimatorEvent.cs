// Spritedow Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/SpritedowAnimator
using UnityEngine.Events;

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// UnityEvent with BaseAnimator as argument.
    /// </summary>
    [System.Serializable]
    public class SpriteAnimatorEvent : UnityEvent<BaseAnimator> { }
}