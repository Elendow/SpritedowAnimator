// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEditor;

namespace Elendow.SpritedowAnimator
{
    [CustomEditor(typeof(SpriteAnimator))]
    public class EditorSpriteAnimator : EditorBaseAnimator
    {
        private SpriteAnimator t;

        public override void OnInspectorGUI()
        {
            t = (SpriteAnimator)target;
            DrawInspector(t);
        }
    }
}