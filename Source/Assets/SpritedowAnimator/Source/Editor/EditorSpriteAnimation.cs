// Spritedow Animation Plugin by Elendow
// https://elendow.com

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Elendow.SpritedowAnimator
{
    /// <summary>Editor class to show the animation preview.</summary>
    [CustomEditor(typeof(SpriteAnimation))]
    public class EditorSpriteAnimation : Editor
    {
        private const float PANNING_SPEED = 0.5f;
        private const string FPS_EDITOR_PREFS = "spritedowFPSpreviewWindow";

        private bool init = false;
        private bool enabled = false;
        private bool isPlaying = false;
        private bool forceRepaint = false;
        private bool loop = true;
        private bool isPanning = false;
        private bool saveToDisk = false;
        private int currentFrame = 0;
        private int loadedFPS = 30;
        private int framesPerSecond = 30;
        private int frameDurationCounter = 0;
        private int frameListSelectedIndex = -1;
        private float animationTimer = 0;
        private float lastFrameEditorTime = 0;
        private float deltaTime;
        private Vector2 scrollWindowPosition;
        private SpriteAnimation animation = null;
        private ReorderableList frameList;
        private List<SpriteAnimationFrame> frames;
        private List<SpriteAnimationAction> actions;

        // Styles
        private GUIStyle previewButtonSettings;
        private GUIStyle preSlider;
        private GUIStyle preSliderThumb;
        private GUIStyle preLabel;
        private GUIContent speedScale;
        private GUIContent playButtonContent;
        private GUIContent pauseButtonContent;
        private GUIContent loopIcon;
        private GUIContent loopIconActive;

        private GameObject go;
        private GameObject cameraGO;
        private Camera pc;
        private SpriteRenderer sr;

#if !UNITY_5
        private Material linearMaterial;
        private Material defaultMaterial;
#endif

        private void OnEnable()
        {
            if (!enabled)
            {
                enabled = true;

                if (target == null)
                {
                    return;
                }

                if (animation == null)
                {
                    animation = (SpriteAnimation)target;
                    animation.Setup();
                }

                EditorApplication.update += Update;

                // Load last used settings
                loadedFPS = framesPerSecond = EditorPrefs.GetInt(FPS_EDITOR_PREFS, 30);

                // Setup preview object and camera
                go = EditorUtility.CreateGameObjectWithHideFlags("previewGO", HideFlags.HideAndDontSave, typeof(SpriteRenderer));
                cameraGO = EditorUtility.CreateGameObjectWithHideFlags("cameraGO", HideFlags.HideAndDontSave, typeof(Camera));
                sr = go.GetComponent<SpriteRenderer>();
                pc = cameraGO.GetComponent<Camera>();

#if !UNITY_5
                // Colorspace correction is only needed after Unity 5 for some reasons
                linearMaterial = Resources.Load<Material>("Spritedow");
                defaultMaterial = sr.sharedMaterial;
                if (PlayerSettings.colorSpace == ColorSpace.Linear)
                {
                    sr.sharedMaterial = linearMaterial;
                }
                else
                {
                    sr.sharedMaterial = defaultMaterial;
                }
#endif

                // Set camera
                pc.cameraType = CameraType.Preview;
                pc.clearFlags = CameraClearFlags.Depth;
                pc.backgroundColor = Color.clear;
                pc.orthographic = true;
                pc.orthographicSize = 3;
                pc.nearClipPlane = -10;
                pc.farClipPlane = 10;
                pc.targetDisplay = -1;
                pc.depth = -999;

                // Set renderer
                if (animation != null && animation.FramesCount > 0)
                {
                    sr.sprite = animation.Frames[0].Sprite;
                    cameraGO.transform.position = Vector2.zero;
                }

                // Get preview culling layer in order to render only the preview object and nothing more
                pc.cullingMask = -2147483648;
                go.layer = 0x1f;

                // Also, disable the object to prevent render on scene/game views
                go.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (enabled)
            {
                enabled = false;
                EditorApplication.update -= Update;

                if (frameList != null)
                {
                    frameList.drawHeaderCallback -= DrawFrameListHeader;
                    frameList.drawElementCallback -= DrawFrameListElement;
                    frameList.onAddCallback -= AddFrameListItem;
                    frameList.onRemoveCallback -= RemoveFrameListItem;
                    frameList.onSelectCallback -= SelectFrameListItem;
                    frameList.onReorderCallback -= ReorderFrameListItem;
                }

                if (go != null)
                {
                    DestroyImmediate(go);
                }

                if (cameraGO != null)
                {
                    DestroyImmediate(cameraGO);
                }
            }
        }

        private void Update()
        {
#if !UNITY_5
            if (sr != null)
            {
                if (PlayerSettings.colorSpace == ColorSpace.Linear)
                {
                    sr.sharedMaterial = linearMaterial;
                }
                else
                {
                    sr.sharedMaterial = defaultMaterial;
                }
            }
#endif

            // Calculate deltaTime
            float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
            deltaTime = timeSinceStartup - lastFrameEditorTime;
            lastFrameEditorTime = timeSinceStartup;

            if (animation == null)
            {
                return;
            }

            if (frameList == null || SpriteEditorHelper.CheckListOutOfSync(frames, actions, animation))
            {
                InitializeReorderableList();
            }

            // Check animation bounds
            if (currentFrame < 0)
            {
                currentFrame = 0;
            }
            else if (currentFrame > animation.FramesCount)
            {
                currentFrame = animation.FramesCount - 1;
            }

            // Check if playing and use the editor time to change frames
            if (isPlaying)
            {
                animationTimer += deltaTime;
                float timePerFrame = 1f / framesPerSecond;
                if (timePerFrame < animationTimer)
                {
                    frameDurationCounter++;
                    animationTimer -= timePerFrame;
                    if (frameDurationCounter >= animation.Frames[currentFrame].Duration)
                    {
                        // Change frame and repaint the preview
                        currentFrame++;
                        if (currentFrame >= animation.FramesCount)
                        {
                            currentFrame = 0;

                            if (!loop)
                            {
                                isPlaying = false;
                            }
                        }

                        frameDurationCounter = 0;
                        Repaint();
                        forceRepaint = true;
                    }
                }
            }

            // Save preview FPS value on the editorPrefs
            if (framesPerSecond != loadedFPS)
            {
                loadedFPS = framesPerSecond;
                EditorPrefs.SetInt(FPS_EDITOR_PREFS, framesPerSecond);
            }
        }

        public override void OnInspectorGUI()
        {
            saveToDisk = false;

            FPSDrawer();
           
            Buttons();

            if (frameList != null)
            {
                scrollWindowPosition = EditorGUILayout.BeginScrollView(scrollWindowPosition);

                // Individual frames
                frameList.displayRemove = (animation.FramesCount > 0);
                frameList.DoLayoutList();
                EditorGUILayout.Space();

                EditorGUILayout.EndScrollView();
            }

            Buttons();

            if (GUI.changed || saveToDisk)
            {
                animation.Setup();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(animation);
                if(saveToDisk)
                {
                    AssetDatabase.SaveAssets();
                }
            }
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (currentFrame >= 0 &&
                animation != null &&
                animation.FramesCount > 0 &&
                currentFrame < animation.FramesCount)
            {
                // Draw Camera
                sr.sprite = animation.Frames[currentFrame].Sprite;
                go.SetActive(true);
                Handles.DrawCamera(r, pc);
                go.SetActive(false);

                // Check Events
                Event evt = Event.current;

                // Zoom preview window with scrollwheel
                if (evt.type == EventType.ScrollWheel)
                {
                    Vector2 mpos = Event.current.mousePosition;
                    if (mpos.x >= r.x && mpos.x <= r.x + r.width &&
                        mpos.y >= r.y && mpos.y <= r.y + r.height)
                    {
                        Zoom = -evt.delta.y;
                    }
                    forceRepaint = true;
                    Repaint();
                }
                // Stop panning on mouse up
                else if (evt.type == EventType.MouseUp)
                {
                    isPanning = false;
                }
                // Pan the camera with mouse drag
                else if (evt.type == EventType.MouseDrag)
                {
                    Vector2 mpos = Event.current.mousePosition;
                    if ((mpos.x >= r.x && mpos.x <= r.x + r.width &&
                        mpos.y >= r.y && mpos.y <= r.y + r.height) ||
                        isPanning)
                    {
                        Vector2 panning = Vector2.zero;
                        panning.x -= Event.current.delta.x;
                        panning.y += Event.current.delta.y;
                        cameraGO.transform.Translate(panning * PANNING_SPEED * deltaTime);
                        forceRepaint = true;
                        isPanning = true;
                        Repaint();
                    }
                }
                // Reset camera pressing F
                else if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.F)
                {
                    cameraGO.transform.position = Vector2.zero;
                    forceRepaint = true;
                    isPanning = true;
                    Repaint();
                }
            }
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Animation Preview");
        }

        public override void OnPreviewSettings()
        {
            if (!init)
            {
                // Define styles
                previewButtonSettings = new GUIStyle("preButton");
                preSlider = new GUIStyle("preSlider");
                preSliderThumb = new GUIStyle("preSliderThumb");
                preLabel = new GUIStyle("preLabel");
                speedScale = EditorGUIUtility.IconContent("SpeedScale");
                playButtonContent = EditorGUIUtility.IconContent("PlayButton");
                pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
                loopIcon = EditorGUIUtility.IconContent("RotateTool");
                loopIconActive = EditorGUIUtility.IconContent("RotateTool On");
                init = true;
            }

            // Play Button
            GUIContent buttonContent = isPlaying ? pauseButtonContent : playButtonContent;
            isPlaying = GUILayout.Toggle(isPlaying, buttonContent, previewButtonSettings);

            // Loop Button
            GUIContent loopContent = loop ? loopIconActive : loopIcon;
            loop = GUILayout.Toggle(loop, loopContent, previewButtonSettings);

            // FPS Slider
            GUILayout.Box(speedScale, preLabel);
            framesPerSecond = (int)GUILayout.HorizontalSlider(framesPerSecond, 0, 60, preSlider, preSliderThumb);
            GUILayout.Label(framesPerSecond.ToString("0") + " fps", preLabel, GUILayout.Width(50));
        }

		public override bool HasPreviewGUI()
		{
            // The preview is broken on the inspector right now.
            return false; // (animation != null && animation.FramesCount > 0);
		}

        #region Drawers
        public void FPSDrawer()
        {
            EditorGUI.BeginChangeCheck();
            int fpsValue = EditorGUILayout.IntField("FPS", animation.FPS);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(animation, "Change FPS");
                animation.FPS = fpsValue;
            }
        }

        private void Buttons()
        {
            if (animation.FramesCount > 0)
            {
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    if (GUILayout.Button("Delete All Frames"))
                    {
                        Undo.RecordObject(animation, "Delete All Frames");
                        animation.Frames.Clear();
                        InitializeReorderableList();
                        saveToDisk = true;
                    }

                    if (GUILayout.Button("Reverse Frames"))
                    {
                        Undo.RecordObject(animation, "Reverse Frames");
                        List<SpriteAnimationFrame> prevFrames = new List<SpriteAnimationFrame>(animation.Frames);
                        animation.Frames.Clear();

                        for (int i = prevFrames.Count - 1; i >= 0; i--)
                        {
                            animation.Frames.Add(prevFrames[i]);
                        }

                        InitializeReorderableList();
                        saveToDisk = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }
        }
        #endregion


        #region Reorderable List Methods
        private void InitializeReorderableList()
        {
            if (animation == null)
            {
                return;
            }

            if (frames == null)
            {
                frames = new List<SpriteAnimationFrame>();
            }

            if (actions == null)
            {
                actions = new List<SpriteAnimationAction>();
            }

            frames.Clear();
            actions.Clear();

            for (int i = 0; i < animation.Actions.Count; i++)
            {
                actions.Add(animation.Actions[i]);
            }

            for (int i = 0; i < animation.FramesCount; i++)
            {
                frames.Add(animation.Frames[i]);
            }

            // Kill listener of the previous list
            if (frameList != null)
            {
                frameList.drawHeaderCallback -= DrawFrameListHeader;
                frameList.drawElementCallback -= DrawFrameListElement;
                frameList.onAddCallback -= AddFrameListItem;
                frameList.onRemoveCallback -= RemoveFrameListItem;
                frameList.onSelectCallback -= SelectFrameListItem;
                frameList.onReorderCallback -= ReorderFrameListItem;
                frameList.elementHeightCallback -= ElementHeightCallback;
            }

            frameList = new ReorderableList(frames, typeof(SpriteAnimationFrame));
            frameList.drawHeaderCallback += DrawFrameListHeader;
            frameList.drawElementCallback += DrawFrameListElement;
            frameList.onAddCallback += AddFrameListItem;
            frameList.onRemoveCallback += RemoveFrameListItem;
            frameList.onSelectCallback += SelectFrameListItem;
            frameList.onReorderCallback += ReorderFrameListItem;
            frameList.elementHeightCallback += ElementHeightCallback;
        }

        private void DrawFrameListHeader(Rect r)
        {
            GUI.Label(r, "Frame List");
        }

        private void DrawFrameListElement(Rect r, int i, bool active, bool focused)
        {
            if(speedScale == null)
            {
                speedScale = EditorGUIUtility.IconContent("SpeedScale");
            }

            if (i < animation.FramesCount)
            {
                SpriteEditorHelper.DrawFrameListElement(ref animation, speedScale, r, i, active, focused);
            }
        }

        private void AddFrameListItem(ReorderableList list)
        {
            Undo.RecordObject(animation, "Add Frame");
            AddFrame();
            EditorUtility.SetDirty(animation);
        }

        private void AddFrame()
        {
            frameList.list.Add(new SpriteAnimationFrame(null, 1));
            animation.Frames.Add(null);
        }

        private void RemoveFrameListItem(ReorderableList list)
        {
            Undo.RecordObject(animation, "Remove Frame");

            int i = list.index;
            animation.Frames.RemoveAt(i);
            frameList.list.RemoveAt(i);
            frameListSelectedIndex = frameList.index;

            if (i >= animation.FramesCount)
            {
                frameList.index -= 1;
                frameListSelectedIndex -= 1;
                currentFrame = frameListSelectedIndex;
                frameList.GrabKeyboardFocus();
            }

            EditorUtility.SetDirty(animation);
            Repaint();
        }

        private void ReorderFrameListItem(ReorderableList list)
        {
            Undo.RecordObject(animation, "Reorder Frames");

            SpriteAnimationFrame frame = animation.Frames[frameListSelectedIndex];

            animation.Frames.RemoveAt(frameListSelectedIndex);
            animation.Frames.Insert(list.index, frame);

            EditorUtility.SetDirty(animation);
        }

        private void SelectFrameListItem(ReorderableList list)
        {
            currentFrame = list.index;
            forceRepaint = true;
            frameListSelectedIndex = list.index;
        }

        private float ElementHeightCallback(int index)
        {
            if (index >= animation.FramesCount)
            {
                return 0;
            }

            return SpriteEditorHelper.GetElementHeight(animation, index);
        }
        #endregion

        #region Properties
        public int FramesPerSecond
        {
            get => framesPerSecond;
            set => framesPerSecond = value;
        }

        public bool IsPlaying
        {
            get => isPlaying; 
            set => isPlaying = value; 
        }

        public bool ForceRepaint
        {
            get =>  forceRepaint;
            set => forceRepaint = value;
        }

        public SpriteAnimation CurrentAnimation
        {
            get => animation;
        }

        public int CurrentFrame
        {
            set => currentFrame = value;
        }

        public bool Loop
        {
            get => loop;
            set => loop = value;
        }

        public float Zoom
        {
            set
            {
                if (pc != null)
                {
                    float z = value / 50f;
                    if (pc.orthographicSize + z >= 0.1f &&
                        pc.orthographicSize + z <= 100)
                    {
                        pc.orthographicSize += z;
                    }
                }
            }
        }

        public bool IsPanning
        {
            get => isPanning;
            set => isPanning = value;
        }
        #endregion
    }
}