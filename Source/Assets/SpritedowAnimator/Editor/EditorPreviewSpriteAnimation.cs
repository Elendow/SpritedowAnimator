// Spritedow Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/SpritedowAnimator
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

        private void OnEnable()
        {
            if (animation == null)
                animation = (SpriteAnimation)target;

            EditorApplication.update += Update;
            init = false;

        }

        private void Update()
        {
            if (animation == null || animation.FramesCount == 0)
                return;

            // Check if playing and use the editor time to change frames
            if (isPlaying)
            {
                // Calculate deltaTime
                float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
                float deltaTime = timeSinceStartup - lastFrameEditorTime;
                lastFrameEditorTime = timeSinceStartup;
                animationTimer += deltaTime;

                // Double check out of bounds, sometimes the preview on the editor may desynchronize and get errors
                if (currentFrame >= animation.FramesCount)
                    currentFrame = 0;

                if (1f / framesPerSecond < animationTimer)
                {
                    frameDurationCounter++;
                    animationTimer = 0;
                    if (frameDurationCounter >= animation.FramesDuration[currentFrame])
                    {
                        // Change frame and repaint the preview
                        currentFrame++;
                        if (currentFrame >= animation.FramesCount)
                            currentFrame = 0;
                        frameDurationCounter = 0;
                        Repaint();
                        forceRepaint = true;
                    }
                }
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
			if (animation != null && animation.FramesCount > 0 && currentFrame < animation.FramesCount)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(animation.Frames[currentFrame]);
                if (texture != null)
                {
                    // Use the filtermode of the texture to preview the sprite as ingame
                    texture.filterMode = animation.Frames[currentFrame].texture.filterMode;
                    EditorGUI.DrawTextureTransparent(r, texture, ScaleMode.ScaleToFit);
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
                init = true;
            }

            // Play Button
            GUIContent buttonContent = isPlaying ? pauseButtonContent : playButtonContent;
            isPlaying = GUILayout.Toggle(isPlaying, buttonContent, previewButtonSettings);

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
    }
}