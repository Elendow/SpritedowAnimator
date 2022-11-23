// Spritedow Animation Plugin by Elendow
// https://elendow.com

using UnityEngine;

namespace Elendow.SpritedowAnimator.Examples
{
    public class PlayerPlatformer : MonoBehaviour
    {
        public SpriteAnimation duckAnimation;
		public SpriteAnimation standAnimation;
		public SpriteAnimation walkAnimation;

		private Rigidbody2D rigidBody;

        public SpriteAnimation jumpAnimation;
        private SpriteAnimator spriteAnimator;

        private void Awake()
        {
            spriteAnimator = GetComponent<SpriteAnimator>();
            spriteAnimator.AddCustomEvent(jumpAnimation, 5).AddListener(JumpEvent);
        }

        private void JumpEvent(BaseAnimator animator)
        {
            // Do something
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                rigidBody.AddForce(Vector2.left * 50);
                spriteAnimator.FlipSpriteX(true);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                rigidBody.AddForce(Vector2.right * 50);
                spriteAnimator.FlipSpriteX(false);
            }
            else if(Input.GetKey(KeyCode.DownArrow) && rigidBody.velocity.y == 0)
            {
                spriteAnimator.Play(duckAnimation);
            }
            else
            {
                spriteAnimator.Play(standAnimation);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rigidBody.AddForce(Vector2.up * 500);
            }

            if (!spriteAnimator.PlayingAnimation.Equals(duckAnimation))
            {
                if (rigidBody.velocity.y != 0)
                {
                    spriteAnimator.Play(jumpAnimation);
                }
                else if (rigidBody.velocity.x != 0)
                {
                    spriteAnimator.Play(walkAnimation);
                }
            }
        }
    }
}