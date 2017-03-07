// Spritedow Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/SpritedowAnimator
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
        private int frameListSelectedIndex = -1;
        private string newAnimName = "New Animation";
        private Texture2D clockIcon = null;
        private SpriteAnimation selectedAnimation = null;
        private Vector2 scrollWindowPosition = Vector2.zero;
        private List<Sprite> draggedSprites = null;
        private EditorPreviewSpriteAnimation spritePreview = null;
		private ReorderableList frameList;
		private List<AnimationFrame> frames;

        // Styles
        private GUIStyle box;
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

        [MenuItem("Elendow Tools/Sprite Animation Editor", false, 0)]
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
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
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
            InitializeReorderableList();
            Repaint();
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
            if (spritePreview != null && spritePreview.IsPlaying && spritePreview.ForceRepaint)
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
            EditorGUILayout.Space();
            NewAnimationBox();

            // Edit animation box
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Edit Animation");
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
                    // TODO Record Undo/Redo for dragged sprites, currently not working, don't know why
                    if (draggedSprites != null && draggedSprites.Count > 0)
                    {
                        for (int i = 0; i < draggedSprites.Count; i++)
                            AddFrame(draggedSprites[i]);
                        draggedSprites.Clear();
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
                            selectedAnimation.FPS = EditorGUILayout.IntField("FPS", selectedAnimation.FPS);
                            if (selectedAnimation.FPS < 0) selectedAnimation.FPS = 0;

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
                                    RemoveFrameListItem(frameList);
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
                EditorUtility.SetDirty(selectedAnimation);
        }

        private void Initialize()
        {
            minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
            buttonStyle = new GUIStyle("preButton");
            sliderStyle = new GUIStyle("preSlider");
            sliderThumbStyle = new GUIStyle("preSliderThumb");
            labelStyle = new GUIStyle("preLabel");
            box = new GUIStyle("box");
            playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            speedScaleIcon = EditorGUIUtility.IconContent("SpeedScale");
            loopIcon = EditorGUIUtility.IconContent("RotateTool");
            loopIconActive = EditorGUIUtility.IconContent("RotateTool On");
            lowPaddingBox = new GUIStyle("box");
            lowPaddingBox.padding = new RectOffset(1, 1, 1, 1);
			lowPaddingBox.stretchWidth = true;
			lowPaddingBox.stretchHeight = true;
            previewToolBar = new GUIStyle("RectangleToolHBar");
            preview = new GUIStyle("CurveEditorBackground");
        }

        private void InitializeReorderableList()
        {
            if (frames == null)
                frames = new List<AnimationFrame>();

            frames.Clear();
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
            EditorGUILayout.LabelField("Create Animation");
			EditorGUILayout.BeginHorizontal(box);
            {
                // New animation name field
                newAnimName = EditorGUILayout.TextField("Name", newAnimName);

                // New animaton button
                if (GUILayout.Button("New Animation"))
                {
                    string folder = EditorUtility.OpenFolderPanel("New Animation", "Assets", "");
                    if (folder != "")
                    {
                        EditorGUILayout.BeginHorizontal();
                        CreateAnimation(folder);
                    }
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
                if(newSpriteAnimation != selectedAnimation)
                {
                    selectedAnimation = newSpriteAnimation;
                    InitializeReorderableList();
                    spritePreview = (EditorPreviewSpriteAnimation)Editor.CreateEditor(selectedAnimation, typeof(EditorPreviewSpriteAnimation));
                }

                // Name field
                selectedAnimation.Name = EditorGUILayout.TextField("Name", selectedAnimation.Name);
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
            GUI.Box(dropArea, "\nDrop sprites to add frames automatically.", box);
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
            if (i < selectedAnimation.FramesCount)
            {
                EditorGUI.BeginChangeCheck();
                {
                    string spriteName = (selectedAnimation.Frames[i] != null) ? selectedAnimation.Frames[i].name : "No sprite selected";
                    EditorGUIUtility.labelWidth = r.width - 105;
                    selectedAnimation.Frames[i] = EditorGUI.ObjectField(new Rect(r.x + 10, r.y + 1, r.width - 85, r.height - 4), spriteName, selectedAnimation.Frames[i], typeof(Sprite), false) as Sprite;

                    EditorGUIUtility.labelWidth = 20;
                    selectedAnimation.FramesDuration[i] = EditorGUI.IntField(new Rect(r.x + r.width - 50, r.y + 1, 50, r.height - 4), speedScaleIcon, selectedAnimation.FramesDuration[i]);
                }
                if (EditorGUI.EndChangeCheck())
                    EditorUtility.SetDirty(selectedAnimation);
            }
        }

        private void AddFrameListItem(ReorderableList list)
        {
            Undo.RecordObject(selectedAnimation, "Add Frame");
            AddFrame();
            EditorUtility.SetDirty(selectedAnimation);
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
                frameList.index = -1;
                frameListSelectedIndex = -1;
                frameList.ReleaseKeyboardFocus();
            }

            EditorUtility.SetDirty(selectedAnimation);
            Repaint();
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

            EditorUtility.SetDirty(selectedAnimation);
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
            AddFrame();
            selectedAnimation.Frames[selectedAnimation.Frames.Count - 1] = s;
            frameList.list[selectedAnimation.Frames.Count - 1] = new AnimationFrame(s, 1);
        }

        /// <summary>
        /// Creates the animation asset
        /// </summary>
        /// <param name="folder">Folder for the asset</param>
        private void CreateAnimation(string folder)
        {
            SpriteAnimation asset = CreateInstance<SpriteAnimation>();
            string relativeFolder = "";
            asset.Name = newAnimName;

            // Get path relative to assets folder
            int folderPosition = folder.IndexOf("Assets/");
            if (folderPosition > 0)
            {
                relativeFolder = folder.Substring(folderPosition);
                relativeFolder += "/";
            }
            else
                relativeFolder = "Assets/";
            // Check if animation already exists
            if (AssetDatabase.LoadAssetAtPath<SpriteAnimation>(relativeFolder + newAnimName + ".asset") != null)
            {
                if (!EditorUtility.DisplayDialog("Sprite Animation Already Exist", "An Sprite Animation already exist on that folder with that name.\n Do you want to overwrite it?", "Yes", "No"))
                    return;
            }

            // Create the animation
            AssetDatabase.CreateAsset(asset, relativeFolder + newAnimName + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
            selectedAnimation = asset;
            InitializeReorderableList();
            EditorUtility.DisplayDialog("Sprite Animation Created", "Sprite Animation saved to " + relativeFolder + newAnimName, "OK");
        }
    }
}