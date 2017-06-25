using UnityEngine;

namespace Elendow.SpritedowAnimator.Examples
{
    public class PlayerPlatformer : MonoBehaviour
    {
        private SpriteAnimator spriteAnimator;
        private Rigidbody2D rigidBody;

        private void Awake()
        {
            spriteAnimator = GetComponent<SpriteAnimator>();
            rigidBody = GetComponent<Rigidbody2D>();
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
                spriteAnimator.Play("AlienDuck");
            }
            else
            {
                spriteAnimator.Play("AlienStand");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rigidBody.AddForce(Vector2.up * 500);
            }

            if (!spriteAnimator.CurrentAnimation.Equals("AlienDuck"))
            {
                if (rigidBody.velocity.y != 0)
                {
                    spriteAnimator.Play("AlienJump");
                }
                else if (rigidBody.velocity.x != 0)
                {
                    spriteAnimator.Play("AlienWalk");
                }
            }
        }
    }
}