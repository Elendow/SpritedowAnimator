// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EditorSpriteAnimation : EditorWindow {

    private const float FRAME_WIDTH = 70f;
    private const float FRAME_HEIGHT = 70f;
    private const float FRAME_OFFSET = 10f;

    private string newAnimName = "Animation";

    private Texture2D clockIcon = null;
    private SpriteAnimation selectedAnimation;
    private Vector2 pos = Vector2.zero;
    private List<Sprite> draggedSprites;

	[MenuItem ("Elendow Tools/Sprite Animation Editor", false, 0)]
    private static void ShowWindow(){
		GetWindow(typeof(EditorSpriteAnimation), false, "Sprite Animation");
	}

	private void Update()
	{
        // Change animation if we select an animation on the project
        if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(SpriteAnimation))
		{
			selectedAnimation = Selection.activeObject as SpriteAnimation;
			Repaint();
		}

        // Add the frames dropped on the drag and drop box
        if(draggedSprites != null && draggedSprites.Count > 0)
        {
            for(int i = 0; i < draggedSprites.Count; i++)
            {
                AddFrame(draggedSprites[i]);
            }
            draggedSprites = null;
        }
	}

	private void OnGUI()
    {
        // Get clock icon
        if (clockIcon == null)
            clockIcon = Resources.Load<Texture2D>("clockIcon");

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
                Event evt = Event.current;
                Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
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
                            DragAndDrop.AcceptDrag();
                            draggedSprites = new List<Sprite>();
                            foreach (Sprite draggedObject in DragAndDrop.objectReferences)
                            {
                                draggedSprites.Add(draggedObject);
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
					pos = EditorGUILayout.BeginScrollView(pos);
					{
						EditorGUILayout.BeginHorizontal();
						int j = 0;
						for(int i = 0; i < selectedAnimation.FramesCount; i++)
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

		if(GUI.changed && selectedAnimation != null)
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
        int folderPosition = folder.IndexOf("Assets/");
        if (folderPosition > 0)
        {
            relativeFolder = folder.Substring(folderPosition);
            relativeFolder += "/";
        }
        else
            relativeFolder = "Assets/";
		AssetDatabase.CreateAsset(asset, relativeFolder + newAnimName + ".asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Selection.activeObject 	= asset;
		selectedAnimation = asset;
		EditorUtility.DisplayDialog("Sprite Animation Created", "Sprite Animation saved to " + relativeFolder + newAnimName, "OK");
	}
}