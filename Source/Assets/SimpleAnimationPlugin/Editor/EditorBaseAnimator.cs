// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseAnimator))]
public class EditorBaseAnimator : Editor
{
    protected int startAnimationIndex = 0;
    protected string[] animationNames;

    private BaseAnimator t;

    public override void OnInspectorGUI()
    {
        t = (UIAnimator)target;
        DrawInspector(t);
    }

    protected void DrawInspector(BaseAnimator targetAnimator)
    {
        if (animationNames == null)
            GetAnimationNames(targetAnimator);

        SerializedProperty animations = serializedObject.FindProperty("animations");
        targetAnimator.playOnAwake = EditorGUILayout.Toggle("Play on Awake", targetAnimator.playOnAwake);
        if (targetAnimator.playOnAwake)
        {
            if (animationNames != null && animationNames.Length > 0)
            {
                startAnimationIndex = EditorGUILayout.Popup("Start Animation", startAnimationIndex, animationNames);
                targetAnimator.startAnimation = animationNames[startAnimationIndex];
            }
            else
            {
                EditorGUILayout.LabelField("Start animation", "No animations");
            }
            EditorUtility.SetDirty(targetAnimator);
        }

        targetAnimator.framesPerSecond = EditorGUILayout.IntField("FPS", targetAnimator.framesPerSecond);
        if (targetAnimator.framesPerSecond < 0) targetAnimator.framesPerSecond = 0;

        EditorGUILayout.PropertyField(animations, true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetAnimator);
            serializedObject.ApplyModifiedProperties();
            GetAnimationNames(targetAnimator);
        }
    }

    private void GetAnimationNames(BaseAnimator targetAnimator)
    {
        if (targetAnimator.animations != null && targetAnimator.animations.Count > 0)
        {
            animationNames = new string[targetAnimator.animations.Count];
            for (int i = 0; i < animationNames.Length; i++)
            {
                if (targetAnimator.animations[i])
                {
                    animationNames[i] = targetAnimator.animations[i].Name;
                    if (targetAnimator.animations[i].Name == targetAnimator.startAnimation)
                        startAnimationIndex = i;
                }
                else
                    animationNames[i] = "null" + i;
            }
        }
    }
}
