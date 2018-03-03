using AssetBundles;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// added by wsh @ 2017.12.29
/// 功能：GUILayout通用工具类
/// </summary>

public class GUILayoutUtils
{
    static public void DrawPadding()
    {
        GUILayout.Space(18f);
    }
    
    static public void DrawProperty(string title, string content, float totalWidth = 250f, float titleWidth = 200f)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(totalWidth));
        EditorGUILayout.LabelField(title, GUILayout.MaxWidth(titleWidth));
        EditorGUILayout.LabelField(content, GUILayout.MinWidth(totalWidth - titleWidth));
        EditorGUILayout.EndHorizontal();
    }

    static public string DrawInputField(string title, string content, float totalWidth = 250f, float titleWidth = 200f)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(totalWidth));
        EditorGUILayout.LabelField(title, GUILayout.MaxWidth(titleWidth));
        content = EditorGUILayout.TextField(content, GUILayout.MinWidth(totalWidth - titleWidth));
        EditorGUILayout.EndHorizontal();
        return content;
    }

    static public bool DrawHeader(string text, Dictionary<string, bool> states, string key, bool forceOn, bool minimalistic, params GUILayoutOption[] options)
    {
        bool state = forceOn;
        if (forceOn)
        {
            states[key] = forceOn;
        }
        else
        {
            states.TryGetValue(key, out state);
        }

        if (!minimalistic)
        {
            GUILayout.Space(3f);
        }
        if (!forceOn && !state)
        {
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        }
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state)
            {
                text = "\u25BC" + (char)0x200a + text;
            }
            else
            {
                text = "\u25BA" + (char)0x200a + text;
            }

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            state = GUILayout.Toggle(state, text, "PreToolbar2", options);
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state)
            {
                text = "\u25BC " + text;
            }
            else
            {
                text = "\u25BA " + text;
            }
            state = GUILayout.Toggle(state, text, "dragtab", options);
        }

        if (GUI.changed)
        {
            states[key] = state;
        }

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    static public bool DrawSubHeader(int level, string text, Dictionary<string, bool> states, string key, string subText)
    {
        if (string.IsNullOrEmpty(subText))
        {
            EditorGUILayout.BeginHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(250f));
        }
        EditorGUILayout.LabelField("", GUILayout.MinWidth(20 * level), GUILayout.MaxWidth(20 * level));
        var expanded = DrawHeader(text, states, key, false, true, GUILayout.MinWidth(200f));
        EditorGUILayout.LabelField(subText, GUILayout.MinWidth(50), GUILayout.MaxWidth(50));
        EditorGUILayout.EndHorizontal();
        return expanded;
    }

    static public bool DrawRemovableSubHeader(int level, string text, Dictionary<string, bool> states, string key, Action callback)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MinWidth(250f));
        EditorGUILayout.LabelField("", GUILayout.MinWidth(20 * level), GUILayout.MaxWidth(20 * level));
        var expanded = DrawHeader(text, states, key, false, true, GUILayout.MinWidth(200f));
        if (GUILayout.Button("X", GUILayout.MaxWidth(18), GUILayout.MaxHeight(18)))
        {
            callback();
            states.Remove(key);
        }
        EditorGUILayout.EndHorizontal();
        return expanded;
    }

    static public void BeginContents(bool minimalistic)
    {
        if (!minimalistic)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        }
        else
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
            GUILayout.Space(10f);
        }
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }

    static public void EndContents(bool minimalistic)
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (!minimalistic)
        {
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(3f);
    }

    static public void DrawTextListContent(List<string> list, string prefix = null)
    {
        BeginContents(false);
        for (int i = 0; i < list.Count; i++)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                EditorGUILayout.LabelField(prefix + list[i], GUILayout.MinWidth(150f));
            }
            else
            {
                EditorGUILayout.LabelField(list[i], GUILayout.MinWidth(150f));
            }
        }
        EndContents(false);
    }

    static public void DrawTextArrayContent(string[] array, string prefix = null)
    {
        BeginContents(false);
        for (int i = 0; i < array.Length; i++)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                EditorGUILayout.LabelField(prefix + array[i], GUILayout.MinWidth(150f));
            }
            else
            {
                EditorGUILayout.LabelField(array[i], GUILayout.MinWidth(150f));
            }
        }
        EndContents(false);
    }
}