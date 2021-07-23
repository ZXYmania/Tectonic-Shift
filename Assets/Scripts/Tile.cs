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
[Serializable]
public class Tile : Animated, Clickable
{
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
    public int terrain { get; protected set; }
    public bool edge;
    protected override void Initialise()
    {
        base.Initialise();
        terrain = 0;
        AddAnimationLayer("plate", "plates", Color.black, true);
        AddAnimationLayer("border", "plates", Color.red, true);
        m_animationLayer["border"].SetVisible(false);
        SetTransform(position.x, position.y);
        selected = false;
        hovered = false;
        edge = false;
        m_animationLayer["plate"].ChangeAnimation(0);
    }

    void Start()
    {
        Initialise();
    }

    // Update is called once per frame
    void Update()
    {
        if (terrain != m_animationLayer["plate"].GetAnimation())
        {
            m_animationLayer["plate"].ChangeAnimation(terrain);
        }
        if ((selected || hovered) != m_animationLayer["border"].GetVisible())
        {
            m_animationLayer["border"].SetVisible(selected||hovered);
        }
        Animate();

    }

    void OnMouseDown()
    {
       Mode.Select(this);
    }

    void OnMouseOver()
    {
        OnHover();
        Mode.Hover(this);
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
    }

    public void UnHover()
    {
        hovered = false;
    }
    public void OnSelect()
    {
        selected = true;
    }

    public void UnSelect()
    {
        selected = false;
    }

    public void SetTerrain(int terrain)
    {
        edge = true;
        this.terrain = terrain;
    }
}
