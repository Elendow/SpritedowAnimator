using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elendow.SpritedowAnimator.Examples
{
    public class PlayerPlatformer : MonoBehaviour
    {
        private SpriteAnimator spriteAnimator;

        private void Awake()
        {
            spriteAnimator = GetComponent<SpriteAnimator>();
        }
    }
}