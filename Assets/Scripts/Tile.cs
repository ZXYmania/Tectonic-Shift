using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Position
{
    public int x;
    public int y;

    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2 GetVector()
    {
        return new Vector2(x, y);
    }

    public void ConvertToDirection()
    {
        x = x.CompareTo(0);
        y = y.CompareTo(0);
    }

    public override int GetHashCode()
    {
        return (x + "0" + y).GetHashCode();
    }

    public static bool operator ==(Position c1, Position c2)
    {
        return c1.Equals(c2);
    }

    public static Position operator +(Position c1, Position c2)
    {
        return new Position(c1.x + c2.x, c1.y + c2.y);
    }

    public static Position operator -(Position c1, Position c2)
    {
        return new Position(c1.x - c2.x, c1.y - c2.y);
    }

    public static bool operator !=(Position c1, Position c2) => !c1.Equals(c2);
    public override bool Equals(object obj)
    {
        if (obj is Position)
        {
            return Equals((Position) obj);
        }
        return false;
    }

    public bool Equals(Position obj)
    {
        return obj != null && obj.x == this.x && obj.y == this.y;
    }

    public override string ToString()
    {
        return "(" + x + "," + y + ")";
    }
}

public interface TileProperty
{
    public Tuple<string, int> Animate();
}
[Serializable]
public class Tile : Animated, Clickable
{
    public static event Action<Tile> Clicked = delegate { };
    public static event Action<Tile> Hovered = delegate { };
    public static string draw_property;
    public static Tile CreateTile(int x, int y)
    {
        GameObject gameObject = new GameObject();
        gameObject.name = "tile("+ x + "," + y + ")";
        BoxCollider2D m_collider = gameObject.AddComponent<BoxCollider2D>();
        m_collider.offset = new Vector2(0.5f, 0.5f);
        Tile tile = gameObject.AddComponent<Tile>();
        tile.position = new Position(x, y);
        return tile;
    }
    public Position position { get; protected set; }
    public bool selected;
    public bool hovered;
    Dictionary<string, TileProperty> m_properties;
    
    public T GetProperty<T>() where T : TileProperty, new()
    {
        if(m_properties.ContainsKey(typeof(T).Name))
        {
            return (T) m_properties[typeof(T).Name];
        }
        return new T();
    }

    public void SetProperty<T>(T given_property) where T : TileProperty, new()
    {
        string name = typeof(T).Name;
        m_properties[name] = given_property;
        if(name == draw_property)
        {
            Tuple<string, int> animation = given_property.Animate();
            m_animationLayer[animation.Item1].ChangeAnimation(animation.Item2);
        }

    }

    protected override void Initialise()
    {
        base.Initialise();
        AddAnimationLayer<SpriteLayer>("plate", "plates", Color.black, true);
        AddAnimationLayer<SpriteLayer>("border", "plates", Color.red, true, false);
        SetTransform(position.x, position.y);
        m_animationLayer["border"].SetAnimationPosition(0, 0, -1);
        selected = false;
        hovered = false;
        m_properties = new Dictionary<string, TileProperty>();
    }

    void Start()
    {
        Initialise();
    }

    // Update is called once per frame
    void Update()
    {
        Animate();
    }

    void OnMouseDown()
    {
        Clicked(this);
    }

    void OnMouseEnter()
    {
        Hovered(this);
    }

    void OnMouseExit()
    {
        UnHover();
    }

    public override string ToString()
    {
        return position.ToString();
    }

    public bool IsSelected() { return selected; }

    public void OnHover()
    {
        hovered = true;
        m_animationLayer["border"].SetVisible(true);
    }

    public void UnHover()
    {
        hovered = false;
        m_animationLayer["border"].SetVisible(selected || hovered);

    }
    public void OnSelect()
    {
        selected = true;
        UnHover();
        m_animationLayer["border"].SetVisible(selected);
    }

    public void UnSelect()
    {
        selected = false;
        m_animationLayer["border"].SetVisible(selected || hovered);
    }
}
