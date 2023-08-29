using System;
using UnityEngine;

public class Readme : ScriptableObject 
{
	public Texture2D icon;
    public string title;

    public Section[] sections;


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
