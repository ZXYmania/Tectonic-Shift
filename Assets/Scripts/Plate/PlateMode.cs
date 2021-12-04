using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class PlateMode : Mode
{
    public static List<Position> draw_hover;
    public static List<Position> draw_selected;
    protected static bool waiting;
    protected static bool short_cuts_off;
    EditPlateMenu m_menu;
    protected EditPlateMenu GetPlateMenu(Plate selected_plate)
    {
        if (m_menu == null)
        {
            m_menu = Menu.CreateMenu<EditPlateMenu>();
        }
        m_menu.SetPlate(selected_plate);
        return m_menu;
    }
    void Start()
    {
        if (current_menu == null)
        {
            SetUp();
        }
        Initialise();
    }

    public void Initialise()
    {
        Mode.current_menu = this;
        selected = new List<Clickable>();
        draw_hover = new List<Position>();
        draw_selected = new List<Position>();
        //m_menu = new List<Menu>();
        Tile.Clicked += OnClick;
        Tile.Hovered += OnHover;
        EditPlateMenu.Clicked += OnClick;
        TextField.Selected += Focus;
        TextField.UnSelected += Unfocus;
        SaveMenu.Save += OnClick;
        SaveMenu save = Menu.CreateMenu<SaveMenu>();
    }

    void Update()
    {
        if (!short_cuts_off)
        {
            MoveCamera();
        }
        if (!Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKeyDown(KeyCode.Mouse1))
        { 
            if (selected.Count > 0)
            {
                if (selected[0] is Tile)
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
                }
                else if (selected[0] is Plate)
                {
                    m_menu.UnSelect();
                    Clear();
                }
            }
        }
    }
    public class PlateDrawContoller : DrawContoller
    {
        static JobHandle current_job;
        protected static FindPath path;

        public int GetDistance(Position from, Position to)
        {
            if (Map.map[to].GetContinent() != Guid.Empty)
            {
                return -1;
            }
            Position result = new Position(to.x - from.x, to.y - from.y);
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

    public void OnClick(Tile given_tile)
    {
        selected.Add(given_tile);
        if (given_tile.GetContinent() != Guid.Empty)
        {
            try
            {
                Clear();
                Plate selected_plate = Plate.plate[given_tile.GetContinent()];
                GetPlateMenu(selected_plate);
                selected.Add(selected_plate);
                selected[0].OnSelect();
                m_menu.OnSelect();
                Debug.Log(selected[0] + " " + selected_plate.id);
            }
            catch(Exception e)
            {
                Debug.Log(given_tile + " " + e);
            }
        }
        else if (selected.Count > 1)
        {
            if (draw_hover.Count > 0 && draw_hover[draw_hover.Count - 1] == given_tile.position)
            {
                if (draw_selected.Contains(given_tile.position))
                {
                    draw_selected.AddRange(draw_hover);
                    ClearHover();
                    try
                    {
                        Plate.CreatePlate(new List<Position>(draw_selected));
                    }
                    catch (Exception e)
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
                selected.RemoveAt(selected.Count - 1);
            }
        }

    }

    public void OnClick(EditPlateMenu given_menu)
    {
        Debug.Log("Menu event started");
    }

    public void OnClick(bool save)
    {
        if (save)
        {
            SaveFile<Plate>(Plate.plate.Values.ToList());
        }
        else
        {
            List<Plate> load = LoadFile<Plate>();
            Plate.plate.Clear();
            for (int i = 0; i < load.Count; i++)
            {
                Plate.CreatePlate((load[i]));
            }
        }
    }


    public void OnHover(Tile given_tile)
    {
        if (selected.Count > 0)
        {
            if (selected[selected.Count-1] is Tile )
            {
                Tile start = (Tile) selected[selected.Count - 1];
                Tile destination = given_tile;
                List<Position> draw_copy = new List<Position>(draw_hover);
                PlateDrawContoller.EndFindPath();
                ClearHover();
                PlateDrawContoller.ScheduleFindPath(start, destination);
            }
        }
        given_tile.OnHover();
    }

    public void UnHover(Tile given_tile)
    {
        given_tile.UnHover();
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
        }
        draw_selected.Clear();
        base.Clear();
    }

    public void Focus(TextField given_field)
    {
        short_cuts_off = true;
    }

    public void Unfocus(TextField given_field)
    {
        short_cuts_off = false;
    }

    public override void OnModeEnter()
    {
        
    }

    public override void OnModeExit()
    {
        
    }
}
