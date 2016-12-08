// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class EditorSpriteAnimation : EditorWindow {

    private const int ITEMS_PER_PAGE = 40;
    private const float FRAME_WIDTH = 70f;
    private const float FRAME_HEIGHT = 70f;
    private const float FRAME_OFFSET = 10f;

    private string newAnimName = "Animation";

    private int currentPage = 0;
    private Texture2D clockIcon = null;
    private SpriteAnimation selectedAnimation;
    private Vector2 pos = Vector2.zero;
    private List<Sprite> draggedSprites;

	[MenuItem ("Elendow Tools/Sprite Animation Editor", false, 0)]
    private static void ShowWindow()
    {
		GetWindow(typeof(EditorSpriteAnimation), false, "Sprite Animation");
	}

    private void OnEnable()
    {
        draggedSprites = new List<Sprite>();
        minSize = new Vector2(350, 250);

        // Get clock icon
        if (clockIcon == null)
            clockIcon = Resources.Load<Texture2D>("clockIcon");
    }

    private void OnSelectionChange()
	{
        // Change animation if we select an animation on the project
        if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(SpriteAnimation))
		{
            SpriteAnimation sa = Selection.activeObject as SpriteAnimation;
            if(sa != selectedAnimation)
            {
                currentPage = 0;
                selectedAnimation = sa;
                Repaint();
            }
        }

        // Add the frames dropped on the drag and drop box
        if(draggedSprites != null && draggedSprites.Count > 0)
        {
            for(int i = 0; i < draggedSprites.Count; i++)
                AddFrame(draggedSprites[i]);
            draggedSprites.Clear();
        }
	}

	private void OnGUI()
    {
        // Create animation box
        EditorGUILayout.Space();
		EditorGUILayout.LabelField("Create Animation");
		EditorGUILayout.BeginHorizontal("box");

        // New animation name field
        newAnimName = EditorGUILayout.TextField("Name", newAnimName);

        // New animaton button
		if(GUILayout.Button("New Animation"))
		{
			string folder = EditorUtility.OpenFolderPanel("New Animation", "Assets", "");
			if(folder != "")
			{
				EditorGUILayout.BeginHorizontal();
                CreateAnimation(folder);
			}
		}
		EditorGUILayout.EndHorizontal();

        // Edit animation box
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Edit Animation");
		EditorGUILayout.BeginVertical("box");
		{
            EditorGUILayout.Space();
            // Animation asset field
            selectedAnimation = EditorGUILayout.ObjectField(selectedAnimation, typeof(SpriteAnimation), false) as SpriteAnimation;

            EditorGUILayout.Space();
            if (selectedAnimation != null)
			{
                // Retrocompatibility check for the new frames duration field
                if (selectedAnimation.FramesCount != selectedAnimation.FramesDuration.Count)
                {
                    selectedAnimation.FramesDuration.Clear();

                    for (int i = 0; i < selectedAnimation.FramesCount; i++)
                        selectedAnimation.FramesDuration.Add(1);
                }

                // Name field
                selectedAnimation.Name = EditorGUILayout.TextField("Name", selectedAnimation.Name);
              
                EditorGUILayout.Space();

                // Drag and drop box for sprite frames
                Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
                Event evt = Event.current;
                GUIStyle style = new GUIStyle("box");
                GUI.Box(dropArea, "\nDrop sprites to add frames automatically.", style);
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
                                    Sprite s = draggedObject as Sprite;
                                    if(s != null)
                                        draggedSprites.Add(s);
                                    else
                                    {
                                        Texture2D t = draggedObject as Texture2D;
                                        if(t != null)
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

                EditorGUILayout.Space();

                // Manually add empty frames
                if (GUILayout.Button("Add Frame"))
                    AddFrame();

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

                    pos = EditorGUILayout.BeginScrollView(pos);
					{
                        EditorGUILayout.BeginHorizontal();
						int j = 0;
						for(int i = currentPage * ITEMS_PER_PAGE; i < (currentPage * ITEMS_PER_PAGE) + ITEMS_PER_PAGE && i < selectedAnimation.FramesCount ; i++)
						{
							if((j+1) * (FRAME_WIDTH + FRAME_OFFSET) > EditorGUIUtility.currentViewWidth)
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

        if (GUI.changed && selectedAnimation != null)
			EditorUtility.SetDirty(selectedAnimation); 
	}

    private void AddFrame()
    {
        selectedAnimation.Frames.Add(new Sprite());
        selectedAnimation.FramesDuration.Add(1);
    }

    private void AddFrame(Sprite s)
    {
        AddFrame();
        selectedAnimation.Frames[selectedAnimation.Frames.Count - 1] = s;
    }

    private void CreateAnimation(string folder){
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
        if(AssetDatabase.LoadAssetAtPath<SpriteAnimation>(relativeFolder + newAnimName + ".asset") != null)
        {
            if (!EditorUtility.DisplayDialog("Sprite Animation Already Exist", "An Sprite Animation already exist on that folder with that name.\n Do you want to overwrite it?", "Yes", "No"))
                return;
        }

        // Create the animation
		AssetDatabase.CreateAsset(asset, relativeFolder + newAnimName + ".asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Selection.activeObject 	= asset;
		selectedAnimation = asset;
		EditorUtility.DisplayDialog("Sprite Animation Created", "Sprite Animation saved to " + relativeFolder + newAnimName, "OK");
	}
}