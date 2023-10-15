using System;
using UnityEngine;

public class Readme : ScriptableObject 
{
	public Texture2D icon;
    public string title;

    public Section[] sections;
    public ReadmeLink[] links;

    public bool isSourceReadme = false;


    [Serializable]
    public class ReadmeLink
    {
        public string name;
        public Readme linkedReadme;
        public int buttonHeight;
    }


    [Serializable]
    public class Section
    {

        [HideInInspector]
        public string name;
        public string heading;
        [TextArea(5,255)]
        public string text;
        public string linkText, url;
    }
}
