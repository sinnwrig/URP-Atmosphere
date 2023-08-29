using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ReadmeEditorStyles
{
    private static GUIStyle m_LinkStyle;
    public static GUIStyle LinkStyle { get { return m_LinkStyle; } }

    public static GUIStyle TitleStyle { get { return m_TitleStyle; } }
    private static GUIStyle m_TitleStyle;

    public static GUIStyle HeadingStyle { get { return m_HeadingStyle; } }
    private static GUIStyle m_HeadingStyle;

    public static GUIStyle BodyStyle { get { return m_BodyStyle; } }
    private static GUIStyle m_BodyStyle;


    static ReadmeEditorStyles()
    {
        m_BodyStyle = new GUIStyle(EditorStyles.label);
        m_BodyStyle.wordWrap = true;
        m_BodyStyle.fontSize = 14;
        m_BodyStyle.richText = true;


        m_TitleStyle = new GUIStyle(EditorStyles.boldLabel);
        m_TitleStyle.fontSize = 30;
        m_TitleStyle.wordWrap = true;
        m_TitleStyle.alignment = TextAnchor.MiddleLeft;
        m_TitleStyle.richText = true;

        m_HeadingStyle = new GUIStyle(m_TitleStyle);
        m_HeadingStyle.fontSize = 18;
        

        m_LinkStyle = new GUIStyle(m_BodyStyle);
        m_LinkStyle.wordWrap = false;
        // Match selection color which works nicely for both light and dark skins
        m_LinkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
        m_LinkStyle.stretchWidth = false;

    }

    public static bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
    {
        var position = GUILayoutUtility.GetRect(label, ReadmeEditorStyles.LinkStyle, options);

        Handles.BeginGUI();
        Handles.color = ReadmeEditorStyles.LinkStyle.normal.textColor;
        Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
        Handles.color = Color.white;
        Handles.EndGUI();

        EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

        return GUI.Button(position, label, ReadmeEditorStyles.LinkStyle);
    }

}