// Spritedow Animation Plugin by Elendow
// https://elendow.com

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Elendow.SpritedowAnimator
{
    public static class SpriteEditorHelper
    {
        public static void DrawFrameListElement(ref SpriteAnimation animation, GUIContent clockIcon, Rect r, int i, bool active, bool focused)
        {
            string spriteName = (animation.Frames[i].Sprite != null) ? animation.Frames[i].Sprite.name : "No sprite selected";

            r.height = 20;
            EditorGUIUtility.labelWidth = 0;

            EditorGUI.BeginChangeCheck();
            Sprite spriteValue = EditorGUI.ObjectField(new Rect(r.x, r.y + 2, r.width - 200, r.height - 2), "", animation.Frames[i].Sprite, typeof(Sprite), false) as Sprite;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(animation, "Change Animation Sprite");
                animation.Frames[i].Sprite = spriteValue;
            }

            EditorGUIUtility.labelWidth = 20;

            EditorGUI.BeginChangeCheck();
            int durationValue = EditorGUI.IntField(new Rect(r.x + r.width - 175, r.y + 1, 75, r.height - 4), clockIcon, animation.Frames[i].Duration);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(animation, "Change Frame Duration");
                animation.Frames[i].Duration = durationValue;
                if (durationValue <= 0)
                {
                    animation.Frames[i].Duration = 1;
                }
            }

            Rect buttonRect = r;
            buttonRect.width = 90;
            buttonRect.x = r.width - 70;
            if (GUI.Button(buttonRect, "Add Action"))
            {
                animation.Actions.Add(new SpriteAnimationAction(i, "New Action"));
            }

            for (int j = animation.Actions.Count - 1; j >= 0; j--)
            {
                if (animation.Actions[j].Frame == i)
                {
                    r.y += 25;
                    animation.Actions[j].Data = EditorGUI.TextField(new Rect(50, r.y, r.width - 70, 20), animation.Actions[j].Data);

                    if (GUI.Button(new Rect(r.x + r.width - 30, r.y, 30, 20), "-"))
                    {
                        animation.Actions.RemoveAt(j);
                    }
                }
            }
        }

        public static float GetElementHeight(SpriteAnimation animation, int index)
        {
            float height = 30f;

            for (int i = 0; i < animation.Actions.Count; i++)
            {
                if (animation.Actions[i].Frame == index)
                {
                    height += 25f;
                }
            }

            return height;
        }

        public static bool CheckListOutOfSync(List<SpriteAnimationFrame> frames, List<SpriteAnimationAction> actions, SpriteAnimation animation)
        {
            if (frames == null || actions == null || animation == null)
            {
                return true;
            }

            if (frames.Count != animation.Frames.Count)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < frames.Count; i++)
                {
                    if (frames[i].Duration != animation.Frames[i].Duration ||
                        frames[i].Sprite != animation.Frames[i].Sprite)
                    {
                        return true;
                    }
                }
            }

            if (actions.Count != animation.Actions.Count)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i].Data != animation.Actions[i].Data)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
