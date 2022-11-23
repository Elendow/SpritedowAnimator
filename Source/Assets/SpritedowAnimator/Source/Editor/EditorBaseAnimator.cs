// Spritedow Animation Plugin by Elendow
// https://elendow.com

using UnityEngine;
using UnityEditor;

namespace Elendow.SpritedowAnimator
{
    [CustomEditor(typeof(BaseAnimator))]
    public class EditorBaseAnimator : Editor
    {
        private bool init;

        private SerializedProperty playOnAwake;
        private SerializedProperty ignoreTimeScale;
        private SerializedProperty delayBetweenLoops;
        private SerializedProperty minDelayBetweenLoops;
        private SerializedProperty maxDelayBetweenLoops;
        private SerializedProperty oneShot;
        private SerializedProperty backwards;
        private SerializedProperty randomAnimation;
        private SerializedProperty disableRendererOnFinish;
        private SerializedProperty startAtRandomFrame;
        private SerializedProperty loopType;
        private SerializedProperty fallbackAnimation;
        private SerializedProperty fallbackLoopType;
        private SerializedProperty enableRenderOnPlay;
        private SerializedProperty startAnimation;
        private SerializedProperty randomAnimationList;

        private GUIContent emptyContent;

        /// <summary>Initialize again on enable.</summary>
        private void OnEnable()
        {
            init = false;
        }

        /// <summary>Initializes the inspector.</summary>
        private void Init(BaseAnimator targetAnimator)
        {
            init = true;

            playOnAwake = serializedObject.FindProperty("playOnAwake");
            ignoreTimeScale = serializedObject.FindProperty("ignoreTimeScale");
            delayBetweenLoops = serializedObject.FindProperty("delayBetweenLoops");
            minDelayBetweenLoops = serializedObject.FindProperty("minDelayBetweenLoops");
            maxDelayBetweenLoops = serializedObject.FindProperty("maxDelayBetweenLoops");
            oneShot = serializedObject.FindProperty("oneShot");
            backwards = serializedObject.FindProperty("backwards");
            randomAnimation = serializedObject.FindProperty("randomAnimation");
            disableRendererOnFinish = serializedObject.FindProperty("disableRendererOnFinish");
            startAtRandomFrame = serializedObject.FindProperty("startAtRandomFrame");
            loopType = serializedObject.FindProperty("loopType");
            fallbackLoopType = serializedObject.FindProperty("fallbackLoopType");
            enableRenderOnPlay = serializedObject.FindProperty("enableRenderOnPlay");
            startAnimation = serializedObject.FindProperty("startAnimation");
            randomAnimationList = serializedObject.FindProperty("randomAnimationList");
            fallbackAnimation = serializedObject.FindProperty("fallbackAnimation");

            emptyContent = new GUIContent("");
        }

        /// <summary>Draws a common inspector.</summary>
        protected void DrawInspector(BaseAnimator targetAnimator)
        {
            if (!init)
                Init(targetAnimator);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.Space();
                DrawField("Ignore time scale", ignoreTimeScale);
                DrawField("Enable render on play", enableRenderOnPlay);
                DrawField("Disable render on finish", disableRendererOnFinish);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.Space();
                DrawField("Play on awake", playOnAwake);
                if (playOnAwake.boolValue)
                {
                    EditorGUI.indentLevel++;
                    DrawField("Start at random frame", startAtRandomFrame);
                    DrawField("Backwards", backwards);
                    DrawField("Random animation", randomAnimation);
                    if (!randomAnimation.boolValue)
                    {
                        DrawField("Animation", startAnimation);
                    }

                    EditorGUILayout.Space();

                    DrawField("One shot", oneShot);
                    if (!oneShot.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(loopType);
                        delayBetweenLoops.boolValue = EditorGUILayout.BeginToggleGroup("Delay", delayBetweenLoops.boolValue);
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                            EditorGUIUtility.labelWidth = 65;
                            EditorGUILayout.PropertyField(minDelayBetweenLoops, new GUIContent("Min"));
                            EditorGUILayout.PropertyField(maxDelayBetweenLoops, new GUIContent("Max"));
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
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    randomAnimationList,
                    new GUIContent("Random Animation List"),
                    GUILayout.ExpandWidth(true));
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                DrawField("Fallback animation", fallbackAnimation);
                if (fallbackAnimation.objectReferenceValue != null)
                {
                    EditorGUI.indentLevel++;
                    DrawField("Loop type", fallbackLoopType);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(targetAnimator);
            }
        }

        /// <summary>Draws a single field.</summary>
        private void DrawField(string label, SerializedProperty property)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField(property, emptyContent);
            EditorGUILayout.EndHorizontal();
        }
    }
}