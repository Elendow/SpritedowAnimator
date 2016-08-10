// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EditorSpriteAnimation : EditorWindow {

    private float _frameWidth = 70f;
    private float _frameHeight = 70f;
    private float _frameOffset = 10f;

    private string _newAnimName = "Animation";

    private Texture2D _clockIcon = null;
    private SpriteAnimation _selectedAnimation;
    private Vector2 _pos = Vector2.zero;
    private List<Sprite> _draggedSprites;

	[MenuItem ("Elendow Tools/Sprite Animation Editor", false, 0)]
    private static void ShowWindow(){
		GetWindow(typeof(EditorSpriteAnimation), false, "Sprite Animation");
	}

	private void Update()
	{
        // Change animation if we select an animation on the project
        if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(SpriteAnimation))
		{
			_selectedAnimation = Selection.activeObject as SpriteAnimation;
			Repaint();
		}

        // Add the frames dropped on the drag and drop box
        if(_draggedSprites != null && _draggedSprites.Count > 0)
        {
            for(int i = 0; i < _draggedSprites.Count; i++)
            {
                AddFrame(_draggedSprites[i]);
            }
            _draggedSprites = null;
        }
	}

	private void OnGUI()
    {
        // Get clock icon
        if (_clockIcon == null)
            _clockIcon = Resources.Load<Texture2D>("clockIcon");

        // Create animation box
        EditorGUILayout.Space();
		EditorGUILayout.LabelField("Create Animation");
		EditorGUILayout.BeginHorizontal("box");

        // New animation name field
        _newAnimName = EditorGUILayout.TextField("Name", _newAnimName);

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
            _selectedAnimation = EditorGUILayout.ObjectField(_selectedAnimation, typeof(SpriteAnimation), false) as SpriteAnimation;

            EditorGUILayout.Space();
            if (_selectedAnimation != null)
			{
                // Retrocompatibility check for the new frames duration field
                if (_selectedAnimation.FramesCount != _selectedAnimation.FramesDuration.Count)
                {
                    _selectedAnimation.FramesDuration.Clear();

                    for (int i = 0; i < _selectedAnimation.FramesCount; i++)
                        _selectedAnimation.FramesDuration.Add(1);
                }

                // Name field
                _selectedAnimation.Name = EditorGUILayout.TextField("Name", _selectedAnimation.Name);
              
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
                            _draggedSprites = new List<Sprite>();
                            foreach (Sprite draggedObject in DragAndDrop.objectReferences)
                            {
                                _draggedSprites.Add(draggedObject);
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
                if (_selectedAnimation.FramesCount > 0)
				{
					List<int> remove = new List<int>();
					_pos = EditorGUILayout.BeginScrollView(_pos);
					{
						EditorGUILayout.BeginHorizontal();
						int j = 0;
						for(int i = 0; i < _selectedAnimation.FramesCount; i++)
						{
							if((j+1) * (_frameWidth + _frameOffset) > EditorGUIUtility.currentViewWidth)
							{
								EditorGUILayout.EndHorizontal();
								EditorGUILayout.BeginHorizontal();
								j = 0;
							}
							EditorGUILayout.BeginVertical(GUILayout.Width(_frameWidth));
							{
                                // Frame Sprite field
								_selectedAnimation.Frames[i] = EditorGUILayout.ObjectField(_selectedAnimation.Frames[i], typeof(Sprite), false, GUILayout.Width(_frameWidth), GUILayout.Height(_frameHeight)) as Sprite;

                                // Frames duration field
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Label(_clockIcon);
                                _selectedAnimation.FramesDuration[i] = EditorGUILayout.IntField(_selectedAnimation.FramesDuration[i], GUILayout.Width(_frameWidth - 20));
                                if (_selectedAnimation.FramesDuration[i] <= 0) _selectedAnimation.FramesDuration[i] = 1;
                                EditorGUILayout.EndHorizontal();

                                // Remove button for individual frame
                                if (GUILayout.Button("Remove", GUILayout.Width(_frameWidth)))
									remove.Add(i);
							}
							EditorGUILayout.EndVertical();
							j++;
						}
                        // Remove the previously selected frames
                        for (int i = 0; i < remove.Count; i++)
                        {
                            _selectedAnimation.Frames.RemoveAt(remove[i]);
                            _selectedAnimation.FramesDuration.RemoveAt(remove[i]);
                        }
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndScrollView();
				}
			}
		}
		EditorGUILayout.EndVertical();

		if(GUI.changed && _selectedAnimation != null)
			EditorUtility.SetDirty(_selectedAnimation); 
	}

    private void AddFrame()
    {
        _selectedAnimation.Frames.Add(new Sprite());
        _selectedAnimation.FramesDuration.Add(1);
    }

    private void AddFrame(Sprite s)
    {
        AddFrame();
        _selectedAnimation.Frames[_selectedAnimation.Frames.Count - 1] = s;
    }

    private void CreateAnimation(string folder){
		SpriteAnimation asset = CreateInstance<SpriteAnimation>();
		string relativeFolder = "";
		asset.Name = _newAnimName;
        int folderPosition = folder.IndexOf("Assets/");
        if (folderPosition > 0)
        {
            relativeFolder = folder.Substring(folderPosition);
            relativeFolder += "/";
        }
        else
            relativeFolder = "Assets/";
		AssetDatabase.CreateAsset(asset, relativeFolder + _newAnimName + ".asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Selection.activeObject 	= asset;
		_selectedAnimation = asset;
		EditorUtility.DisplayDialog("Sprite Animation Created", "Sprite Animation saved to " + relativeFolder + _newAnimName, "OK");
	}
}