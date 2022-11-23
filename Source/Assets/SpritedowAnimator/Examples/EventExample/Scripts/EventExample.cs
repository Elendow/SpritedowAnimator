// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;

namespace Elendow.SpritedowAnimator.Examples
{
    public class EventExample : MonoBehaviour
    {
        #region Attributes
        public SpriteAnimation anim;

        private SpriteAnimator spriteAnimator;
        private ParticleSystem particles;
        #endregion

        private void Awake()
        {
            particles = GetComponentInChildren<ParticleSystem>();

            // Here we add an event to the 9th frame of the animation Slimedow
            // After that, we subscribe the method "BurstParticles"
            // We can also get the event with spriteAnimator.GetCustomEvent("Slimedow", 9) and suscribe whatever we want
            spriteAnimator = GetComponent<SpriteAnimator>();
            spriteAnimator.AddCustomEvent(anim, 9).AddListener(BurstParticles);

            // Then we simply play the animation and the magic begins
            spriteAnimator.Play(anim, false);
        }

        private void BurstParticles(BaseAnimator caller)
        {
            particles.Play();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                spriteAnimator.Play(anim, true);
                Debug.Break();
            }
        }
    }
}