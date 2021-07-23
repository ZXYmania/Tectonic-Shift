using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class InvalidContinentShape : System.Exception
{
    public InvalidContinentShape(string message) : base(message)
    {

    }
}

[Serializable]
public class Plate
{
    public static List<Plate> plate = new List<Plate>();
    public HashSet<Position> border;
    [Serializable]
    public class Edge
    {
        public List<Position> tile;
        public Position facing;
        int plate_index;
        public HashSet<Position> corner;
        public Edge(List<Position> tile, int plate_index)
        {
            this.plate_index = plate_index;
            this.tile = new List<Position>(tile);
            corner = new HashSet<Position>() { this.tile[0], this.tile[this.tile.Count - 1] };
        }


        public Position GetDirection()
        {
            return Map.GetDirection(tile[1], tile[0]);
        }
    }
    public List<Edge> edge { get; protected set; }
    protected Plate(List<Position> given_tiles)
    {
        try
        {
            plate.Add(this);
            this.edge = new List<Edge>();
            List<Position> perimeter = new List<Position>(given_tiles);
            int trim = perimeter.FindIndex((tile) => tile == perimeter[perimeter.Count - 1]);
            perimeter.RemoveRange(0, trim);
            border = new HashSet<Position>(perimeter);
            CreateEdges(perimeter);
            if (perimeter.Count > 9)
            {
                FillArea();
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
            throw e;
        }
    }

    public static Plate CreatePlate(List<Position> perimeter)
    {
        return new Plate(perimeter);
    }

    public static void CreatePlate(Plate given_plate)
    {
        plate.Add(given_plate);
        given_plate.FillArea();
    }

    public bool IsInside(Position selectedtile)
    {
        int below = 0;
        int above = 0;
        List<Tuple<Position, bool>> number_line = new List<Tuple<Position, bool>>();
        Dictionary<Position, int> corner_direction = new Dictionary<Position, int>();
        for (int i = 0; i < edge.Count; i++)
        {
            List<Position> current_tile = edge[i].tile;
            List<Position> hit = current_tile.FindAll((tile) => tile.x == selectedtile.x && tile.y < selectedtile.y);
            if (hit.Count == 1)
            {
                int hit_index = current_tile.IndexOf(hit[0]);
                if (hit_index == 0 || hit_index == current_tile.Count - 1)
                {
                    Position direction = edge[i].GetDirection();
                    if (!corner_direction.ContainsKey(current_tile[hit_index]))
                    {
                        List<Edge> vertical = edge.FindAll((edge) => edge.tile.Contains(hit[0]) && edge.GetDirection().x == 0);
                        if (vertical.Count == 0)
                        {
                            //Traverse to see if opposite end of vertical line has key
                            corner_direction.Add(current_tile[hit_index], direction.x);
                            below++;
                            number_line.Add(new Tuple<Position, bool>(hit[0], true));
                        }
                        else
                        {
                            int vertical_index = vertical[0].tile.IndexOf(hit[0]);
                            vertical_index = vertical_index.CompareTo(vertical[0].tile.Count - 1) * (vertical[0].tile.Count - 1) * -1;
                            Position vertical_tile = vertical[0].tile[vertical_index];
                            if (!corner_direction.ContainsKey(vertical_tile))
                            {
                                corner_direction.Add(current_tile[hit_index], direction.x);
                                corner_direction.Add(vertical_tile, direction.x);
                                below++;
                                number_line.Add(new Tuple<Position, bool>(hit[0], true));
                            }
                            else if (corner_direction[vertical[0].tile[vertical_index]] != direction.x)
                            {
                                below++;
                                corner_direction.Add(current_tile[hit_index], direction.x);
                                number_line.Add(new Tuple<Position, bool>(hit[0], true));
                            }
                            else
                            {
                                number_line.Add(new Tuple<Position, bool>(hit[0], false));
                            }
                        }
                    }
                    else if (corner_direction[current_tile[hit_index]] != direction.x)
                    {
                        below++;
                        number_line.Add(new Tuple<Position, bool>(hit[0], true));
                    }
                    else
                    {
                        number_line.Add(new Tuple<Position, bool>(hit[0], false));
                    }
                }
                else
                {
                    below++;
                    number_line.Add(new Tuple<Position, bool>(hit[0], true));
                }
            }
        }
        corner_direction.Clear();
        number_line.Add(new Tuple<Position, bool>(new Position(-1, -1), false));
        for (int i = 0; i < edge.Count; i++)
        {
            List<Position> current_tile = edge[i].tile;
            List<Position> hit = edge[i].tile.FindAll((tile) => tile.x == selectedtile.x && tile.y > selectedtile.y);
            if (hit.Count == 1)
            {
                int hit_index = current_tile.IndexOf(hit[0]);
                if (hit_index == 0 || hit_index == current_tile.Count - 1)
                {
                    Position direction = edge[i].GetDirection();
                    if (!corner_direction.ContainsKey(current_tile[hit_index]))
                    {
                        List<Edge> vertical = edge.FindAll((edge) => edge.tile.Contains(hit[0]) && edge.GetDirection().x == 0);
                        if (vertical.Count == 0)
                        {
                            //Traverse to see if opposite end of vertical line has key
                            corner_direction.Add(current_tile[hit_index], direction.x);
                            above++;
                            number_line.Add(new Tuple<Position, bool>(hit[0], true));
                        }
                        else
                        {
                            int vertical_index = vertical[0].tile.IndexOf(hit[0]);
                            vertical_index = vertical_index.CompareTo(vertical[0].tile.Count - 1) * (vertical[0].tile.Count - 1) * -1;
                            Position vertical_tile = vertical[0].tile[vertical_index];
                            if (!corner_direction.ContainsKey(vertical_tile))
                            {
                                corner_direction.Add(current_tile[hit_index], direction.x);
                                corner_direction.Add(vertical_tile, direction.x);
                                above++;
                                number_line.Add(new Tuple<Position, bool>(hit[0], true));
                            }
                            else if (corner_direction[vertical[0].tile[vertical_index]] != direction.x)
                            {
                                above++;
                                corner_direction.Add(current_tile[hit_index], direction.x);
                                number_line.Add(new Tuple<Position, bool>(hit[0], true));
                            }
                            else
                            {
                                number_line.Add(new Tuple<Position, bool>(hit[0], false));
                            }
                        }
                    }
                    else if (corner_direction[current_tile[hit_index]] != direction.x)
                    {
                        above++;
                        number_line.Add(new Tuple<Position, bool>(hit[0], true));
                    }
                    else
                    {
                        number_line.Add(new Tuple<Position, bool>(hit[0], false));
                    }
                }
                else
                {
                    above++;
                    number_line.Add(new Tuple<Position, bool>(hit[0], true));
                }
            }
        }

        bool inside = below % 2 != 0;
        if (inside != (above % 2 != 0))
        {
            Debug.Log(selectedtile + ", " + number_line);
            throw new Exception("Inside algorithm failed"+selectedtile + ", " + number_line);
        }

        return inside;
    }

    protected bool Contains(Position position)
    {
        return edge.Exists((edge) => edge.tile.Contains(position));
    }

    protected void CreateEdges(List<Position> perimeter)
    {
        List<Position> corner = new List<Position>();
        Position previous_direction = new Position(0, 0);
        Position previous_corner = new Position(0, 0);
        List<Position> current_edge = new List<Position>();
        // start at one to allow inequality comparison
        int flat = 0;
        for (int i = 0; i < perimeter.Count; i++)
        {
            int previous_index = i - 1;
            int next_index = i + 1;
            if (previous_index < 0)
            {
                previous_index = perimeter.Count - 1;
            }
            if (next_index >= perimeter.Count)
            {
                next_index = 0;
            }
            if (perimeter[next_index] == perimeter[i])
            {
                corner.Remove(perimeter[next_index]);
                perimeter.RemoveAt(next_index);
                if (next_index >= perimeter.Count)
                {
                    next_index = 0;
                }
                else if (next_index == 0)
                {
                    i--;
                    previous_index--;
                }
            }
            Position current_tile = perimeter[i];
            Position next_direction = Map.GetDirection(perimeter[i], perimeter[next_index]);
            Position current_direction = Map.GetDirection(perimeter[previous_index], perimeter[i]);
            // Corner started
            if (current_direction != next_direction)
            {
                corner.Add(perimeter[i]);
                current_edge.Add(perimeter[i]);
                if (current_edge.Exists((tile) => tile.x == 0) && current_edge.Exists((tile) => tile.x == Map.size.x - 1))
                {
                    flat += Map.GetDirection(current_edge[1], current_edge[0]).x;
                }
                previous_direction = new Position(current_direction.x, current_direction.y);
                edge.Add(new Edge(current_edge, Plate.plate.IndexOf(this)));

                current_edge.Clear();
                previous_direction = new Position(next_direction.x, next_direction.y);
                previous_corner = perimeter[i];
            }
            current_edge.Add(perimeter[i]);

        }
        edge.RemoveAt(0);
        if (!corner.Contains(perimeter[perimeter.Count - 1]))
        {
            current_edge.AddRange(edge[0].tile.GetRange(1, edge[0].tile.Count - 1));
            edge.RemoveAt(0);
            edge.Add(new Edge(current_edge, Plate.plate.IndexOf(this)));
            if (current_edge.Exists((tile) => tile.x == 0) && current_edge.Exists((tile) => tile.x == Map.size.x - 1))
            {
                flat += Map.GetDirection(current_edge[1], current_edge[0]).x;
            }
        }
        current_edge.Clear();
        int modulo = flat % 2;
        if (modulo != 0)
        {
            if (perimeter[perimeter.Count - 1].y.CompareTo(Mathf.FloorToInt(Map.size.y / 2)) <= 0)
            {
                //create edge from 0,0 to  map.size.x, 0
                for (int i = 0; i < Map.size.x; i++)
                {
                    current_edge.Add(new Position(i, 0));
                }
            }
            else
            {
                //create edge from 0, map.size.y to  map.size.x, mape.size.y
                for (int i = 0; i < Map.size.x; i++)
                {
                    current_edge.Add(new Position(i, Map.size.y - 1));
                }

            }
            edge.Add(new Edge(current_edge, Plate.plate.IndexOf(this)));
            border.UnionWith(current_edge);
        }
    }


    protected void FillArea()
    {
        for (int i = 0; i < edge.Count; i++)
        {
            Edge draw_edge = edge[i];
            Position direction = edge[i].GetDirection();
            for (int j = 0; j < edge[i].tile.Count; j++)
            {
                if (Map.map[draw_edge.tile[j]].terrain == 0)
                {
                    Map.map[draw_edge.tile[j]].SetTerrain(3);
                }
                if (direction.x != 0)
                {
                    Position next_tile = draw_edge.tile[j] + new Position(0, 1);
                    while (!border.Contains(next_tile) && IsInside(next_tile))
                    {
                        Map.map[next_tile].SetTerrain(1);
                        next_tile.y++;
                    }
                }

            }
        }
    }
}
