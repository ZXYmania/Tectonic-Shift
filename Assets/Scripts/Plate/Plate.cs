using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
namespace PlateSpace
{
    public class InvalidContinentShape : System.Exception
    {
        public InvalidContinentShape(string message) : base(message)
        {

        }
    }
    [Serializable]
    public struct CreatePlateErrorData
    {
        string m_message;
        public List<Position> m_perimeter;
        public Guid m_id;

        public CreatePlateErrorData(string message, List<Position> perimeter, Guid id)
        {
            m_message = message;
            m_perimeter = perimeter;
            m_id = id;
        }
    }
    public class CreatePlateError : Exception
    {
        public CreatePlateErrorData m_data;

        public CreatePlateError(string message, List<Position> perimeter, Guid id) : base(message)
        {
            m_data = new CreatePlateErrorData(message, perimeter, id);
        }
    }


    [Serializable]
    public struct PlateProperty : TileProperty
    {
        public int craton;
        public Guid plate_id;
        public PlateProperty(int craton, Guid id)
        {
            this.craton = craton;
            this.plate_id = id;
        }

        public Tuple<string, int> Animate()
        {
            return new Tuple<string, int>("plate", craton );
        }
    }

    [Serializable]
    public class Plate : Clickable
    {
        public static Dictionary<Guid, Plate> plate { get; protected set; } = new Dictionary<Guid, Plate>();
        public static void Clear()
        {
            plate.Clear();
        }
        public HashSet<Position> border;
        public string name { set; get; }
        public Guid id { get; protected set; }
        [Serializable]
        public class Edge
        {
            public List<Position> tile;
            public Position facing;
            public Guid plate_id;
            public Edge(List<Position> tile, Guid plate_id)
            {
                this.plate_id = plate_id;
                this.tile = new List<Position>(tile);
            }

            public Position GetDirection()
            {
                return Map.GetDirection(tile[1], tile[0]);
            }
            public void SetOutside()
            {
                int halfway = Mathf.FloorToInt(tile.Count / 2);
                Position outside = GetDirection();
                outside = new Position(outside.y, outside.x * -1);
                if (plate[plate_id].IsInside(tile[halfway] + outside))
                {
                    outside = new Position(outside.x * -1, outside.y * -1);
                    if (plate[plate_id].IsInside(tile[halfway] + outside))
                    {
                        facing = new Position(0, 0);
                    }
                }
                facing = outside;
            }

            public bool Validate()
            {
                Plate parent = plate[plate_id];
                int i = 1;
                for (; i < tile.Count - 1; i++)
                {
                    if (parent.IsInside(tile[i] + facing))
                    {
                        Debug.Log(tile[i] + " " + facing + " edge is not valid");
                        Map.map[tile[i] + facing].SetProperty<PlateProperty>(new PlateProperty(2, plate_id));
                        return false;
                    }
                }
                return i > 1 || !parent.IsInside(tile[0] + facing);
            }
        }
        public List<Edge> edge { get; protected set; }
        protected Plate(List<Position> given_tiles)
        {
            try
            {
                int count = plate.Count;
                this.name = GenerateName();
                id = System.Guid.NewGuid();
                plate.Add(id, this);
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
                if (plate.Count == count)
                {
                    throw new Exception("Plate wasn't created");
                }
            }
            catch (InvalidContinentShape e)
            {
                throw new CreatePlateError(e.Message, given_tiles, id);
            }
        }
        public static Plate CreatePlate(List<Position> perimeter)
        {
            return new Plate(perimeter);
        }

        public static void CreatePlate(Plate given_plate)
        {
            plate.Add(given_plate.id, given_plate);
            given_plate.FillArea();
        }
        protected void CreateEdges(List<Position> perimeter)
        {
            List<Position> corner = new List<Position>();
            Position previous_direction = new Position(0, 0);
            Position previous_corner = new Position(0, 0);
            List<Position> current_edge = new List<Position>();
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
                    edge.Add(new Edge(current_edge, id));

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
                edge.Add(new Edge(current_edge, id));
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
                edge.Add(new Edge(current_edge, id));
                border.UnionWith(current_edge);
            }
            for (int i = 0; i < edge.Count; i++)
            {
                int previous_index = i - 1;
                if (previous_index < 0)
                {
                    previous_index = edge.Count - 1;
                }
                Edge previous_edge = edge[previous_index];
                edge[i].SetOutside();
                if (!edge[i].Validate())
                {
                    Debug.Log(i + " " + edge[i].facing + " " + edge[i].tile[0] + " edge is not valid");
                }
            }
        }
        protected void FillArea()
        {
            for (int i = 0; i < edge.Count; i++)
            {
                Edge draw_edge = edge[i];
                Position direction = edge[i].GetDirection();
                int previous_index = i - 1;
                if (previous_index < 0)
                {
                    previous_index = edge.Count - 1;
                }
                Position previous_tile = edge[previous_index].tile[edge[previous_index].tile.Count - 1];
                for (int j = 0; j < draw_edge.tile.Count; j++)
                {

                    if (Map.map[draw_edge.tile[j]].GetProperty<PlateProperty>().plate_id == Guid.Empty)
                    {
                        if (draw_edge.Validate())
                        {
                            Map.map[draw_edge.tile[j]].SetProperty<PlateProperty>(new PlateProperty(3, id));
                        }
                        else
                        {
                            Map.map[draw_edge.tile[j]].SetProperty<PlateProperty>(new PlateProperty(1, id));

                        }
                    }
                    if (direction.x != 0)
                    {
                        Position next_tile = draw_edge.tile[j] + new Position(0, 1);
                        while (!border.Contains(next_tile) && IsInside(next_tile))
                        {
                            Map.map[next_tile].SetProperty<PlateProperty>(new PlateProperty(0, id));
                            next_tile.y++;
                        }
                    }

                }
            }
        }
        public bool IsInside(Position selectedtile)
        {
            int below = 0;
            int above = 0;
            List<Tuple<Position, bool>> number_line = new List<Tuple<Position, bool>>();
            Dictionary<Position, int> corner_direction = new Dictionary<Position, int>();
            if (border.Contains(selectedtile))
            {
                return true;
            }
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
                throw new Exception("Inside algorithm failed " + selectedtile + ", " + number_line);
            }

            return inside;
        }
        protected bool Contains(Position position)
        {
            return edge.Exists((edge) => edge.tile.Contains(position));
        }
        public override string ToString()
        {
            return "The " + name + " " + base.ToString();
        }

        public void OnSelect()
        {

        }

        public void UnSelect()
        {

        }

        public void OnHover()
        {
        }

        public void UnHover()
        {
        }

        public void DeletePlate()
        {
            if(!plate.ContainsKey(id))
            {
                for (int i = 0; i < edge.Count; i++)
                {
                    Edge draw_edge = edge[i];
                    Position direction = edge[i].GetDirection();
                    int previous_index = i - 1;
                    for (int j = 0; j < draw_edge.tile.Count; j++)
                    {

                        if (Map.map[draw_edge.tile[j]].GetProperty<PlateProperty>().plate_id == id)
                        {
                           Map.map[draw_edge.tile[j]].SetProperty<PlateProperty>(new PlateProperty(0, Guid.Empty));
                        }
                        if (direction.x != 0)
                        {
                            Position next_tile = draw_edge.tile[j] + new Position(0, 1);
                            while (!border.Contains(next_tile) && IsInside(next_tile))
                            {
                                Map.map[next_tile].SetProperty<PlateProperty>(new PlateProperty(0, Guid.Empty));
                                next_tile.y++;
                            }
                        }

                    }
                }
            }
        }

        protected static string GenerateName()
        {
            List<string> names = new List<string>() {
            "Pangea",
            "Rodinia",
            "Laurasia",
            "Gondwana",
            "Amazonia",
            "Falkirk",
            "Paarsia",
            "Ruthenia",
            "Zealandia",
            "Pannotia",
            "Vaalbara",
            "Simmeria",
            "Kalaharia",
            "Baltica",
            "Avalonia",
            "Atlantica",
            "Superior",
            "Scalvia"
        };
            Debug.Log(names.Count);
            return names[plate.Count];
        }
    }
}