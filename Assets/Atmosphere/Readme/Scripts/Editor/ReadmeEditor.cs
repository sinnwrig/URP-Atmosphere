﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.VersionControl;

[CustomEditor(typeof(Readme))]
[InitializeOnLoad]
public class ReadmeEditor : Editor 
{

        static readonly string kShowedReadmeSessionStateName = "ReadmeEditor.showedAtmosphereReadme";

        static readonly float kSpace = 16f;



        static ReadmeEditor()
        {
            EditorApplication.delayCall += SelectReadmeAutomatically;
        }


        static void SelectReadmeAutomatically()
        {
            if (!SessionState.GetBool(kShowedReadmeSessionStateName, false))
            {
                var readme = SelectReadme();
                if (readme)
                {
                    SessionState.SetBool(kShowedReadmeSessionStateName, true);
                }
            }
        }


        [MenuItem("Tutorial/Show Tutorial Instructions")]
        static Readme SelectReadme()
        {
            Readme result = GetReadmeRoot();

            if (result != null)
            {
                Selection.objects = new Object[] { result };

            }
            else
            {
                Debug.LogWarning("Couldn't find a readme");
            }

            return result;
        }


        static void RemoveReadmeAssets()
        {
            // Erases the first Readme folder it finds- Not the best option if you have multiple Readme folders.
            string readmeFolder = FindFolder("Readme");

            if (EditorUtility.DisplayDialog("Remove Readme assets?", $"Are you sure you want to delete all Readme assets under {readmeFolder}?", "Confirm", "Cancel")) 
            {
                AssetDatabase.DeleteAsset(readmeFolder);
                AssetDatabase.SaveAssets();
            }
        }


        static string FindFolder(string folderName)
        {
            string fPath = null;

            var folders = AssetDatabase.GetSubFolders("Assets");
            foreach (var folder in folders)
            {
                if (FindFolder(folder, folderName, out fPath))
                {
                    break;
                }
            }

            return fPath;
        }


        static bool FindFolder(string checkFolder, string folderName, out string folderPath)
        {
            folderPath = null;

            if (System.IO.Path.GetFileName(checkFolder) == folderName)
            {
                folderPath = checkFolder;
                return true;
            }

            var folders = AssetDatabase.GetSubFolders(checkFolder);
            foreach (var fld in folders)
            {
                if (FindFolder(fld, folderName, out folderPath))
                {
                    return true;
                }
            }

            return false;
        }


        protected override void OnHeaderGUI()
        {
            var readme = (Readme)target;

            DrawHeaderGUI(readme);
        }


        public override void OnInspectorGUI()
        {
            var readme = (Readme)target;

            DrawInspectorGUI(readme);

            if (GUILayout.Button("Remove Readme Assets", GUILayout.MinHeight(25)))
            {
                RemoveReadmeAssets();
                return;
            }
        }


        public static void DrawHeaderGUI(Readme readme)
        {
            if (readme == null)
                return;

            var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 128f);

            GUILayout.BeginHorizontal("In BigTitle");
            {

                GUILayout.Label(readme.icon, GUILayout.Width(iconWidth), GUILayout.Height(iconWidth));
                GUILayout.Label(readme.title, ReadmeEditorStyles.TitleStyle, GUILayout.ExpandHeight(true));

            }
            GUILayout.EndHorizontal();
        }


        public static void DrawInspectorGUI(Readme readme)
        {
            if (readme == null)
                return;
            if (readme.sections == null)
                return;

            foreach (var section in readme.sections)
            {
                if (!string.IsNullOrEmpty(section.heading))
                {
                    section.name = "Header -" + section.heading;

                    GUILayout.Label(section.heading, ReadmeEditorStyles.HeadingStyle);

                    //Add Horizontal Bar
                    if (section.heading != "") { EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); }
                }
                if (!string.IsNullOrEmpty(section.text))
                {
                    if (string.IsNullOrEmpty(section.name))
                        section.name = "Text: " + section.text;

                    GUILayout.Label(section.text, ReadmeEditorStyles.BodyStyle);
                }
                if (!string.IsNullOrEmpty(section.linkText))
                {

                    if (string.IsNullOrEmpty(section.name))
                        section.name = "Link: " + section.text;

                    if (ReadmeEditorStyles.LinkLabel(new GUIContent(section.linkText)))
                    {
                        Application.OpenURL(section.url);
                    }
                }
                GUILayout.Space(kSpace);

                if (section.name.Length > 20)
                {
                    section.name = section.name.Remove(17);
                    section.name += "...";
                }

            }
        }


        static Readme GetReadmeRoot()
        {
            var ids = AssetDatabase.FindAssets("Readme t:Readme");
            List<Readme> results = new List<Readme>();

            foreach (string guid in ids)
            {
                var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                results.Add((Readme)readmeObject);
            }

            return results[0];
        }
}