// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using Elendow.SpritedowAnimator;

namespace Elendow.SpritedowAnimator.Examples
{
    public class EventExample : MonoBehaviour
    {
        private SpriteAnimator spriteAnimator;
        private ParticleSystem particles;

        private void Awake()
        {
            particles = GetComponentInChildren<ParticleSystem>();

            // Here we add an event to the 9th frame of the animation Slimedow
            // After that, we subscribe the method "BurstParticles"
            // We can also get the event with spriteAnimator.GetCustomEvent("Slimedow", 9) and suscribe whatever we want
            spriteAnimator = GetComponent<SpriteAnimator>();
            spriteAnimator.AddCustomEvent("Slimedow", 9).AddListener(BurstParticles);

            // Then we simply play the animation and the magic begins
            spriteAnimator.Play(false);
        }

        private void BurstParticles(BaseAnimator caller)
        {
            particles.Play();
        }
    }
}