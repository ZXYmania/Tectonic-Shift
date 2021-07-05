using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class PlateMode : Mode
{
    public static List<Position> draw;
    protected static bool waiting;
    public PlateMode()
    {
        Initialise();
    }

    public class PlateDrawContoller : DrawContoller
    {
        static JobHandle current_job;
        protected static FindPath path;

        public int GetDistance(Position from, Position to)
        {
            Position result = new Position(to.x - from.x, to.y - from.y);
            if (Map.map[from].edge || Map.map[to].edge || (Map.map[Map.GetAdjacent(from, result.x, 0)].edge && Map.map[Map.GetAdjacent(from, 0, result.y)].edge))
            {
                return -1;
            }

            return Mathf.Abs(result.x) + Mathf.Abs(result.y);
        }

        public bool QuickStop()
        {
            return waiting;
        }


        public static void ScheduleFindPath(Tile start, Tile end)
        {
            EndFindPath();
            waiting = false;
            path = new FindPath(start, end);
            current_job = path.Schedule();
        }

        public static void EndFindPath()
        {
            if (!current_job.IsCompleted)
            {
                waiting = true;
                current_job.Complete();
            }
        }
    }

    public struct FindPath : IJob
    {
        public Position start;
        public Position destination;
        public FindPath(Tile start, Tile destination)
        {
            this.start = start.position;
            this.destination = destination.position;
        }

        public void Execute()
        {
            try
            {
                draw = Map.DrawLine(start, destination, new PlateDrawContoller());
                for (int i = 0; i < draw.Count; i++)
                {
                    Map.map[draw[i]].OnHover();
                }
            }
            catch (Map.NoPathException)
            {

            }
        }
    }

    public override void OnSelect()
    {
        if (selected.Count == 2)
        {
            if (draw.Count > 0)
            {
                if (draw[draw.Count-1] == ((Tile)selected[1]).position)
                {
                    for (int i = 0; i < draw.Count; i++)
                    {
                        Map.map[draw[i]].SetTerrain();
                        Map.map[draw[i]].UnHover();
                    }
                }
                else
                {
                    Debug.Log(draw[draw.Count - 1] + ", " + ((Tile)selected[1]).position);
                }
            }
            Clear();
        }

    }

    public override void OnHover()
    {
        if (selected.Count == 1)
        {
            if (selected[0] is Tile && hover is Tile)
            {
                Tile start = (Tile) selected[0];
                Tile destination = (Tile)hover;
                List<Position> draw_copy = new List<Position>(draw);
                PlateDrawContoller.EndFindPath();
                for (int i = 1; i < draw.Count; i++)
                {
                    Map.map[draw[i]].UnHover();
                }
                PlateDrawContoller.ScheduleFindPath(start, destination);
            }
        }
    }

    public void Initialise()
    {
        Mode.current_menu = this;
        selected = new List<Clickable>();
        draw = new List<Position>();
    }
}
