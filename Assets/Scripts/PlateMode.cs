using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class PlateMode : Mode
{
    public static List<Position> draw_hover;
    public static List<Position> draw_selected;
    protected static bool waiting;

    void Start()
    {
        if (current_menu == null)
        {
            SetUp();
        }
        Initialise();
    }

    void Update()
    {
        MoveCamera();
        if(Input.GetKeyDown( KeyCode.Space))
        {
            SaveFile<Plate>(Plate.plate);
        }
        if(Input.GetKeyDown(KeyCode.Return))
        {
            List<Plate> load = LoadFile<Plate>();
            for(int i = 0; i < load.Count; i++)
            {
                Plate.CreatePlate((load[i]));
            }
        }
        if(!Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (selected.Count > 0)
            {
                selected[selected.Count - 1].UnSelect();
                selected[selected.Count - 1].UnHover();
                List<Position> unselected = draw_selected;
                if (selected.Count > 1)
                {
                    int index = draw_selected.IndexOf(((Tile)selected[selected.Count - 2]).position);
                    unselected = draw_selected.GetRange(index + 1, draw_selected.Count - index - 1);
                    draw_selected.RemoveRange(index + 1, draw_selected.Count - index - 1);
                }
                else
                {
                    draw_selected.Clear();
                    ClearHover();
                }
                for (int i = 0; i < unselected.Count; i++)
                {
                    Map.map[unselected[i]].UnSelect();
                }
                selected.RemoveAt(selected.Count - 1);
                OnHover();
            }
        }
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
            return result.x*result.x + result.y*result.y;
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
                draw_hover = Map.DrawLine(start, destination, new PlateDrawContoller());
                for (int i = 0; i < draw_hover.Count; i++)
                {
                    Map.map[draw_hover[i]].OnHover();
                }
            }
            catch (Map.NoPathException)
            {
                Map.map[destination].UnHover();
            }
        }
    }

    public override void OnSelect()
    {
        if (selected.Count > 1)
        {
            Tile destination = (Tile)selected[selected.Count - 1];
            if (draw_hover.Count > 0 && draw_hover[draw_hover.Count - 1] == destination.position)
            {
                if (draw_selected.Contains(destination.position))
                {
                    draw_selected.AddRange(draw_hover);
                    draw_hover.Clear();
                    try
                    {
                        Plate.CreatePlate(new List<Position>(draw_selected));
                    }
                    catch(Exception e)
                    {
                        Plate.CreatePlate(new List<Position>(draw_selected));
                        throw e;
                    }
                    Clear();
                }
                else
                {
                    for (int i = 0; i < draw_hover.Count; i++)
                    {
                        Map.map[draw_hover[i]].UnHover();
                        Map.map[draw_hover[i]].OnSelect();
                    }
                    draw_selected.AddRange(draw_hover);
                    draw_hover.Clear();
                }
            }
            else
            {
                selected[selected.Count - 1].UnSelect();
                selected[selected.Count - 1].UnHover();
                selected.RemoveAt(selected.Count - 1);
            }
        }
    }

    public override void OnHover()
    {
        if (selected.Count > 0)
        {
            if (selected[selected.Count-1] is Tile && hover is Tile)
            {
                Tile start = (Tile) selected[selected.Count - 1];
                Tile destination = (Tile)hover;
                List<Position> draw_copy = new List<Position>(draw_hover);
                PlateDrawContoller.EndFindPath();
                ClearHover();
                PlateDrawContoller.ScheduleFindPath(start, destination);
            }
        }
    }

    public static void ClearHover()
    {
        for (int i = 1; i < draw_hover.Count; i++)
        {
            Map.map[draw_hover[i]].UnHover();
        }
        draw_hover.Clear();
    }

    public override void Clear()
    {
        for (int i = 0; i < draw_selected.Count; i++)
        {
            Map.map[draw_selected[i]].UnSelect();
            Map.map[draw_selected[i]].UnHover();
        }
        draw_selected.Clear();
        base.Clear();
    }

    public void Initialise()
    {
        Mode.current_menu = this;
        selected = new List<Clickable>();
        draw_hover = new List<Position>();
        draw_selected = new List<Position>();
    }

    public override void OnModeEnter()
    {
        
    }

    public override void OnModeExit()
    {
        
    }
}
