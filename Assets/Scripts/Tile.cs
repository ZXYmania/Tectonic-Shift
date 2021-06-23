using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position
{
    public int x;
    public int y;
    protected int index;

    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.index = Convert.ToInt32(string.Format("{0}0{1}", x, y));
    }

    public Vector2 GetVector()
    {
        return new Vector2(x, y);
    }
    public override int GetHashCode()
    {
        return index;   
    }
    public override bool Equals(object obj)
    {
        return Equals(obj as Position);
    }

    public bool Equals(Position obj)
    {
        return obj != null && obj.x == this.x && obj.y == this.y;
    }
}
public class Tile : MonoBehaviour
{
    public static Tile CreateTile(int x, int y)
    {
        GameObject gameObject = new GameObject();
        BoxCollider2D m_collider = gameObject.AddComponent<BoxCollider2D>();
        Tile tile = gameObject.AddComponent<Tile>();
        tile.initialise(x,y);
        return tile;
    }
    public Position position { get; protected set; }

    protected void initialise(int x, int y)
    {
        this.position = new Position(x, y);
    }

    protected void initialise(Position position)
    {
        this.position = position;
    }

    void Start()
    {
        transform.position = new Vector3(position.x, position.y, 0);
        gameObject.GetComponent<Renderer>().material = Resources.Load("molten_core") as Material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
