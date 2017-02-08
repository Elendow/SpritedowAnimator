// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor window to edit animations
/// </summary>
public class EditorSpriteAnimation : EditorWindow
{
    private const int ITEMS_PER_PAGE = 40;
    private const float CONFIG_BOX_HEIGHT = 120;
    private const float DROP_AREA_HEIGHT = 50;
    private const float PREVIEW_WINDOW_WIDTH = 100;
    private const float FRAME_WIDTH = 65f;
    private const float FRAME_HEIGHT = 65f;
    private const float FRAME_OFFSET = 10f;
    private const float MIN_WINDOW_WIDTH = 500f;
    private const float MIN_WINDOW_HEIGHT = 200f;

    private string newAnimName = "Animation";

    private bool init = false;
    private int currentPage = 0;
    private Texture2D clockIcon = null;
    private SpriteAnimation selectedAnimation;
    private Vector2 pos = Vector2.zero;
    private List<Sprite> draggedSprites;
	private SpriteAnimationEditor spritePreview;

    // Styles
    private GUIStyle box;
    private GUIStyle buttonStyle;
    private GUIStyle sliderStyle;
    private GUIStyle sliderThumbStyle;
    private GUIStyle labelStyle;
    private GUIContent playButtonContent;
    private GUIContent pauseButtonContent;
    private GUIContent speedScale;

    [MenuItem("Elendow Tools/Sprite Animation Editor", false, 0)]
    private static void ShowWindow()
    {
        GetWindow(typeof(EditorSpriteAnimation), false, "Sprite Animation");
    }

    private void OnEnable()
    {
        // Get clock icon
        if (clockIcon == null)
            clockIcon = Resources.Load<Texture2D>("clockIcon");

        EditorApplication.update += Update;

		// Initialize
        draggedSprites = new List<Sprite>();
        init = false;
    }

	private void OnDisable()
	{
		EditorApplication.update -= Update;
	}

    private void OnSelectionChange()
    {
        // Change animation if we select an animation on the project
        if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(SpriteAnimation))
        {
            SpriteAnimation sa = Selection.activeObject as SpriteAnimation;
            if (sa != selectedAnimation)
            {
                currentPage = 0;
                selectedAnimation = sa;
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
            minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
            buttonStyle = new GUIStyle("preButton");
            sliderStyle = new GUIStyle("preSlider");
            sliderThumbStyle = new GUIStyle("preSliderThumb");
            labelStyle = new GUIStyle("preLabel");
            box = new GUIStyle("box");
            playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            speedScale = EditorGUIUtility.IconContent("SpeedScale");
            init = true;
        }

        // Create animation box
        EditorGUILayout.Space();
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
        EditorGUILayout.Space();

		// Edit animation box
		EditorGUILayout.LabelField("Edit Animation");

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUILayout.BeginVertical();
	        {
                // Animation asset field
                if (selectedAnimation == null)
                {
                    EditorGUILayout.BeginVertical(box);
                    {
                        selectedAnimation = EditorGUILayout.ObjectField("Animation", selectedAnimation, typeof(SpriteAnimation), false) as SpriteAnimation;
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    // Add the frames dropped on the drag and drop box
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

                    EditorGUILayout.BeginHorizontal();
                    {
                        // Config settings
                        EditorGUILayout.BeginVertical(box, GUILayout.Height(CONFIG_BOX_HEIGHT));
                        {
                            selectedAnimation = EditorGUILayout.ObjectField("Animation", selectedAnimation, typeof(SpriteAnimation), false) as SpriteAnimation;
                            if (selectedAnimation == null)
                                return;

                            // Name field
                            selectedAnimation.Name = EditorGUILayout.TextField("Name", selectedAnimation.Name);

                            EditorGUILayout.Space();

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

                            // Manually add empty frames
                            if (GUILayout.Button("Add Frame"))
                                AddFrame();
                        }
                        EditorGUILayout.EndVertical();

                        // Preview window setup
                        GUIStyle previewWindow = new GUIStyle("box");
                        previewWindow.padding = new RectOffset(1, 1, 1, 1);
                        Rect previewRect = EditorGUILayout.BeginVertical(previewWindow, GUILayout.Width(PREVIEW_WINDOW_WIDTH), GUILayout.Height(CONFIG_BOX_HEIGHT));
                        {
                            PreviewWindow(previewRect);
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    // Individual frames
                    if (selectedAnimation.FramesCount > 0)
                    {
                        List<int> remove = new List<int>();

                        // Pagination for very big animations in order to avoid GC Alloc
                        if (selectedAnimation.FramesCount > ITEMS_PER_PAGE)
                        {
                            int pages = selectedAnimation.FramesCount / ITEMS_PER_PAGE;
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Pages", GUILayout.Width(FRAME_WIDTH));
                            string[] pageNames = new string[pages + 1];
                            for (int i = 0; i <= pages; i++)
                                pageNames[i] = (i + 1).ToString();
                            currentPage = GUILayout.Toolbar(currentPage, pageNames);
                            EditorGUILayout.EndHorizontal();
                            int maxValue = Mathf.Min((currentPage * ITEMS_PER_PAGE) + ITEMS_PER_PAGE, selectedAnimation.FramesCount);
                            EditorGUILayout.LabelField("Showing elements from " + ((currentPage * ITEMS_PER_PAGE)) + " to " + (maxValue - 1));
                            EditorGUILayout.Space();

                            if (currentPage > pages)
                                currentPage = pages + 1;
                        }
                        else
                            currentPage = 0;

                        pos = EditorGUILayout.BeginScrollView(pos, box);
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                            int j = 0;
                            for (int i = currentPage * ITEMS_PER_PAGE; i < (currentPage * ITEMS_PER_PAGE) + ITEMS_PER_PAGE && i < selectedAnimation.FramesCount; i++)
                            {
                                if ((j + 1) * (FRAME_WIDTH + FRAME_OFFSET) > EditorGUIUtility.currentViewWidth)
                                {
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.BeginHorizontal();
                                    j = 0;
                                }
                                EditorGUILayout.BeginVertical(GUILayout.Width(FRAME_WIDTH));
                                {
                                    // Frame Sprite field
                                    selectedAnimation.Frames[i] = EditorGUILayout.ObjectField(selectedAnimation.Frames[i], typeof(Sprite), false, GUILayout.Width(FRAME_WIDTH), GUILayout.Height(FRAME_HEIGHT)) as Sprite;

                                    // Frames duration field
                                    EditorGUILayout.BeginHorizontal();
                                    GUILayout.Label(clockIcon);
                                    selectedAnimation.FramesDuration[i] = EditorGUILayout.IntField(selectedAnimation.FramesDuration[i], GUILayout.Width(FRAME_WIDTH - 20));
                                    if (selectedAnimation.FramesDuration[i] <= 0) selectedAnimation.FramesDuration[i] = 1;
                                    EditorGUILayout.EndHorizontal();

                                    // Remove button for individual frame
                                    if (GUILayout.Button("Remove", GUILayout.Width(FRAME_WIDTH)))
                                        remove.Add(i);
                                }
                                EditorGUILayout.EndVertical();
                                j++;
                            }
                            // Remove the previously selected frames
                            for (int i = 0; i < remove.Count; i++)
                            {
                                selectedAnimation.Frames.RemoveAt(remove[i]);
                                selectedAnimation.FramesDuration.RemoveAt(remove[i]);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndScrollView();
                    }
                }
	        }
	        EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndHorizontal();

        if (GUI.changed && selectedAnimation != null)
        {
            EditorUtility.SetDirty(selectedAnimation);
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// Draws the preview window
    /// </summary>
    /// <param name="r">Draw rect</param>
	private void PreviewWindow(Rect r)
	{
		if (spritePreview == null || spritePreview.CurrentAnimation != selectedAnimation)
			spritePreview = (SpriteAnimationEditor)Editor.CreateEditor(selectedAnimation, typeof(SpriteAnimationEditor));

		if(spritePreview != null)
		{
			EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
			{
				r.height -= 21;
                r.width -= 2;
				r.y += 1;
                r.x += 1;
				spritePreview.OnInteractivePreviewGUI(r, EditorStyles.whiteLabel);
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.BeginHorizontal(new GUIStyle("ProjectBrowserPreviewBg"), GUILayout.Height(10));
			{
				// Play Button
				GUIContent buttonContent = spritePreview.IsPlaying ? pauseButtonContent : playButtonContent;
				spritePreview.IsPlaying = GUILayout.Toggle(spritePreview.IsPlaying, buttonContent, buttonStyle);	

				// FPS Slider
				GUILayout.Box(speedScale, labelStyle);
				spritePreview.FramesPerSecond = (int)GUILayout.HorizontalSlider(spritePreview.FramesPerSecond, 0, 60, sliderStyle, sliderThumbStyle);
				GUILayout.Label(spritePreview.FramesPerSecond.ToString("0") + " fps", labelStyle, GUILayout.Width(50));
			}
			EditorGUILayout.EndHorizontal();
		}
	}

    /// <summary>
    /// Adds an empty frame
    /// </summary>
    private void AddFrame()
    {
        selectedAnimation.Frames.Add(new Sprite());
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
    }

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
        EditorUtility.DisplayDialog("Sprite Animation Created", "Sprite Animation saved to " + relativeFolder + newAnimName, "OK");
    }
}
	
/// <summary>
/// Editor class to show the animation preview
/// </summary>
[CustomEditor(typeof(SpriteAnimation))]
public class SpriteAnimationEditor : Editor
{
    private bool init = false;
	private bool isPlaying = false;
    private bool forceRepaint = false;
	private int currentFrame = 0;
	private int framesPerSecond = 60;
	private int frameDurationCounter = 0;
	private float animationTimer = 0;
	private float lastFrameEditorTime = 0;
	private SpriteAnimation animation;

    // Styles
    private GUIStyle previewButtonSettingsStyle;
    private GUIStyle preSlider;
    private GUIStyle preSliderThumb;
    private GUIStyle preLabel;
    private GUIContent speedScale;
    private GUIContent playButtonContent;
    private GUIContent pauseButtonContent;

    private void OnEnable()
	{
		if(animation == null)
			animation = (SpriteAnimation)target;
		
		EditorApplication.update += Update;
        init = false;
    }

	private void Update()
	{
		if(animation == null || animation.FramesCount == 0)
			return;

		// Check if playing and use the editor time to change frames
		if(isPlaying)
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
		if (animation != null && animation.FramesCount > 0)
		{
			Texture2D texture = AssetPreview.GetAssetPreview(animation.Frames[currentFrame]);
			if(texture != null)
			{
				// Use the filtermode of the texture to preview the sprite as ingame
				texture.filterMode = animation.Frames[currentFrame].texture.filterMode;
				EditorGUI.DrawTextureTransparent(r, texture, ScaleMode.ScaleToFit);
			}
		}
	}

	public override GUIContent GetPreviewTitle ()
	{
		return new GUIContent ("Animation Preview");
	}

	public override void OnPreviewSettings()
	{
        if(!init)
        {
            // Define styles
            previewButtonSettingsStyle = new GUIStyle("preButton");
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
		isPlaying = GUILayout.Toggle(isPlaying, buttonContent, previewButtonSettingsStyle);	

		// FPS Slider
		GUILayout.Box(speedScale, preLabel);
		framesPerSecond = (int)GUILayout.HorizontalSlider(framesPerSecond, 0, 60, preSlider, preSliderThumb);
		GUILayout.Label(framesPerSecond.ToString("0") + " fps", preLabel, GUILayout.Width(50));
	}

	public override bool HasPreviewGUI ()
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
}
