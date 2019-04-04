// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Elendow.SpritedowAnimator
{
    [CustomEditor(typeof(BaseAnimator))]
    public class EditorBaseAnimator : Editor
    {
        private static readonly string SHOW_ANIMATION_LIST_KEY = "ShowAnimationList";
        private static bool showAnimationList;

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
        private SerializedProperty fallbackLoopType;

        private GUIStyle dragAndDropStyle;
        private GUIStyle animationBoxStyle;
        private GUIContent emptyContent;
        private GUIContent arrowContent;
        private List<SpriteAnimation> draggedAnimations;
        
        /// <summary>
        /// Initialize again on enable
        /// </summary>
        private void OnEnable()
        {
            init = false;
        }

        /// <summary>
        /// Draws a common inspector
        /// </summary>
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

            if (targetAnimator.animations == null)
                targetAnimator.animations = new List<SpriteAnimation>();

            dragAndDropStyle = new GUIStyle(EditorStyles.helpBox);
            dragAndDropStyle.richText = true;
            dragAndDropStyle.alignment = TextAnchor.MiddleCenter;

            animationBoxStyle = new GUIStyle(EditorStyles.helpBox);
            animationBoxStyle.margin.left += 10;

            emptyContent = new GUIContent("");
            arrowContent = new GUIContent(EditorGUIUtility.IconContent("UpArrow"));

            draggedAnimations = new List<SpriteAnimation>();

            if (!targetAnimator.StartAnimation.Equals(""))
            {
                int startAnimationIndex = -1;
                for (int i = 0; i < targetAnimator.animations.Count; i++)
                {
                    if (targetAnimator.animations[i] != null &&
                        targetAnimator.animations[i].Name.Equals(targetAnimator.StartAnimation))
                        startAnimationIndex = i;
                }

                if (startAnimationIndex != -1)
                    MoveStartingAnimationToTop(targetAnimator, startAnimationIndex);
                else
                    targetAnimator.StartAnimation = "";
            }

            showAnimationList = EditorPrefs.GetBool(SHOW_ANIMATION_LIST_KEY, true);
        }

        /// <summary>
        /// Draws a common inspector
        /// </summary>
        protected void DrawInspector(BaseAnimator targetAnimator)
        {
            if (!init)
                Init(targetAnimator);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Basic Settings");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Ignore time scale", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(ignoreTimeScale, emptyContent, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Start at random frame", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(startAtRandomFrame, emptyContent, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Play on awake", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(playOnAwake, emptyContent, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();

                if (playOnAwake.boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("One shot", GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(oneShot, emptyContent, GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();

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

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Disable render on finish", GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(disableRendererOnFinish, emptyContent, GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Backwards", GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(backwards, emptyContent, GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Random animation", GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(randomAnimation, emptyContent, GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Fallback animation", GUILayout.ExpandWidth(false));
                GUILayout.FlexibleSpace();
                targetAnimator.FallbackAnimation = EditorGUILayout.ObjectField(targetAnimator.FallbackAnimation, typeof(SpriteAnimation), false) as SpriteAnimation;
                EditorGUILayout.EndHorizontal();

                if (targetAnimator.FallbackAnimation != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Fallback animation loop type", GUILayout.ExpandWidth(false));
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.PropertyField(fallbackLoopType, emptyContent, GUILayout.ExpandWidth(false));
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            showAnimationList = EditorGUI.Foldout(GUILayoutUtility.GetRect(40f, 40f, 16f, 16f), showAnimationList, "Animation List", true);

            DragAndDropBox(targetAnimator);

            if (showAnimationList)
                DrawAnimationList(targetAnimator);
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.EndVertical();
            }

            if (draggedAnimations != null && draggedAnimations.Count > 0)
            {
                showAnimationList = true;
                for (int i = 0; i < draggedAnimations.Count; i++)
                {
                    targetAnimator.animations.Add(draggedAnimations[i]);
                    if (targetAnimator.animations.Count == 1 && targetAnimator.animations[0] != null)
                        targetAnimator.StartAnimation = targetAnimator.animations[0].Name;
                }

                draggedAnimations.Clear();
            }

            if (GUI.changed)
            {
                EditorPrefs.SetBool(SHOW_ANIMATION_LIST_KEY, showAnimationList);

                if (targetAnimator.FallbackAnimation != null)
                    if (!targetAnimator.animations.Contains(targetAnimator.FallbackAnimation))
                        targetAnimator.animations.Add(targetAnimator.FallbackAnimation);

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(targetAnimator);
            }
        }

        private void DrawAnimationList(BaseAnimator targetAnimator)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // Draw animation list
                if (targetAnimator.animations != null && targetAnimator.animations.Count > 0)
                {
                    if (targetAnimator.animations[0] != null)
                        targetAnimator.StartAnimation = targetAnimator.animations[0].Name;

                    int toRemove = -1;
                    int toFirst = -1;
                    for (int i = 0; i < targetAnimator.animations.Count; i++)
                    {
                        bool fallback = (targetAnimator.FallbackAnimation != null && targetAnimator.FallbackAnimation.Name.Equals(targetAnimator.animations[i].Name));

                        EditorGUILayout.BeginVertical(i == 0 ? EditorStyles.helpBox : animationBoxStyle);
                        {
                            if (i == 0)
                            {
                                if(!fallback)
                                    EditorGUILayout.LabelField("Starting Animation", EditorStyles.miniLabel);
                                else
                                    EditorGUILayout.LabelField("Starting and Fallback Animation", EditorStyles.miniLabel);
                            }
                            else
                            {
                                if (fallback)
                                    EditorGUILayout.LabelField("Fallback Animation", EditorStyles.miniLabel);
                            }

                            EditorGUILayout.BeginHorizontal();
                            {
                                targetAnimator.animations[i] = EditorGUILayout.ObjectField("", targetAnimator.animations[i], typeof(SpriteAnimation), false) as SpriteAnimation;

                                // Make this animation the first one
                                if (i != 0 && GUILayout.Button(arrowContent, GUILayout.Width(30), GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
                                {
                                    toFirst = i;
                                    targetAnimator.StartAnimation = targetAnimator.animations[i].Name;
                                }
                                
                                // Remove animation
                                if (GUILayout.Button("-", GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(false)))
                                    toRemove = i;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }

                    if (toRemove != -1)
                    {
                        targetAnimator.animations.RemoveAt(toRemove);
                        if (targetAnimator.animations.Count == 0)
                            targetAnimator.StartAnimation = "";
                    }

                    if (toFirst != -1)
                        MoveStartingAnimationToTop(targetAnimator, toFirst);
                }
                else
                    targetAnimator.StartAnimation = "";
            }
            EditorGUILayout.EndVertical();
        }

        private void MoveStartingAnimationToTop(BaseAnimator targetAnimator, int startAnimationIndex)
        {
            List<SpriteAnimation> auxList = new List<SpriteAnimation>(targetAnimator.animations);
            targetAnimator.animations.Clear();
            targetAnimator.animations.Add(auxList[startAnimationIndex]);
            for (int i = 0; i < auxList.Count; i++)
            {
                if (i != startAnimationIndex)
                    targetAnimator.animations.Add(auxList[i]);
            }
        }

        private void DragAndDropBox(BaseAnimator targetAnimator)
        {
            // Drag and drop box for sprite frames
            Rect dropArea = GUILayoutUtility.GetRect(0f, 50, GUILayout.ExpandWidth(true));
            Event evt = Event.current;
            GUI.Box(dropArea, "Drop animations <b>HERE</b>.", dragAndDropStyle);
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        if (DragAndDrop.objectReferences.Length > 0)
                        {
                            DragAndDrop.AcceptDrag();
                            draggedAnimations.Clear();
                            foreach (Object draggedObject in DragAndDrop.objectReferences)
                            {
                                // Get dragged sprites
                                SpriteAnimation s = draggedObject as SpriteAnimation;
                                if (s != null && !targetAnimator.animations.Contains(s))
                                    draggedAnimations.Add(s);
                            }
                        }
                    }
                    break;
            }
        }
    }
}