// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
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