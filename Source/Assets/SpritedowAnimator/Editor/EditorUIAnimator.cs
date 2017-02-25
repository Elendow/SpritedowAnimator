// Spritedow Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/SpritedowAnimator
using UnityEditor;

namespace Elendow.SpritedowAnimator
{
    [CustomEditor(typeof(UIAnimator))]
    public class EditorUIAnimator : EditorBaseAnimator
    {
        private UIAnimator t;

        public override void OnInspectorGUI()
        {
            t = (UIAnimator)target;
            DrawInspector(t);
        }
    }
}