// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// Editor class to show the animation preview
    /// </summary>
    [CustomEditor(typeof(SpriteAnimation))]
    public class EditorPreviewSpriteAnimation : Editor
    {
        private const float PANNING_SPEED = 0.5f;
        private const string FPS_EDITOR_PREFS = "spritedowFPSpreviewWindow";

        private bool init = false;
        private bool isPlaying = false;
        private bool forceRepaint = false;
        private bool loop = true;
        private bool isPanning = false;
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
        private List<AnimationFrame> frames;

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

        private void OnEnable()
        {
            if (target == null)
                return;

            if (animation == null)
                animation = (SpriteAnimation)target;

            EditorApplication.update += Update;
            init = false;

            // Load last used settings
            loadedFPS = framesPerSecond = EditorPrefs.GetInt(FPS_EDITOR_PREFS, 30);

            // Setup preview object and camera
            go = EditorUtility.CreateGameObjectWithHideFlags("previewGO", HideFlags.HideAndDontSave, typeof(SpriteRenderer));
            cameraGO = EditorUtility.CreateGameObjectWithHideFlags("cameraGO", HideFlags.HideAndDontSave, typeof(Camera));
            sr = go.GetComponent<SpriteRenderer>();
            pc = cameraGO.GetComponent<Camera>();

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
                sr.sprite = animation.Frames[0];
                cameraGO.transform.position = Vector2.zero;
            }

            // Get preview culling layer in order to render only the preview object and nothing more
            pc.cullingMask = -2147483648;
            go.layer = 0x1f;

            // Also, disable the object to prevent render on scene/game views
            go.SetActive(false);
        }

        private void OnDisable()
        {
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
                DestroyImmediate(go);

            if (cameraGO != null)
                DestroyImmediate(cameraGO);
        }

        private void Update()
        {
            // Calculate deltaTime
            float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
            deltaTime = timeSinceStartup - lastFrameEditorTime;
            lastFrameEditorTime = timeSinceStartup;

            if (animation == null || animation.FramesCount == 0)
                return;

            if (frameList == null)
                InitializeReorderableList();

            // Check animation bounds
            if (currentFrame < 0) currentFrame = 0;
            else if (currentFrame > animation.FramesCount) currentFrame = animation.FramesCount - 1;

            // Check if playing and use the editor time to change frames
            if (isPlaying)
            {
                animationTimer += deltaTime;

                if (1f / framesPerSecond < animationTimer)
                {
                    frameDurationCounter++;
                    animationTimer = 0;
                    if (frameDurationCounter >= animation.FramesDuration[currentFrame])
                    {
                        // Change frame and repaint the preview
                        currentFrame++;
                        if (currentFrame >= animation.FramesCount)
                        {
                            currentFrame = 0;

                            if (!loop)
                                isPlaying = false;
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
			animation.FPS = EditorGUILayout.IntField("FPS", animation.FPS);

            if (frameList != null)
            {
                scrollWindowPosition = EditorGUILayout.BeginScrollView(scrollWindowPosition);

                // Individual frames
                frameList.displayRemove = (animation.FramesCount > 0);
                frameList.DoLayoutList();
                EditorGUILayout.Space();

                EditorGUILayout.EndScrollView();
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
                sr.sprite = animation.Frames[currentFrame];
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
			return (animation != null && animation.FramesCount > 0);
		}

        #region Reorderable List Methods
        private void InitializeReorderableList()
        {
            if (animation == null)
                return;

            if (frames == null)
                frames = new List<AnimationFrame>();

            frames.Clear();

            for (int i = 0; i < animation.FramesCount; i++)
                frames.Add(new AnimationFrame(animation.Frames[i], animation.FramesDuration[i]));

            // Kill listener of the previous list
            if (frameList != null)
            {
                frameList.drawHeaderCallback -= DrawFrameListHeader;
                frameList.drawElementCallback -= DrawFrameListElement;
                frameList.onAddCallback -= AddFrameListItem;
                frameList.onRemoveCallback -= RemoveFrameListItem;
                frameList.onSelectCallback -= SelectFrameListItem;
                frameList.onReorderCallback -= ReorderFrameListItem;
            }

            frameList = new ReorderableList(frames, typeof(AnimationFrame));
            frameList.drawHeaderCallback += DrawFrameListHeader;
            frameList.drawElementCallback += DrawFrameListElement;
            frameList.onAddCallback += AddFrameListItem;
            frameList.onRemoveCallback += RemoveFrameListItem;
            frameList.onSelectCallback += SelectFrameListItem;
            frameList.onReorderCallback += ReorderFrameListItem;
        }

        private void DrawFrameListHeader(Rect r)
        {
            GUI.Label(r, "Frame List");
        }

        private void DrawFrameListElement(Rect r, int i, bool active, bool focused)
        {
            if(speedScale == null)
                speedScale = EditorGUIUtility.IconContent("SpeedScale");

            if (i < animation.FramesCount)
            {
                EditorGUI.BeginChangeCheck();

                string spriteName = (animation.Frames[i] != null) ? animation.Frames[i].name : "No sprite selected";
                EditorGUIUtility.labelWidth = r.width - 105;
                animation.Frames[i] = EditorGUI.ObjectField(new Rect(r.x + 10, r.y + 1, r.width - 85, r.height - 4), spriteName, animation.Frames[i], typeof(Sprite), false) as Sprite;

                EditorGUIUtility.labelWidth = 20;
                animation.FramesDuration[i] = EditorGUI.IntField(new Rect(r.x + r.width - 50, r.y + 1, 50, r.height - 4), speedScale, animation.FramesDuration[i]);

                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(animation);
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
            frameList.list.Add(new AnimationFrame(null, 1));
            animation.Frames.Add(null);
            animation.FramesDuration.Add(1);
        }

        private void RemoveFrameListItem(ReorderableList list)
        {
            Undo.RecordObject(animation, "Remove Frame");

            int i = list.index;
            animation.Frames.RemoveAt(i);
            animation.FramesDuration.RemoveAt(i);
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

            Sprite s = animation.Frames[frameListSelectedIndex];
            animation.Frames.RemoveAt(frameListSelectedIndex);
            animation.Frames.Insert(list.index, s);

            int i = animation.FramesDuration[frameListSelectedIndex];
            animation.FramesDuration.RemoveAt(frameListSelectedIndex);
            animation.FramesDuration.Insert(list.index, i);

            EditorUtility.SetDirty(animation);
        }

        private void SelectFrameListItem(ReorderableList list)
        {
            currentFrame = list.index;
            forceRepaint = true;
            frameListSelectedIndex = list.index;
        }
        #endregion

        #region Properties
        public int FramesPerSecond
        {
            get { return framesPerSecond; }
            set { framesPerSecond = value; }
        }

        public bool IsPlaying
        {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        public bool ForceRepaint
        {
            get { return forceRepaint; }
            set { forceRepaint = value; }
        }

        public SpriteAnimation CurrentAnimation
        {
            get { return animation; }
        }

        public int CurrentFrame
        {
            set { currentFrame = value; }
        }

        public bool Loop
        {
            get { return loop; }
            set { loop = value; }
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
            get { return isPanning; }
            set { isPanning = value; }
        }
        #endregion
    }
}