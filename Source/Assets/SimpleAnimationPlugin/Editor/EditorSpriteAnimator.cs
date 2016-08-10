// Simple Sprite Animation Plugin by Elendow
// http://elendow.com
// https://github.com/Elendow/Unity-Simple-Sprite-Animation-Plugin
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteAnimator))]
public class EditorSpriteAnimator : Editor
{
    private int _startAnimationIndex = 0;
    private string[] _animationNames;
    private SpriteAnimator _target;

    public override void OnInspectorGUI()
    {
        _target = (SpriteAnimator)target;

        if (_animationNames == null)
            GetAnimationNames();

        SerializedProperty animations = serializedObject.FindProperty("animations");
        _target.playOnAwake = EditorGUILayout.Toggle("Play on Awake", _target.playOnAwake);
        if (_target.playOnAwake)
        {
            if (_animationNames != null && _animationNames.Length > 0)
            {
                _startAnimationIndex = EditorGUILayout.Popup("Start Animation", _startAnimationIndex, _animationNames);
                _target.startAnimation = _animationNames[_startAnimationIndex];
            }
            else
            {
                EditorGUILayout.LabelField("Start animation", "No animations");
            }
            EditorUtility.SetDirty(target);
        }

        _target.framesPerSecond = EditorGUILayout.IntField("FPS", _target.framesPerSecond);
        if (_target.framesPerSecond < 0) _target.framesPerSecond = 0;

        EditorGUILayout.PropertyField(animations, true);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_target);
            serializedObject.ApplyModifiedProperties();
            GetAnimationNames();
        }
    }

    private void GetAnimationNames()
    {
        if (_target.animations != null && _target.animations.Count > 0)
        {
            _animationNames = new string[_target.animations.Count];
            for (int i = 0; i < _animationNames.Length; i++)
            {
                if (_target.animations[i])
                {
                    _animationNames[i] = _target.animations[i].Name;
                    if (_target.animations[i].Name == _target.startAnimation)
                        _startAnimationIndex = i;
                }
                else
                    _animationNames[i] = "null_" + i;
            }
        }
    }
}
