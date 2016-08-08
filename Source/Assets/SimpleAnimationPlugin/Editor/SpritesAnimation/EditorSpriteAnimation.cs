// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EditorSpriteAnimation : EditorWindow {

	private SpriteAnimation _selectedAnimation;
	private string _animName = "Animation";
	private Vector2 _pos = Vector2.zero;
    private List<Sprite> _draggedSprites;

	[MenuItem ("Elendow Tools/Sprite Animation Editor", false, 0)]
    private static void ShowWindow(){
		GetWindow(typeof(EditorSpriteAnimation), false, "Sprite Animation");
	}

	private void Update()
	{
		if(Selection.activeObject != null && Selection.activeObject.GetType() == typeof(SpriteAnimation))
		{
			_selectedAnimation = Selection.activeObject as SpriteAnimation;
			Repaint();
		}

        if(_draggedSprites != null && _draggedSprites.Count > 0)
        {
            for(int i = 0; i < _draggedSprites.Count; i++)
            {
                AddFrame(_draggedSprites[i]);
            }
            _draggedSprites = null;
        }
	}

	private void OnGUI(){	
		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Create Animation");
		EditorGUILayout.BeginHorizontal("box");
		_animName = EditorGUILayout.TextField("Name", _animName);
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

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Edit Animation");
		EditorGUILayout.BeginVertical("box");
		{
			_selectedAnimation = EditorGUILayout.ObjectField(_selectedAnimation, typeof(SpriteAnimation), false) as SpriteAnimation;
			if(_selectedAnimation != null)
			{
				_selectedAnimation.Name = EditorGUILayout.TextField("Name", _selectedAnimation.Name);

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

                if (GUILayout.Button("Add Frame"))
                    AddFrame();

				if(_selectedAnimation.FramesCount > 0)
				{
					List<int> remove = new List<int>();
					_pos = EditorGUILayout.BeginScrollView(_pos);
					{
						EditorGUILayout.BeginHorizontal();
						int j = 0;
						for(int i = 0; i < _selectedAnimation.FramesCount; i++)
						{
							if((j+1) * 80 > EditorGUIUtility.currentViewWidth)
							{
								EditorGUILayout.EndHorizontal();
								EditorGUILayout.BeginHorizontal();
								j = 0;
							}
							EditorGUILayout.BeginVertical(GUILayout.Width(70f));
							{
								_selectedAnimation.Frames[i] = EditorGUILayout.ObjectField(_selectedAnimation.Frames[i], typeof(Sprite), false, GUILayout.Width(70f), GUILayout.Height(70f)) as Sprite;
								if(GUILayout.Button("Remove", GUILayout.Width(70f)))
									remove.Add(i);
							}
							EditorGUILayout.EndVertical();
							j++;
						}
						for(int i = 0; i < remove.Count; i++)
							_selectedAnimation.Frames.RemoveAt(remove[i]);
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
        _selectedAnimation.Frames.Add(Sprite.Create(new Texture2D(0, 0), new Rect(), Vector2.zero));
    }

    private void AddFrame(Sprite s)
    {
        AddFrame();
        _selectedAnimation.Frames[_selectedAnimation.Frames.Count - 1] = s;
    }

    private void CreateAnimation(string folder){
		SpriteAnimation asset = CreateInstance<SpriteAnimation>();
		string relativeFolder = "";
		asset.Name = _animName;
        int folderPosition = folder.IndexOf("Assets/");
        if (folderPosition > 0)
        {
            relativeFolder = folder.Substring(folderPosition);
            relativeFolder += "/";
        }
        else
            relativeFolder = "Assets/";
		AssetDatabase.CreateAsset(asset, relativeFolder + _animName + ".asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Selection.activeObject 	= asset;
		_selectedAnimation = asset;
		EditorUtility.DisplayDialog("Sprite Animation Created", "Sprite Animation saved to " + relativeFolder + _animName , "OK");
	}

}