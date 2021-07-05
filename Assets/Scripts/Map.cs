using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Jobs;
using UnityEngine;

public class Map
{
    public class NoPathException : System.Exception
    {
        public NoPathException(string message) : base(message)
        {

        }
    }
    public static Dictionary<Position, Tile> map { get; protected set; }
    public struct AStarData
    {
        public Position previous_tile;
        public float distance;
        public float waste;
        public bool ventured;

        public AStarData(Position tile, float distance, float waste)
        {
            this.previous_tile = tile;
            this.distance = distance;
            this.waste = waste;
            this.ventured = false;
        }
        public AStarData(Position tile, float distance, float waste, bool ventured)
        {
            this.previous_tile = tile;
            this.distance = distance;
            this.waste = waste;
            this.ventured = ventured;
        }
        public AStarData(AStarData data, bool ventured)
        {
            this.previous_tile = data.previous_tile;
            this.distance = data.distance;
            this.waste = data.waste;
            this.ventured = ventured;
        }
    }

    public static Position size { get; protected set; } = new Position(150, 75);
    
    public static void Initialise()
    {
        if(map is null)
        {
            map = new Dictionary<Position, Tile>();
        }
    }
    public static void AddColumn(int rowX)
    {
        for (int j = 0; j < size.y; j++)
        {
            Tile curr_tile = Tile.CreateTile(rowX, j);
            Map.map.Add(curr_tile.position, curr_tile);
        }
    }



    public static Position GetAdjacent(Position tile, int i, int j)
    {
        int return_x = tile.x + i;
        int return_y = tile.y + j;
        if (return_x >= size.x)
        {
            return_x = 0;
        }
        else if (return_x < 0)
        {
            return_x = size.x - 1;
        }
        return new Position(return_x, return_y);
    }

    public static Position GetDirection(Position start, Position destination)
    {
        Position goal_direction = new Position(destination.x - start.x, destination.y - start.y);
        if (Mathf.Abs(goal_direction.x) > size.x/2 )
        {
            goal_direction.x = goal_direction.x * -1;
        }
        if (goal_direction.x != 0)
        {
            goal_direction.x = goal_direction.x / Mathf.Abs(goal_direction.x);
        }
        if (goal_direction.y != 0)
        {
            goal_direction.y = goal_direction.y / Mathf.Abs(goal_direction.y);
        }
        return goal_direction;
    }
    public static List<Position> DrawLine(Tile start, Tile destination, DrawContoller draw_option)
    {
        if(start == destination)
        {
            return new List<Position>() { start.position };
        }
        return DrawLine(start.position, destination.position, draw_option);
    }

    public static List<Position> DrawLine(Position start, Position destination, DrawContoller draw_option)
    {
        Stopwatch timer = new Stopwatch();
        timer.Reset();
        timer.Start();
        if(start == destination)
        {
            return new List<Position>() { start };
        }
        Dictionary<Position, AStarData> data_map = new Dictionary<Position, AStarData>();
        List<Position> quick_search = new List<Position>();
        List<Position> medium_search = new List<Position>();
        List<Position> slow_search = new List<Position>();
        // first position is tile, second position is x = distance travelled and y = waste
        quick_search.Add(start);
        data_map.Add(start, new AStarData(new Position(-1, -1), 0, 0));
        List<Position> result = new List<Position>();
        Position current_tile = start;
        bool path_found = false;
        while (quick_search.Count + medium_search.Count + slow_search.Count > 0  && !path_found && !draw_option.QuickStop())
        {
            data_map[current_tile] = new AStarData(data_map[current_tile], true);
            Position goal_direction = GetDirection(current_tile, destination);
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Position adjacent_tile = GetAdjacent(current_tile, i, j);
                    if (map.ContainsKey(adjacent_tile))
                    {
                        if (adjacent_tile == destination)
                        {
                            if (draw_option.GetDistance(current_tile, adjacent_tile) > 0)
                            {
                                float distance = data_map[current_tile].distance + draw_option.GetDistance(current_tile, adjacent_tile);
                                float waste = data_map[current_tile].waste;
                                data_map.Add(adjacent_tile, new AStarData(current_tile, distance, waste));
                                path_found = true;
                            }
                        }
                        else
                        {
                            float distance = draw_option.GetDistance(current_tile, adjacent_tile);
                            if (distance > 0)
                            {
                                distance += data_map[current_tile].distance;
                                float waste = data_map[current_tile].waste + Mathf.Abs(goal_direction.x - i) + Mathf.Abs(goal_direction.y - j);
                                if (!data_map.ContainsKey(adjacent_tile))
                                {
                                    data_map.Add(adjacent_tile, new AStarData(current_tile, distance, waste));
                                }
                                else
                                {
                                    if (data_map[adjacent_tile].distance > distance)
                                    {
                                        data_map[adjacent_tile] = new AStarData(current_tile, distance, waste, data_map[adjacent_tile].ventured);
                                    }
                                }
                                if (!data_map[adjacent_tile].ventured)
                                {
                                    if (waste == data_map[current_tile].waste)
                                    {
                                        if (!quick_search.Contains(adjacent_tile))
                                        {
                                            quick_search.Add(adjacent_tile);
                                        }
                                        if (medium_search.Contains(adjacent_tile))
                                        {
                                            medium_search.Remove(adjacent_tile);
                                        }
                                        else if (slow_search.Contains(adjacent_tile))
                                        {
                                            slow_search.Remove(adjacent_tile);
                                        }
                                    }
                                    else if (waste - data_map[current_tile].waste < 2)
                                    {
                                        if (!medium_search.Contains(adjacent_tile))
                                        {
                                            medium_search.Add(adjacent_tile);
                                        }
                                        if (slow_search.Contains(adjacent_tile))
                                        {
                                            slow_search.Remove(adjacent_tile);
                                        }
                                    }
                                    else
                                    {
                                        if (!slow_search.Contains(adjacent_tile))
                                        {
                                            slow_search.Add(adjacent_tile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (quick_search.Count > 0)
            {
                //Sort Quick Search
                quick_search.Sort((tile1, tile2) => data_map[tile1].waste.CompareTo(data_map[tile2].waste));
                current_tile = quick_search[0];
                quick_search.Remove(current_tile);
            }
            else if (medium_search.Count > 0)
            {
                //Sort medium_search
                medium_search.Sort((tile1, tile2) => data_map[tile1].waste.CompareTo(data_map[tile2].waste));
                current_tile = medium_search[0];
                medium_search.Remove(current_tile);
            }
            else if (slow_search.Count > 0)
            {
                slow_search.Sort((tile1, tile2) => data_map[tile1].waste.CompareTo(data_map[tile2].waste));
                current_tile = slow_search[0];
                slow_search.Remove(current_tile);
            }
        }
        if (!data_map.ContainsKey(destination))
        {
            throw new NoPathException(start +" to "+ destination +" has no path");
        }
        AStarData item = data_map[destination];
        result.Add(destination);
        while ( data_map.ContainsKey(item.previous_tile))
        {

            result.Add(item.previous_tile);
            item = data_map[item.previous_tile];
        }
        result.Reverse();
        return result;
    }
}
