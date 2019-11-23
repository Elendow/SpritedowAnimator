// Spritedow Animation Plugin by Elendow
// http://elendow.com

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;

namespace Elendow.SpritedowAnimator
{
    /// <summary>
    /// Editor window to edit animations
    /// </summary>
    public class EditorSpriteAnimation : EditorWindow
    {
        private const float CONFIG_BOX_HEIGHT = 120;
        private const float DROP_AREA_HEIGHT = 50;
        private const float MIN_WINDOW_WIDTH = 500f;
        private const float MIN_WINDOW_HEIGHT = 200f;

        private bool init = false;
        private bool justCreatedAnim = false;
        private int frameListSelectedIndex = -1;
        private Texture2D clockIcon = null;
        private SpriteAnimation selectedAnimation = null;
        private Vector2 scrollWindowPosition = Vector2.zero;
        private List<Sprite> draggedSprites = null;
        private EditorPreviewSpriteAnimation spritePreview = null;
		private ReorderableList frameList;
		private List<AnimationFrame> frames;

        // Styles
        private GUIStyle box;
		private GUIStyle dragAndDropBox;
        private GUIStyle lowPaddingBox;
        private GUIStyle buttonStyle;
        private GUIStyle sliderStyle;
        private GUIStyle sliderThumbStyle;
        private GUIStyle labelStyle;
        private GUIStyle previewToolBar;
        private GUIStyle preview;
        private GUIContent playButtonContent;
        private GUIContent pauseButtonContent;
		private GUIContent speedScaleIcon;
        private GUIContent loopIcon;
        private GUIContent loopIconActive;

        [MenuItem("Tools/Spritedow/Sprite Animation Editor", false, 0)]
        private static void ShowWindow()
        {
            GetWindow(typeof(EditorSpriteAnimation), false, "Sprite Animation");
        }

        [MenuItem("Assets/Create/Spritedow/Sprite Animation")]
        public static void CreateAsset()
        {
            SpriteAnimation asset = CreateInstance<SpriteAnimation>();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
                path = "Assets";
            else if (System.IO.Path.GetExtension(path) != "")
                path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Animation.asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnEnable()
        {
            // Get clock icon
            if (clockIcon == null)
                clockIcon = Resources.Load<Texture2D>("clockIcon");

            // Initialize
            draggedSprites = new List<Sprite>();
            init = false;

			// Events
			EditorApplication.update += Update;

            Undo.undoRedoPerformed += OnUndoOrRedo;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;

			if(frameList != null)
			{
				frameList.drawHeaderCallback -= DrawFrameListHeader;
				frameList.drawElementCallback -= DrawFrameListElement;
				frameList.onAddCallback -= AddFrameListItem;
				frameList.onRemoveCallback -= RemoveFrameListItem;
				frameList.onSelectCallback -= SelectFrameListItem;
				frameList.onReorderCallback -= ReorderFrameListItem;
			}

            Undo.undoRedoPerformed -= OnUndoOrRedo;
        }

        /// <summary>
        /// Reinitialize the frame list on undo/redo
        /// </summary>
        private void OnUndoOrRedo()
        {
            if (selectedAnimation != null)
            {
                InitializeReorderableList();
                Repaint();
            }
        }

        private void OnSelectionChange()
        {
            // Change animation if we select an animation on the project
            if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(SpriteAnimation))
            {
                SpriteAnimation sa = Selection.activeObject as SpriteAnimation;
                if (sa != selectedAnimation)
                {
                    selectedAnimation = sa;
                    spritePreview = null;
                    InitializeReorderableList();
                    Repaint();
                }
            }
        }

        private void Update()
        {
            // Only force repaint on update if the preview is playing and has changed the frame
            if (spritePreview != null && 
               (spritePreview.IsPlaying || spritePreview.IsPanning) &&
                spritePreview.ForceRepaint)
            {
                spritePreview.ForceRepaint = false;
                Repaint();
            }
        }

        private void OnGUI()
        {
            // Style initialization
            if (!init)
            {
                Initialize();
                init = true;
            }

            // Create animation box
            NewAnimationBox();

            if(justCreatedAnim)
            {
                justCreatedAnim = false;
                return;
            }

            // Edit animation box
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            {
                // Animation asset field
                if (selectedAnimation == null)
                {
                    EditorGUILayout.BeginVertical(box);
                    selectedAnimation = EditorGUILayout.ObjectField("Animation", selectedAnimation, typeof(SpriteAnimation), false) as SpriteAnimation;
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    // Init reorderable list
					if(frameList == null)
                        InitializeReorderableList();

                    // Add the frames dropped on the drag and drop box
                    if (draggedSprites != null && draggedSprites.Count > 0)
                    {
                        // TODO Record Undo/Redo for dragged sprites, currently not working, don't know why
                        //Undo.RecordObject(selectedAnimation, "Add Frames");

                        for (int i = 0; i < draggedSprites.Count; i++)
                            AddFrame(draggedSprites[i]);
                        draggedSprites.Clear();

                        SaveFile(true);
                    }

                    // Retrocompatibility check for the new frames duration field
                    if (selectedAnimation.FramesCount != selectedAnimation.FramesDuration.Count)
                    {
                        selectedAnimation.FramesDuration.Clear();
                        for (int i = 0; i < selectedAnimation.FramesCount; i++)
                            selectedAnimation.FramesDuration.Add(1);
                    }

					// Config settings
					ConfigBox();

                    EditorGUILayout.Space();

                    EditorGUILayout.BeginHorizontal();
					{
                        // Preview window setup
						Rect previewRect = EditorGUILayout.BeginVertical(lowPaddingBox, GUILayout.MaxWidth(position.width / 2));
                        PreviewBox(previewRect);
						EditorGUILayout.EndVertical();

                        EditorGUILayout.BeginVertical();
                        {
                            // FPS 
                            int fps = selectedAnimation.FPS;
                            EditorGUI.BeginChangeCheck();
                            {
                                fps = EditorGUILayout.IntField("FPS", selectedAnimation.FPS);
                            }
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(selectedAnimation, "Change FPS");
                                selectedAnimation.FPS = fps;
                                if (selectedAnimation.FPS < 0)
                                    selectedAnimation.FPS = 0;
                            }

                            EditorGUILayout.Space();

                            scrollWindowPosition = EditorGUILayout.BeginScrollView(scrollWindowPosition);
                            {
                                // Individual frames
                                frameList.displayRemove = (selectedAnimation.FramesCount > 0);
                                frameList.DoLayoutList();
                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.EndScrollView();

                            EditorGUILayout.Space();
                        }
                        EditorGUILayout.EndVertical();

                        // Check Events
                        Event evt = Event.current;
                        switch (evt.type)
                        {
                            // Delete frames with supr
                            case EventType.KeyDown:
                                if (Event.current.keyCode == KeyCode.Delete &&
                                    selectedAnimation.FramesCount > 0 &&
                                    frameList.HasKeyboardControl() &&
                                    frameListSelectedIndex != -1)
                                {
                                    RemoveFrameListItem(frameList);
                                }
                                break;
                            // Zoom preview window with scrollwheel
                            case EventType.ScrollWheel:
                                if (spritePreview != null)
                                {
                                    Vector2 mpos = Event.current.mousePosition;
                                    if (mpos.x >= previewRect.x && mpos.x <= previewRect.x + previewRect.width &&
                                        mpos.y >= previewRect.y && mpos.y <= previewRect.y + previewRect.height)
                                    {
                                        Repaint();
                                        spritePreview.Zoom = -evt.delta.y;
                                    }
                                }
                                break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
  
            if (GUI.changed && selectedAnimation != null)
            {
                SaveFile();
            }
        }

        private void Initialize()
        {
            minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
            buttonStyle = new GUIStyle("preButton");
            sliderStyle = new GUIStyle("preSlider");
            sliderThumbStyle = new GUIStyle("preSliderThumb");
            labelStyle = new GUIStyle("preLabel");
            box = new GUIStyle(EditorStyles.helpBox);
            playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            speedScaleIcon = EditorGUIUtility.IconContent("SpeedScale");
            loopIcon = EditorGUIUtility.IconContent("RotateTool");
            loopIconActive = EditorGUIUtility.IconContent("RotateTool On");
            lowPaddingBox = new GUIStyle(EditorStyles.helpBox);
            lowPaddingBox.padding = new RectOffset(1, 1, 1, 1);
			lowPaddingBox.stretchWidth = true;
			lowPaddingBox.stretchHeight = true;
            previewToolBar = new GUIStyle("RectangleToolHBar");
            preview = new GUIStyle("CurveEditorBackground");

			dragAndDropBox = new GUIStyle(EditorStyles.helpBox);
			dragAndDropBox.richText = true;
            dragAndDropBox.alignment = TextAnchor.MiddleCenter;
        }

        private void InitializeReorderableList()
        {
            if (frames == null)
                frames = new List<AnimationFrame>();

            frames.Clear();

            if (selectedAnimation == null)
                return;

            for (int i = 0; i < selectedAnimation.FramesCount; i++)
                frames.Add(new AnimationFrame(selectedAnimation.Frames[i], selectedAnimation.FramesDuration[i]));

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

        /// <summary>
        /// Draws the new animation box
        /// </summary>
        private void NewAnimationBox()
        {
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                GUILayout.FlexibleSpace();

                // New animaton button
                if (GUILayout.Button("Create Animation", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                {
                    CreateAnimation();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the box with the name and file of the animation
        /// </summary>
        private void ConfigBox()
        {
            EditorGUILayout.BeginVertical(box);
            {
                SpriteAnimation newSpriteAnimation = EditorGUILayout.ObjectField("Animation", selectedAnimation, typeof(SpriteAnimation), false) as SpriteAnimation; 
                if (newSpriteAnimation == null)
                    return;

                // Reset preview and list if we select a new animation
                if (newSpriteAnimation != selectedAnimation)
                {
                    selectedAnimation = newSpriteAnimation;
                    InitializeReorderableList();
                    spritePreview = (EditorPreviewSpriteAnimation)Editor.CreateEditor(selectedAnimation, typeof(EditorPreviewSpriteAnimation));
                }

                EditorGUILayout.Space();
                DragAndDropBox();
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws the drag and drop box and saves the dragged objects
        /// </summary>
        private void DragAndDropBox()
        {
            // Drag and drop box for sprite frames
			Rect dropArea = GUILayoutUtility.GetRect(0f, DROP_AREA_HEIGHT, GUILayout.ExpandWidth(true));
            Event evt = Event.current;
			GUI.Box(dropArea, "Drop sprites <b>HERE</b> to add frames automatically.", dragAndDropBox);
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
							draggedSprites.Clear();
                            foreach (Object draggedObject in DragAndDrop.objectReferences)
                            {
                                // Get dragged sprites
                                Sprite s = draggedObject as Sprite;
                                if (s != null)
                                    draggedSprites.Add(s);
                                else
                                {
                                    // If the object is a complete texture, get all the sprites in it
                                    Texture2D t = draggedObject as Texture2D;
                                    if (t != null)
                                    {
                                        string texturePath = AssetDatabase.GetAssetPath(t);
                                        Sprite[] spritesInTexture = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();
                                        for (int i = 0; i < spritesInTexture.Length; i++)
                                            draggedSprites.Add(spritesInTexture[i]);
                                    }
                                }
                            }

                            if (DragAndDrop.objectReferences.Length > 1)
                            {
                                draggedSprites.Sort(new SpriteSorter());
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Draws the preview window
        /// </summary>
        /// <param name="r">Draw rect</param>
        private void PreviewBox(Rect r)
        {
            if (spritePreview == null || spritePreview.CurrentAnimation != selectedAnimation)
                spritePreview = (EditorPreviewSpriteAnimation)Editor.CreateEditor(selectedAnimation, typeof(EditorPreviewSpriteAnimation));

            if (spritePreview != null)
            {
				EditorGUILayout.BeginVertical(preview, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                {
                    r.height -= 21;
                    r.width -= 2;
                    r.y += 1;
                    r.x += 1;
                    spritePreview.OnInteractivePreviewGUI(r, EditorStyles.whiteLabel);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal(previewToolBar);
                {
                    // Play Button
                    GUIContent buttonContent = spritePreview.IsPlaying ? pauseButtonContent : playButtonContent;
                    spritePreview.IsPlaying = GUILayout.Toggle(spritePreview.IsPlaying, buttonContent, buttonStyle, GUILayout.Width(40));

                    // Loop Button
                    GUIContent loopContent = spritePreview.Loop ? loopIconActive : loopIcon;
                    spritePreview.Loop = GUILayout.Toggle(spritePreview.Loop, loopContent, buttonStyle, GUILayout.Width(40));

                    // FPS Slider
                    GUILayout.Box(speedScaleIcon, labelStyle, GUILayout.ExpandWidth(false));
                    spritePreview.FramesPerSecond = (int)GUILayout.HorizontalSlider(spritePreview.FramesPerSecond, 0, 60, sliderStyle, sliderThumbStyle);
                    GUILayout.Label(spritePreview.FramesPerSecond.ToString("0") + " fps", labelStyle, GUILayout.Width(50));
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        #region Reorderable List Methods
        private void DrawFrameListHeader(Rect r)
        {
            GUI.Label(r, "Frame List");
        }

        private void DrawFrameListElement(Rect r, int i, bool active, bool focused)
        {
            EditorGUI.BeginChangeCheck();
            {
                string spriteName = (selectedAnimation.Frames[i] != null) ? selectedAnimation.Frames[i].name : "No sprite selected";
                EditorGUI.LabelField(new Rect(r.x, r.y + 2, r.width, r.height), spriteName);
                selectedAnimation.Frames[i] = EditorGUI.ObjectField(new Rect(r.x + r.width - 120, r.y + 1, 50, r.height - 4), "", selectedAnimation.Frames[i], typeof(Sprite), false) as Sprite;
                EditorGUIUtility.labelWidth = 20;
                selectedAnimation.FramesDuration[i] = EditorGUI.IntField(new Rect(r.x + r.width - 50, r.y + 1, 50, r.height - 4), speedScaleIcon, selectedAnimation.FramesDuration[i]);
            }
            if (EditorGUI.EndChangeCheck())
                SaveFile(true);
        }

        private void AddFrameListItem(ReorderableList list)
        {
            Undo.RecordObject(selectedAnimation, "Add Frame");
            AddFrame();
            SaveFile(true);
        }

        private void RemoveFrameListItem(ReorderableList list)
        {
            Undo.RecordObject(selectedAnimation, "Remove Frame");

            int i = list.index;
            selectedAnimation.Frames.RemoveAt(i);
            selectedAnimation.FramesDuration.RemoveAt(i);
            frameList.list.RemoveAt(i);
            frameListSelectedIndex = frameList.index;

            if (i >= selectedAnimation.FramesCount)
            {
                frameList.index -= 1;
                frameListSelectedIndex -= 1;
                spritePreview.CurrentFrame = frameListSelectedIndex;
                frameList.GrabKeyboardFocus();
            }

            Repaint();
            SaveFile(true);
        }

        private void ReorderFrameListItem(ReorderableList list)
        {
            Undo.RecordObject(selectedAnimation, "Reorder Frames");

            Sprite s = selectedAnimation.Frames[frameListSelectedIndex];
            selectedAnimation.Frames.RemoveAt(frameListSelectedIndex);
            selectedAnimation.Frames.Insert(list.index, s);

            int i = selectedAnimation.FramesDuration[frameListSelectedIndex];
            selectedAnimation.FramesDuration.RemoveAt(frameListSelectedIndex);
            selectedAnimation.FramesDuration.Insert(list.index, i);

            SaveFile(true);
        }

        private void SelectFrameListItem(ReorderableList list)
        {
            spritePreview.CurrentFrame = list.index;
            spritePreview.ForceRepaint = true;
            frameListSelectedIndex = list.index;
        }
        #endregion

        /// <summary>
        /// Adds an empty frame
        /// </summary>
        private void AddFrame()
        {
            frameList.list.Add(new AnimationFrame(null, 1));
            selectedAnimation.Frames.Add(null);
            selectedAnimation.FramesDuration.Add(1);
        }

        /// <summary>
        /// Adds a frame with specified sprite
        /// </summary>
        /// <param name="s">Sprite to add</param>
        private void AddFrame(Sprite s)
        {
            frameList.list.Add(new AnimationFrame(s, 1));
            selectedAnimation.Frames.Add(s);
            selectedAnimation.FramesDuration.Add(1);
        }

        /// <summary>
        /// Creates the animation asset with a prompt
        /// </summary>
        private void CreateAnimation()
        {
            string folder = EditorUtility.SaveFilePanel("New Animation", "Assets", "New Animation", "asset");
            string relativeFolder = folder;

            if (folder.Length > 0)
            {
                int folderPosition = folder.IndexOf("Assets/", System.StringComparison.InvariantCulture);
                if (folderPosition > 0)
                {
                    relativeFolder = folder.Substring(folderPosition);

                    // Create the animation
                    SpriteAnimation asset = CreateInstance<SpriteAnimation>(); 
                    AssetDatabase.CreateAsset(asset, relativeFolder);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    selectedAnimation = AssetDatabase.LoadAssetAtPath<SpriteAnimation>(relativeFolder);
                    InitializeReorderableList();
                    justCreatedAnim = true;
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Select a path inside the Assets folder", "OK");
                }
            }
        }

        /// <summary>
        /// Forces serialization of the current animation
        /// </summary>
        /// <param name="toDisk">If true, it forces the asset database to save the file to disk. It causes little freeze, so I only use it on a few moments. Remember to save project before closing Unity!!</param>
        private void SaveFile(bool toDisk = false)
        {
            EditorUtility.SetDirty(selectedAnimation);

            if(toDisk)
                AssetDatabase.SaveAssets();
        }
    }
}