// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using UnityEditor;

namespace Elendow.SpritedowAnimator
{
    [CustomEditor(typeof(BaseAnimator))]
    public class EditorBaseAnimator : Editor
    {
        private SerializedProperty playOnAwake;
        private SerializedProperty ignoreTimeScale;
        private SerializedProperty delayBetweenLoops;
        private SerializedProperty minDelayBetweenLoops;
        private SerializedProperty maxDelayBetweenLoops;
        private SerializedProperty startAnimation;
        private SerializedProperty animations;
        private SerializedProperty oneShot;
        private SerializedProperty backwards;
        private SerializedProperty randomAnimation;
        private SerializedProperty disableRendererOnFinish;

        protected int startAnimationIndex = 0;
        protected string[] animationNames;

        /// <summary>
        /// Draws a common inspector
        /// </summary>
        /// <param name="targetAnimator"></param>
        protected void DrawInspector(BaseAnimator targetAnimator)
        {
            playOnAwake = serializedObject.FindProperty("playOnAwake");
            ignoreTimeScale = serializedObject.FindProperty("ignoreTimeScale");
            delayBetweenLoops = serializedObject.FindProperty("delayBetweenLoops");
            minDelayBetweenLoops = serializedObject.FindProperty("minDelayBetweenLoops");
            maxDelayBetweenLoops = serializedObject.FindProperty("maxDelayBetweenLoops");
            startAnimation = serializedObject.FindProperty("startAnimation");
            animations = serializedObject.FindProperty("animations");
            oneShot = serializedObject.FindProperty("oneShot");
            backwards = serializedObject.FindProperty("backwards");
            randomAnimation = serializedObject.FindProperty("randomAnimation");
            disableRendererOnFinish = serializedObject.FindProperty("disableRendererOnFinish");

            if (animationNames == null)
                GetAnimationNames(targetAnimator);

            EditorGUILayout.PropertyField(ignoreTimeScale);
            EditorGUILayout.PropertyField(playOnAwake);

            if (playOnAwake.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(oneShot);
                if (!oneShot.boolValue)
                {
                    EditorGUI.indentLevel++;

                    delayBetweenLoops.boolValue = EditorGUILayout.BeginToggleGroup("Delay", delayBetweenLoops.boolValue);
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        EditorGUIUtility.labelWidth = 65;
                        EditorGUILayout.PropertyField(minDelayBetweenLoops);
                        EditorGUILayout.PropertyField(maxDelayBetweenLoops);
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndToggleGroup();

                    if (minDelayBetweenLoops.floatValue < 0)
                        minDelayBetweenLoops.floatValue = 0;

                    if (maxDelayBetweenLoops.floatValue < minDelayBetweenLoops.floatValue)
                        maxDelayBetweenLoops.floatValue = minDelayBetweenLoops.floatValue;

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
                        startAnimation.stringValue = animationNames[startAnimationIndex];
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
                        if (targetAnimator.animations[i].Name == startAnimation.stringValue)
                            startAnimationIndex = i;
                    }
                    else
                        animationNames[i] = "null" + i;
                }
            }
        }
    }
}