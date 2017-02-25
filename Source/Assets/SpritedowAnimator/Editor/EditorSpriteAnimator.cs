// Spritedow Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/SpritedowAnimator
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
            Undo.RecordObject(t, "Edit Sprite Animator");
            DrawInspector(t);
        }
    }
}