using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlateSpace.PlateMode;

namespace PlateSpace
{ 
    public class DrawPlateState : PlateState
    {
        List<Tile> selected_tile;
        List<Position> draw_tile;
        List<Position> draw_hover;
        Tile hover;
        Tile clicked;
        Mode.PathScheduler<PlateDrawController> m_controller;
        bool path_completed;

        public DrawPlateState(Tile given_tile)
        {
            selected_tile = new List<Tile> { given_tile };
            draw_tile = new List<Position>();
            draw_hover = new List<Position>();
        }
        public override void OnActionEnter()
        {
            Debug.Log("Create state entered");
            for(int i = 0; i < selected_tile.Count; i++)
            {
                if(selected_tile[i].GetProperty<PlateProperty>().plate_id != Guid.Empty)
                {
                    PlateMode.ChangeState(new DefaultState());
                }
                selected_tile[i].UnHover();
                selected_tile[i].OnSelect();
            }
            Map.FindPath<PlateDrawController>.FoundPath += DrawComplete;
            base.OnActionEnter();
        }

        public override void OnActionLeave()
        {
            Clear();
            Map.FindPath<PlateDrawController>.FoundPath -= DrawComplete;
           base.OnActionLeave();
        }

        public void DrawComplete(List<Position> draw_path)
        {
            ClearHover();
            for(int i = 0; i < draw_path.Count; i++)
            {
                if (!Map.map[draw_path[i]].selected)
                {
                    Map.map[draw_path[i]].OnHover();
                }
            }
            draw_hover = draw_path;
            path_completed = true;
        }

        public override void CommitAction()
        {
            if (path_completed && clicked != null)
            {
                selected_tile.Add(clicked);
                for (int i = 0; i < draw_hover.Count; i++)
                {
                    Map.map[draw_hover[i]].UnHover();
                    Map.map[draw_hover[i]].OnSelect();
                }
                if (draw_tile.Contains(clicked.position))
                {
                    draw_tile.AddRange(draw_hover);
                    try
                    {
                        Plate.CreatePlate(new List<Position>(draw_tile));
                    }
                    catch (CreatePlateError e)
                    {
                        Mode.SaveFile<CreatePlateErrorData>(new List<CreatePlateErrorData>() { e.m_data }, e.m_data.m_id + ".dat");
                    }
                    ChangeState(new DefaultState());
                }
                else
                {
                    for(int i = 0; i < draw_hover.Count; i++)
                    {
                        Tile current_tile = Map.map[draw_hover[i]];
                        current_tile.UnHover();
                        current_tile.OnSelect();
                        draw_tile.Add(current_tile.position);
                    }
                }

            }
            clicked = null;
        }

        public override void OnClick(Tile given_tile)
        {
            clicked = given_tile;
        }

        public override void OnHover(Tile given_tile)
        {
            if (hover != given_tile)
            {
                Tile start = selected_tile[selected_tile.Count - 1];
                path_completed = false;
                if(m_controller != null)
                {
                    m_controller.EndJob();
                    ClearHover();
                }
                m_controller = new Mode.PathScheduler<PlateDrawController>();
                m_controller.ScheduleFindPath(start, given_tile);
                hover = given_tile;
            }
        }

        public override void Unselect()
        {
            if(clicked == null)
            {
                selected_tile[selected_tile.Count - 1].UnSelect();
                selected_tile.RemoveAt(selected_tile.Count - 1);
                if (selected_tile.Count == 0)
                {
                    ChangeState(new DefaultState());
                    return;
                }
                int index = draw_tile.LastIndexOf(selected_tile[selected_tile.Count - 1].position);
                List<Position> to_remove = draw_tile.GetRange(index, draw_tile.Count - index);
                draw_tile.RemoveRange(index, draw_tile.Count - index);
                for(int i = 0; i < to_remove.Count; i++)
                {
                    Map.map[to_remove[i]].UnSelect();
                }
                Tile redraw = hover;
                hover = null;
                OnHover(redraw);
            }
            clicked = null;
        }
        public void ClearHover()
        {
            for (int i = 1; i < draw_hover.Count; i++)
            {
                Map.map[draw_hover[i]].UnHover();
            }
            draw_hover.Clear();
        }

        public override void Clear()
        {
            for (int i = 0; i < selected_tile.Count; i++)
            {
                selected_tile[i].UnSelect();
            }
            selected_tile.Clear();
            for (int i = 0; i < draw_tile.Count; i++)
            {
                Map.map[draw_tile[i]].UnSelect();
            }
            draw_tile.Clear();
            ClearHover();
            clicked = null;
        }
    }
    public class EditPlateState : PlateState
    {
        Plate m_plate;
        EditPlateMenu m_menu;
        List<Tile> selected_tile;
        List<Position> draw_tile;
        List<Position> draw_hover;
        Tile hover;
        Tile clicked;

        public EditPlateState(Plate given_plate)
        {
            m_plate = given_plate;
            selected_tile = new List<Tile>();
            draw_tile = new List<Position>();
            draw_hover = new List<Position>();
        }

        public override void OnActionEnter()
        {
            m_menu = Menu.CreateMenu<EditPlateMenu>();
            m_menu.SetPlate(m_plate);
            EditPlateMenu.Clicked += OnClick;
            base.OnActionEnter();
        }
        public override void OnActionLeave()
        {
            m_menu = null;
            EditPlateMenu.Clicked -= OnClick;
            base.OnActionLeave();
        }
        public override void CommitAction()
        {
        }

        public override void OnClick(Tile given_tile)
        {

            if(given_tile.GetProperty<PlateProperty>().plate_id != m_plate.id)
            {
                ChangeState(new DefaultState());
            }
        }

        public void OnClick(EditPlateMenu given_menu)
        {

        }

        public override void OnHover(Tile given_tile)
        {
        }

        public override void Unselect()
        {
            throw new NotImplementedException();
        }
        public override void Clear()
        {
            for (int i = 0; i < selected_tile.Count; i++)
            {
                selected_tile[i].UnSelect();
            }
            selected_tile.Clear();
            for (int i = 0; i < draw_tile.Count; i++)
            {
                Map.map[draw_tile[i]].UnSelect();
            }
            draw_tile.Clear();
            ClearHover();
            clicked = null;
            m_menu.UnSelect();
        }
        public void ClearHover()
        {
            for (int i = 1; i < draw_hover.Count; i++)
            {
                Map.map[draw_hover[i]].UnHover();
            }
            draw_hover.Clear();
        }
    }
}

