using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Map
{
    //1000,500 has serious perfomance issues after load
    // 800,400 runs fine but allows for big tasks to be kicked off
    public static Position size { get; protected set; } = new Position(200, 100);
    public static int scale { get; protected set; }
    public static Dictionary<Position, Tile> map { get; protected set; }
    public static void Initialise()
    {
        scale = Mathf.CeilToInt(40000 / size.x);
        Debug.Log(scale);
        if (map is null)
        {
            map = new Dictionary<Position, Tile>();
        }
        CreateMap();
    }
    public class NoPathException : System.Exception
    {
        public NoPathException(string message) : base(message)
        {

        }
    }

    public class MapBuilder : MonoBehaviour
    {
        protected Coroutine build;
        private void Start()
        {
            build = StartCoroutine(BuildMap());
        }

        protected IEnumerator BuildMap()
        {
            if (!Map.map.ContainsKey(new Position(size.x - 1, size.y - 1)))
            {
                for (int i = 0; i < Map.size.x; i++)
                {
                    for (int j = 0; j < Map.size.y; j++)
                    {
                        Tile curr_tile = Tile.CreateTile(i, j);
                        Map.map.Add(curr_tile.position, curr_tile);
                    }
                    yield return null;
                }
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

    }
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
        public class WasteComparer : IComparer<Position>
        {
            Dictionary<Position, AStarData> data_map;
            public WasteComparer(Dictionary<Position, AStarData> data_map)
            {
                this.data_map = data_map;
            }
            public int Compare(Position x, Position y)
            {
                int waste = data_map[x].waste.CompareTo(data_map[y].waste);
                if ( waste == 0)
                {
                    return x.GetHashCode().CompareTo(y.GetHashCode());
                }
                return waste;
            }
        }

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

    public struct FindPath<T> : IJob where T : struct, DrawController
    {
        public static event Action<List<Position>> FoundPath = delegate { };
        public static event Action<Map.NoPathException> NoPath = delegate { };
        public static event Action Finish = delegate { };
        public Position m_start;
        public Position m_destination;
        public T m_controller;
        public static HashSet<Guid> current = new HashSet<Guid>();
        public Guid m_id;
        public FindPath(Tile start, Tile destination)
        {
            m_id = Guid.NewGuid();
            m_start = start.position;
            m_destination = destination.position;
            m_controller = new T();
            m_controller.Initialise();
        }

        public FindPath(Tile start, Tile destination, T given_controller)
        {
            m_id = Guid.NewGuid();
            m_start = start.position;
            m_destination = destination.position;
            m_controller = given_controller;
            m_controller.Initialise();
        }

        public void Execute()
        {
            if (m_start == m_destination)
            {
                FoundPath(new List<Position>() { m_start });
            }
            else
            {
                Dictionary<Position, AStarData> data_map = new Dictionary<Position, AStarData>();
                AStarData.WasteComparer waste_comparer = new AStarData.WasteComparer(data_map);
                SortedSet<Position> quick_search = new SortedSet<Position>(waste_comparer);
                SortedSet<Position> medium_search = new SortedSet<Position>(waste_comparer);
                SortedSet<Position> slow_search = new SortedSet<Position>(waste_comparer);
                SortedSet<Position> backwards_search = new SortedSet<Position>(waste_comparer);

                // first position is tile, second position is x = distance travelled and y = waste
                quick_search.Add(m_start);
                data_map.Add(m_start, new AStarData(new Position(-1, -1), 0, 0));
                List<Position> result = new List<Position>();
                Position current_tile = m_start;
                bool path_found = false;
                while (!path_found && current.Contains(m_id) && (quick_search.Count + medium_search.Count + slow_search.Count > 0))
                {
                    AStarData current_data = new AStarData(data_map[current_tile], true);
                    data_map[current_tile] = current_data;
                    Position goal_direction = GetDirection(current_tile, m_destination);
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Position adjacent_tile = GetAdjacent(current_tile, i, j);
                            if (map.ContainsKey(adjacent_tile))
                            {
                                if (adjacent_tile == m_destination)
                                {
                                    if (m_controller.GetDistance(current_tile, adjacent_tile) > 0)
                                    {
                                        float distance = current_data.distance + m_controller.GetDistance(current_tile, adjacent_tile);
                                        float waste = current_data.waste;
                                        data_map.Add(adjacent_tile, new AStarData(current_tile, distance, waste));
                                        path_found = true;
                                    }
                                }
                                else if (data_map.ContainsKey(adjacent_tile))
                                {
                                    float distance = m_controller.GetDistance(current_tile, adjacent_tile);
                                    distance += current_data.distance;
                                    if (distance < data_map[adjacent_tile].distance)
                                    {
                                        data_map[adjacent_tile] = new AStarData(current_tile, distance, data_map[adjacent_tile].waste);
                                    }
                                }
                                else
                                {
                                    float distance = m_controller.GetDistance(current_tile, adjacent_tile);
                                    if (distance > 0)
                                    {
                                        float waste = (goal_direction.x - i) * (goal_direction.x - i) + (goal_direction.y - j) * (goal_direction.y - j);
                                        data_map.Add(adjacent_tile, new AStarData(current_tile, distance + current_data.distance, current_data.waste + waste));

                                        if (waste < 0.5)
                                        {
                                            quick_search.Add(adjacent_tile);
                                        }
                                        else if (waste <= 2)
                                        {
                                            medium_search.Add(adjacent_tile);
                                        }
                                        else
                                        {
                                            slow_search.Add(adjacent_tile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (quick_search.Count > 0)
                    {
                        current_tile = quick_search.Min;
                        quick_search.Remove(current_tile);
                    }
                    else if (medium_search.Count > 0)
                    {
                        current_tile = medium_search.Min;
                        medium_search.Remove(current_tile);
                    }
                    else if (slow_search.Count > 0)
                    {
                        current_tile = slow_search.Min;
                        slow_search.Remove(current_tile);
                    }
                }
                if (!data_map.ContainsKey(m_destination))
                {
                    NoPath(new NoPathException(m_start + " to " + m_destination + " has no path"));
                }
                else
                { 
                    AStarData item = data_map[m_destination];
                    result.Add(m_destination);
                    while (data_map.ContainsKey(item.previous_tile) && result[result.Count - 1] != item.previous_tile)
                    {
                        result.Add(item.previous_tile);
                        item = data_map[item.previous_tile];
                    }
                    result.Reverse();
                    FoundPath(result);
                }
            }
        }
    }
}
