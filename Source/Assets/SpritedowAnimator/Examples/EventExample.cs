// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using Elendow.SpritedowAnimator;

public class EventExample : MonoBehaviour
{
    private SpriteAnimator spriteAnimator;
    private ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        spriteAnimator = GetComponent<SpriteAnimator>();
        spriteAnimator.AddCustomEvent("Slimedow", 9).AddListener(BurstParticles);

        spriteAnimator.Play(false);
    }

    private void BurstParticles(BaseAnimator caller)
    {
        particles.Play();
    }
}
