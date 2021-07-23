using System;
using System.Collections;
using System.Collections.Generic;
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

    public class MapBuilder : MonoBehaviour
    {
        public Position upto;
        private void Start()
        {
            upto = new Position(0, 0);
        }
        void Update()
        {
            int count = 0;
            int rate_limit = 50;
            if (upto.y >= Map.size.y)
            {
                upto.x++;
                upto.y = 0;
            }
            if (!Map.map.ContainsKey(new Position(size.x -1, size.y -1)))
            {
                for (int i = upto.x; i < Map.size.x; i++)
                {

                    for (int j = upto.y; j < Map.size.y; j++)
                    {
                        upto.y = 0;
                        Tile curr_tile = Tile.CreateTile(i, j);
                        Map.map.Add(curr_tile.position, curr_tile);
                        if( count++ > rate_limit)
                        {
                            upto = new Position(i, j+1);
                            i = Map.size.x+1;
                            j = Map.size.y+1;
                        }
                    }
                }
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

    }
    public static Dictionary<Position, Tile> map { get; protected set; }

    public struct AStarData
    {
        public Position previous_tile;
        public float distance;
        public float waste;
        public bool ventured;
        public float search_criteria;
        public AStarData(Position tile, float distance, float waste)
        {
            this.previous_tile = tile;
            this.distance = distance;
            this.waste = waste;
            this.ventured = false;
            search_criteria = this.waste * size.x + this.distance;
        }
        public AStarData(Position tile, float distance, float waste, bool ventured)
        {
            this.previous_tile = tile;
            this.distance = distance;
            this.waste = waste;
            this.ventured = ventured;
            search_criteria = this.waste * size.x + this.distance;
        }
        public AStarData(AStarData data, bool ventured)
        {
            this.previous_tile = data.previous_tile;
            this.distance = data.distance;
            this.waste = data.waste;
            this.ventured = ventured;
            search_criteria = this.waste * size.x + this.distance;
        }

    }

    public static Position size { get; protected set; } = new Position(250, 125);
    public static int scale { get; protected set; }

    public static void Initialise()
    {
        scale = Mathf.CeilToInt(40000 / size.x);
        Debug.Log(scale);
        if(map is null)
        {
            map = new Dictionary<Position, Tile>();
        }
        CreateMap();
    }

    protected static void CreateMap()
    {
        GameObject gameObject = new GameObject();
        gameObject.AddComponent<MapBuilder>();
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

    public static bool IsAdjacent(Position from, Position to)
    {
        int x = from.x - to.x;
        if((to.x == 0 && from.x == size.x) || (to.x == size.x && from.x == 0))
        {
            x = 1;
        }
        Position dif = new Position(x*x, (to.y - from.y) * (to.y-from.y));
        return dif.x < 2 && dif.y < 2;
    }

    public static Position GetDirection(Position from, Position to)
    {
        Position result = new Position((to.x - from.x).CompareTo(0), (to.y - from.y).CompareTo(0));
        if (Mathf.Abs(to.x-from.x) > size.x - Mathf.Abs(to.x - from.x))
        {
            result.x = result.x * -1;
        }
        return result;
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
        while (!path_found && !draw_option.QuickStop() && (quick_search.Count > 0 || medium_search.Count > 0 || slow_search.Count > 0 ))
        {
            AStarData current_data = new AStarData(data_map[current_tile], true);
            data_map[current_tile] = current_data;
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
                                float distance = current_data.distance + draw_option.GetDistance(current_tile, adjacent_tile);
                                float waste = current_data.waste;
                                data_map.Add(adjacent_tile, new AStarData(current_tile, distance, waste));
                                path_found = true;
                            }
                        }
                        else
                        {
                            float distance = draw_option.GetDistance(current_tile, adjacent_tile);
                            if (distance > 0 && Mathf.Abs(goal_direction.x + i) + Mathf.Abs(goal_direction.y + j) != 0)
                            {
                                distance += current_data.distance;
                                float waste = current_data.waste + (goal_direction.x - i ) * (goal_direction.x - i ) + (goal_direction.y - j ) * (goal_direction.y - j ) ;
                                if (!data_map.ContainsKey(adjacent_tile))
                                {
                                    data_map.Add(adjacent_tile, new AStarData(current_tile, distance, waste));
                                }
                                else
                                {
                                    if (data_map[adjacent_tile].distance > distance || data_map[adjacent_tile].waste > waste)
                                    {
                                        data_map[adjacent_tile] = new AStarData(current_tile, distance, waste, data_map[adjacent_tile].ventured);
                                        if (quick_search.Contains(adjacent_tile))
                                        {
                                            quick_search.Remove(adjacent_tile);
                                        }
                                        else if (medium_search.Contains(adjacent_tile))
                                        {
                                            medium_search.Remove(adjacent_tile);

                                        }
                                        else if (slow_search.Contains(adjacent_tile))
                                        {
                                            slow_search.Remove(adjacent_tile);
                                        }
                                    }
                                }
                                if (!data_map[adjacent_tile].ventured 
                                    && ! quick_search.Contains(adjacent_tile)
                                    && ! medium_search.Contains(adjacent_tile)
                                    && ! slow_search.Contains(adjacent_tile))
                                {
                                     if (waste - current_data.waste < 0.5 )
                                    {
                                        //Add item into sorted quick_search

                                        int index = quick_search.FindLastIndex((tile) => data_map[adjacent_tile].search_criteria > data_map[tile].search_criteria);
                                        if (index == 0 || index == -1)
                                        {
                                            quick_search.Insert(0, adjacent_tile);
                                        }
                                        else
                                        {
                                            quick_search.Insert(index + 1, adjacent_tile) ;

                                        }
                                    }
                                    else if (waste - current_data.waste <= 2)
                                    {
                                        //Add item into sorted medium_search

                                        int index = medium_search.FindLastIndex((tile) => data_map[adjacent_tile].search_criteria > data_map[tile].search_criteria);
                                        if (index == 0 || index == -1)
                                        {
                                            medium_search.Insert(0, adjacent_tile);
                                        }
                                        else
                                        {
                                            medium_search.Insert(index+1, adjacent_tile);
                                        }
                                    }
                                    else if (waste - current_data.waste <= 5)
                                    {
                                        //Add item into sorted slow_search
                                        int index = slow_search.FindLastIndex((tile) => data_map[adjacent_tile].search_criteria > data_map[tile].search_criteria);
                                        if (index == 0 || index == -1)
                                        {
                                            slow_search.Insert(0, adjacent_tile);
                                        }
                                        else
                                        {
                                            slow_search.Insert(index + 1, adjacent_tile);
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
                current_tile = quick_search[0];
                quick_search.Remove(current_tile);
            }
            else if (medium_search.Count > 0)
            {
                current_tile = medium_search[0];
                medium_search.Remove(current_tile);
            }
            else if (slow_search.Count > 0)
            {
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
