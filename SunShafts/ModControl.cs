using System;
using System.Collections.Generic;
using UnityEngine;

public class ModControl : MonoBehaviour
{
    public class Mod
    {
        public Action action;
        public float height;
        public string name;
    }

    private List<Mod> mods = new List<Mod>();
    private bool showWindow = false;
    private float width = 200f;
    private float itemmargin = 15f;
    private Texture2D tex;
    private GUIStyle style;

    public void addMod(string name)
    {
        for (int index = 0; index < this.mods.Count; ++index)
        {
            if (this.mods[index].name == name)
            {
                this.mods.RemoveAt(index);
                --index;
            }
        }
        this.mods.Add(new Mod() {name = name});
    }

    public void setAction(Action modFunction)
    {
        this.mods[this.mods.Count - 1].action = modFunction;
    }

    public void setHeight(float modHeight)
    {
        this.mods[this.mods.Count - 1].height = modHeight;
    }

    private void Start()
    {
        this.tex = new Texture2D(1, 1);
        this.tex.SetPixel(1, 1, new Color(99f / 256f, 159f / 256f, (float) byte.MaxValue / 256f, 0.75f));
        this.tex.Apply();
    }

    private void func(int windowID)
    {
        float y = 20f;
        for (int index = 0; index < this.mods.Count; ++index)
        {
            GUI.BeginScrollView(
                new Rect(this.itemmargin, y, this.width - 2f * this.itemmargin, this.mods[index].height),
                new Vector2(0.0f, 0.0f), new Rect(0.0f, 0.0f, 1f, 1f));
            this.mods[index].action();
            GUI.EndScrollView();
            y += this.mods[index].height;
        }
    }

    private void Update()
    {
        if (!Input.GetKeyUp(KeyCode.F8))
            return;
        this.showWindow = !this.showWindow;
    }

    private void OnGUI()
    {
        if (!this.showWindow)
            return;
        if (this.style == null)
        {
            this.style = new GUIStyle(GUI.skin.window);
            this.style.normal.background = this.tex;
            this.style.active.background = this.tex;
            this.style.hover.background = this.tex;
            this.style.focused.background = this.tex;
            this.style.onActive.background = this.tex;
            this.style.onFocused.background = this.tex;
            this.style.onHover.background = this.tex;
            this.style.onNormal.background = this.tex;
        }
        float height = 30f;
        foreach (Mod mod in this.mods)
            height += mod.height;
        GUI.Window(67432, new Rect(100f, 100f, this.width, height), new GUI.WindowFunction(this.func),
            new GUIContent("ModControl - F8 to hide"), this.style);
    }
}