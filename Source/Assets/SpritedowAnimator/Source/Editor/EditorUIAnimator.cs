// Spritedow Animation Plugin by Elendow
// https://elendow.com

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
            Undo.RecordObject(t, "Edit UI Animator");

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                t.preserveAspectRatio = EditorGUILayout.Toggle("Preserve Aspect Ratio", t.preserveAspectRatio);
            }
            EditorGUILayout.EndVertical();

            DrawInspector(t);
        }
    }
}