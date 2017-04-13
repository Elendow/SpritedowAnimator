// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using UnityEditor;

namespace Elendow.SpritedowAnimator
{
    [CustomEditor(typeof(BaseAnimator))]
    public class EditorBaseAnimator : Editor
    {
        protected int startAnimationIndex = 0;
        protected string[] animationNames;

        /// <summary>
        /// Draws a common inspector
        /// </summary>
        /// <param name="targetAnimator"></param>
        protected void DrawInspector(BaseAnimator targetAnimator)
        {
            if (animationNames == null)
                GetAnimationNames(targetAnimator);

            SerializedProperty animations = serializedObject.FindProperty("animations");
            SerializedProperty oneShot = serializedObject.FindProperty("oneShot");
            SerializedProperty backwards = serializedObject.FindProperty("backwards");
            SerializedProperty randomAnimation = serializedObject.FindProperty("randomAnimation");
            SerializedProperty disableRendererOnFinish = serializedObject.FindProperty("disableRendererOnFinish");

            targetAnimator.ignoreTimeScale = EditorGUILayout.Toggle("Ignore TimeScale", targetAnimator.ignoreTimeScale);

            targetAnimator.playOnAwake = EditorGUILayout.Toggle("Play on Awake", targetAnimator.playOnAwake);

            if (targetAnimator.playOnAwake)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(oneShot);
                if (!oneShot.boolValue)
                {
                    EditorGUI.indentLevel++;

                    targetAnimator.delayBetweenLoops = EditorGUILayout.BeginToggleGroup("Delay", targetAnimator.delayBetweenLoops);
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        EditorGUIUtility.labelWidth = 65;
                        targetAnimator.minDelayBetweenLoops = EditorGUILayout.FloatField("Min", targetAnimator.minDelayBetweenLoops);
                        targetAnimator.maxDelayBetweenLoops = EditorGUILayout.FloatField("Max", targetAnimator.maxDelayBetweenLoops);
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndToggleGroup();

                    if (targetAnimator.minDelayBetweenLoops < 0)
                        targetAnimator.minDelayBetweenLoops = 0;

                    if (targetAnimator.maxDelayBetweenLoops < targetAnimator.minDelayBetweenLoops)
                        targetAnimator.maxDelayBetweenLoops = targetAnimator.minDelayBetweenLoops;

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.PropertyField(disableRendererOnFinish);
                EditorGUILayout.PropertyField(backwards);
                EditorGUILayout.PropertyField(randomAnimation);

                if (!randomAnimation.boolValue)
                {
                    if (animationNames != null && animationNames.Length > 0)
                    {
                        if (startAnimationIndex >= animationNames.Length)
                            startAnimationIndex = animationNames.Length - 1;

                        startAnimationIndex = EditorGUILayout.Popup("Start Animation", startAnimationIndex, animationNames);
                        targetAnimator.startAnimation = animationNames[startAnimationIndex];
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Start animation", "No animations");
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(animations, true);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(targetAnimator);
                GetAnimationNames(targetAnimator);
            }
        }

        /// <summary>
        /// Fetch animations on the target animator
        /// </summary>
        /// <param name="targetAnimator"></param>
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
}