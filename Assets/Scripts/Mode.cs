using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Clickable
{
    public bool IsSelected();
    public void OnSelect();
    public void UnSelect();
}

public interface DrawContoller
{
    public int GetDistance(Position from, Position to);
    public bool QuickStop();
}
public abstract class Mode
{
    protected static List<Clickable> selected;
    protected static Clickable hover;
    public static Mode current_menu { get; protected set; }
    
    public static void Select(Clickable clicked)
    {
        selected.Add(clicked);
        clicked.OnSelect();
        current_menu.OnSelect();
    }

    public static void Hover(Clickable hovered)
    {
        if(hover != hovered)
        {
            hover = hovered;
            current_menu.OnHover();
        }
    }

    public void Clear()
    {
        for(int i = 0; i < selected.Count; i++)
        {
            selected[i].UnSelect();
        }
        selected.Clear();
    }
    public abstract void OnSelect();
    public abstract void OnHover();
}
