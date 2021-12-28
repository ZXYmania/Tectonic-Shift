using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;


namespace PlateSpace
{
    public class PlateMode : Mode
    {

        public static List<Position> draw_hover;
        public static List<Position> draw_selected;
        protected static bool waiting;
        protected static bool focused;
        Dictionary<string,Menu> m_menu;

        protected T GetMenu<T>() where T : Menu
        {
            return (T)m_menu[typeof(T).Name];
        }
        protected EditPlateMenu GetPlateMenu(Plate selected_plate)
        {
            if (!m_menu.ContainsKey(typeof(EditPlateMenu).Name))
            {
                m_menu[typeof(EditPlateMenu).Name] = Menu.CreateMenu<EditPlateMenu>();
            }
            EditPlateMenu menu = GetMenu<EditPlateMenu>();
            menu.SetPlate(selected_plate);
            return menu;
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
            m_menu = new Dictionary<string, Menu>();
            Tile.Clicked += OnClick;
            Tile.Hovered += OnHover;
            EditPlateMenu.Clicked += OnClick;
            TextField.Selected += Focus;
            TextField.UnSelected += Unfocus;
            SaveMenu.Save += OnClick;
            SaveMenu save = Menu.CreateMenu<SaveMenu>();
            Tile.draw_property = typeof(PlateProperty).Name;
        }

        void Update()
        {
            if (!focused)
            {
                MoveCamera();
            }
            if (selected.Count > 0)
            {
                if (!Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKeyDown(KeyCode.Mouse1))
                {

                    UnSelect(selected[selected.Count - 1]);
                }
                if (draw_selected.Count((item) => item == draw_selected[draw_selected.Count - 1]) > 1)
                {
                    draw_selected.AddRange(draw_hover);
                    try
                    {
                        Plate.CreatePlate(new List<Position>(draw_selected));
                    }
                    catch (CreatePlateError e)
                    {
                        SaveFile<CreatePlateErrorData>(new List<CreatePlateErrorData>() { e.m_data }, e.m_data.m_id + ".dat");
                    }
                    Clear();
                }
            }
        }
        public class PlateDrawContoller : DrawContoller
        {
            static JobHandle current_job;
            protected static FindPath path;

            public int GetDistance(Position from, Position to)
            {
                if (Map.map[to].GetProperty<PlateProperty>().plate_id != Guid.Empty)
                {
                    return -1;
                }
                Position result = new Position(to.x - from.x, to.y - from.y);
                return result.x * result.x + result.y * result.y;
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
            PlateProperty tile_property = given_tile.GetProperty<PlateProperty>();
            if (tile_property.plate_id != Guid.Empty)
            {
                Clear();
                Plate selected_plate = Plate.plate[tile_property.plate_id];
                EditPlateMenu menu = GetPlateMenu(selected_plate);
                if (selected_plate.border.Contains(given_tile.position))
                {
                    menu.OnSelect();
                }
                else
                {
                    selected.Add(selected_plate);
                    selected[0].OnSelect();
                    menu.OnSelect();
                }
            }
            else if (selected.Count > 1)
            {
                if (draw_hover.Count > 0 && draw_hover[draw_hover.Count - 1] == given_tile.position)
                {
                    for (int i = 0; i < draw_hover.Count; i++)
                    {
                        Map.map[draw_hover[i]].UnHover();
                        Map.map[draw_hover[i]].OnSelect();
                    }
                    draw_selected.AddRange(draw_hover);
                    draw_hover.Clear();
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
                List<CreatePlateErrorData> error = LoadFile<CreatePlateErrorData>("error.dat");
            }
            Clear();
        }


        public void OnHover(Tile given_tile)
        {
            if (selected.Count > 0)
            {
                if (selected[selected.Count - 1] is Tile)
                {
                    Tile start = (Tile)selected[selected.Count - 1];
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

        public void UnSelect(Clickable unselected)
        {
            if (unselected is Tile)
            {
                Tile unselected_tile = (Tile)unselected;
                unselected.UnSelect();
                unselected.UnHover();
                List<Position> unselected_list = draw_selected;
                if (selected.Count > 1)
                {
                    int index = draw_selected.IndexOf(((Tile)selected[selected.Count - 2]).position);
                    unselected_list = draw_selected.GetRange(index + 1, draw_selected.Count - index - 1);
                    draw_selected.RemoveRange(index + 1, draw_selected.Count - index - 1);
                }
                else
                {
                    draw_selected.Clear();
                    ClearHover();
                }
                for (int i = 0; i < unselected_list.Count; i++)
                {
                    Map.map[unselected_list[i]].UnSelect();
                }
                selected.RemoveAt(selected.Count - 1);
            }
            else if (selected[0] is Plate)
            {
                GetMenu<EditPlateMenu>().UnSelect();
                Clear();
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
            }
            draw_selected.Clear();
            ClearHover();
            base.Clear();
        }

        public void Focus(TextField given_field)
        {
            focused = true;
        }

        public void Unfocus(TextField given_field)
        {
            focused = false;
        }

        public override void OnModeEnter()
        {

        }

        public override void OnModeExit()
        {

        }
    }

}
