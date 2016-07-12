using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class EditorSpriteAnimation : EditorWindow {

	private SpriteAnimation _selectedAnimation;
	private string _animName 	= "Animation";
	private Vector2 _pos 		= Vector2.zero;

	[MenuItem ("Elendow Tools/Sprite Animation Editor", false, 0)]
	public static void ShowWindow(){
		EditorWindow.GetWindow(typeof(EditorSpriteAnimation), false, "Sprite Animation");
	}

	public void Update()
	{
		if(Selection.activeObject != null && Selection.activeObject.GetType() == typeof(SpriteAnimation))
		{
			_selectedAnimation = Selection.activeObject as SpriteAnimation;
			Repaint();
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
				CreateZone(folder);
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
				if(GUILayout.Button("Add Frame"))
					_selectedAnimation.Frames.Add(Sprite.Create(new Texture2D(0,0), new Rect(), Vector2.zero));

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

	private void CreateZone(string folder){
		SpriteAnimation asset	= CreateInstance<SpriteAnimation>();
		string relativeFolder	= "";
		asset.Name 				= _animName;
		relativeFolder  		= folder.Substring(folder.IndexOf("Assets/"));
		relativeFolder			+= "/";
		AssetDatabase.CreateAsset(asset, relativeFolder + _animName + ".asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		Selection.activeObject 	= asset;
		_selectedAnimation 		= asset;
		EditorUtility.DisplayDialog("Sprite Animation Created", "Sprite Animation saved to " + relativeFolder + _animName , "OK");
	}

}