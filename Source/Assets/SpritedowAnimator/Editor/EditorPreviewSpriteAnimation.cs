// Spritedow Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/SpritedowAnimator
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// Editor class to show the animation preview
    /// </summary>
    [CustomEditor(typeof(SpriteAnimation))]
    public class EditorPreviewSpriteAnimation : Editor
    {
        private bool init = false;
        private bool isPlaying = false;
        private bool forceRepaint = false;
        private bool loop = true;
        private int currentFrame = 0;
        private int framesPerSecond = 30;
        private int frameDurationCounter = 0;
        private float animationTimer = 0;
        private float lastFrameEditorTime = 0;
        private SpriteAnimation animation = null;

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
            if (animation == null)
                animation = (SpriteAnimation)target;

            EditorApplication.update += Update;
            init = false;

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
            if(animation != null && animation.FramesCount > 0)
            {
                sr.sprite = animation.Frames[0];
                cameraGO.transform.position = sr.bounds.center;
            }

            // Get preview culling layer in order to render only the preview object and nothing more
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            PropertyInfo propInfo = typeof(Camera).GetProperty("PreviewCullingLayer", flags);
            int previewLayer = (int)propInfo.GetValue(null, new object[0]);
            pc.cullingMask = 1 << previewLayer;
            go.layer = previewLayer;

            // Also, disable the object to prevent render on scene/game views
            go.SetActive(false);
        }

        private void OnDisable()
		{
			EditorApplication.update -= Update;
			if(go != null)
				DestroyImmediate(go);
            if (cameraGO != null)
                DestroyImmediate(cameraGO);
		}

        private void Update()
        {
            if (animation == null || animation.FramesCount == 0)
                return;

            // Center camera only on the first frame (this allows animations with different pivot points)
            if(currentFrame == 0)
                cameraGO.transform.position = sr.bounds.center;

            // Check if playing and use the editor time to change frames
            if (isPlaying)
            {
                // Calculate deltaTime
                float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
                float deltaTime = timeSinceStartup - lastFrameEditorTime;
                lastFrameEditorTime = timeSinceStartup;
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
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
			if (animation != null && animation.FramesCount > 0 && currentFrame < animation.FramesCount)
            {
                // Draw Camera
                sr.sprite = animation.Frames[currentFrame];
                go.SetActive(true);
				Handles.DrawCamera(r, pc);
                go.SetActive(false);

                // Check Events
                Event evt = Event.current;
                switch (evt.type)
                {
                    // Zoom preview window with scrollwheel
                    case EventType.ScrollWheel:
                        Vector2 mpos = Event.current.mousePosition;
                        if (mpos.x >= r.x && mpos.x <= r.x + r.width &&
                            mpos.y >= r.y && mpos.y <= r.y + r.height)
                        {
                            Repaint();
                            Zoom = -evt.delta.y;
                        }
                        break;
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
            set { forceRepaint = true; }
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
                if(pc != null)
                {
                    float z = value / 50f;
                    if (pc.orthographicSize + z >= 1 &&
                        pc.orthographicSize + z <= 10)
                    {
                        pc.orthographicSize += z;
                    }
                }
            }
        }
    }
}