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
                t.adaptPivot = EditorGUILayout.Toggle("Adapt UI Pivot", t.adaptPivot);
            }
            EditorGUILayout.EndVertical();

            DrawInspector(t);
        }
    }
}