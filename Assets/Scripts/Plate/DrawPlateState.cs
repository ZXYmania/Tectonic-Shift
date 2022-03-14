using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlateSpace.PlateMode;

namespace PlateSpace
{
    public class TextFieldAction : UserAction
    {
        InputFieldLayer m_field;
        public TextFieldAction(InputFieldLayer given_field)
        {
            m_field = given_field;
        }
        public void Execute()
        {
            Mode.ChangeFocus(m_field);
        }

        public int GetPriority()
        {
            return 5;
        }

        public bool IsPermanent()
        {
            return false;
        }

        public void Undo()
        {
            m_field.Unselect();
            Mode.ChangeFocus(m_field);
        }
    }
    public class DrawPlateState : PlateState
    {
        public class CreatePlateAction : UserAction
        {
            public List<Position> m_perimeter;
            public Guid m_id;
            public CreatePlateAction(List<Position> given_perimeter)
            {
                m_perimeter = given_perimeter;
            }
            public void Execute()
            {
                try
                {
                    m_id = Plate.CreatePlate(m_perimeter).id;
                }
                catch (CreatePlateError e)
                {
                    Mode.SaveFile<CreatePlateErrorData>(new List<CreatePlateErrorData>() { e.m_data }, e.m_data.m_id + ".dat");
                }
                ChangeState(new DefaultState());
                
            }

            public int GetPriority()
            {
                return 2;
            }

            public bool IsPermanent()
            {
                return true;
            }

            public void Undo()
            {
                if(m_id != Guid.Empty)
                {
                    Plate deleted_plate = Plate.plate[m_id];
                    Plate.plate.Remove(m_id);
                    deleted_plate.DeletePlate();
                    m_id = Guid.Empty;
                }
            }
        }
        public class DrawAction : UserAction
        {
            List<Position> m_line;
            public DrawAction(List<Position> given_line)
            {
                m_line = given_line;
            }
            public void Execute()
            {
                draw_plate_state.AddLine(m_line);
            }

            public int GetPriority()
            {
               return 1;
            }

            public bool IsPermanent()
            {
                return false;
            }

            public void Undo()
            {
                draw_plate_state.RemoveLine(m_line);
            }
        }

        
        List<Position> draw_tile;
        List<Position> draw_hover;
        Tile hover;
        Mode.PathScheduler<PlateDrawController> m_controller;
        bool path_completed;
        protected static DrawPlateState draw_plate_state;
        public DrawPlateState(Tile given_tile)
        {
            draw_tile = new List<Position>() { given_tile.position };
            draw_hover = new List<Position>();
        }
        public override void OnActionEnter()
        {
            Debug.Log("Create state entered");
            for(int i = 0; i < draw_tile.Count; i++)
            {
                Tile current_tile = Map.map[draw_tile[i]];
                if (current_tile.GetProperty<PlateProperty>().plate_id != Guid.Empty)
                {
                    PlateMode.AddToQueue(new StateAction(new DefaultState()));
                    return;
                }
                current_tile.UnHover();
                current_tile.OnSelect();
            }
            Map.FindPath<PlateDrawController>.FoundPath += DrawComplete;
            draw_plate_state = this;
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

        protected void AddLine(List<Position> given_line)
        {
            if(draw_tile.Count ==1)
            {
                draw_tile.Clear();
            }
            for (int i = 0; i < given_line.Count; i++)
            {
                Tile current_tile = Map.map[given_line[i]];
                current_tile.UnHover();
                current_tile.OnSelect();
                draw_tile.Add(current_tile.position);
            }
            ClearHover();
        }

        protected void RemoveLine(List<Position> given_line)
        {
            if(!(given_line[given_line.Count-1] == draw_tile[draw_tile.Count-1]
                && given_line[0] == draw_tile[draw_tile.Count-given_line.Count]))
            {
                throw new Exception("Line not deleted correctly");
            }
            for (int i = 0; i < given_line.Count; i++)
            {
                Tile current_tile = Map.map[draw_tile[draw_tile.Count-1]];
                current_tile.UnSelect();
                draw_tile.RemoveAt(draw_tile.Count - 1);
            }
            Tile redraw = hover;
            hover = null;
            if (draw_tile.Count == 0)
            {
                draw_tile.Add(given_line[0]);
            }
            OnHover(redraw);
        }

        public override void OnClick(Tile given_tile)
        {
            if(path_completed)
            {
                for (int i = 0; i < draw_hover.Count; i++)
                {
                    Map.map[draw_hover[i]].UnHover();
                    Map.map[draw_hover[i]].OnSelect();
                }
                if (draw_tile.Contains(given_tile.position))
                {
                    draw_tile.AddRange(draw_hover);
                    Mode.AddToQueue(new CreatePlateAction(new List<Position>(draw_tile) ));
                }
                else
                {
                    Mode.AddToQueue(new DrawAction(new List<Position>(draw_hover)));
                }
            }
        }

        public override void OnHover(Tile given_tile)
        {
            if (hover != given_tile)
            {
                Position start = draw_tile[draw_tile.Count - 1];
                path_completed = false;
                if(m_controller != null)
                {
                    m_controller.EndJob();
                    ClearHover();
                }
                m_controller = new Mode.PathScheduler<PlateDrawController>();
                m_controller.ScheduleFindPath(Map.map[start], given_tile);
                hover = given_tile;
            }
        }

        public override void UnSelect()
        {
            if (draw_tile.Count == 1)
            {
               Mode.AddToQueue(new StateAction(new DefaultState()));
            }
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
            for (int i = 0; i < draw_tile.Count; i++)
            {
                Map.map[draw_tile[i]].UnSelect();
            }
            draw_tile.Clear();
            ClearHover();
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

        public EditPlateState(Plate given_plate)
        {
            m_plate = given_plate;
            selected_tile = new List<Tile>();
            draw_tile = new List<Position>();
            draw_hover = new List<Position>();
        }

        public override void OnActionEnter()
        {
            base.OnActionEnter();
            m_menu = Menu.CreateMenu<EditPlateMenu>();
            m_menu.SetPlate(m_plate);
            EditPlateMenu.Clicked += OnClick;

        }
        public override void OnActionLeave()
        {
            base.OnActionLeave();
            EditPlateMenu.Clicked -= OnClick;
            Clear();
            UnityEngine.Object.Destroy(m_menu.gameObject);

        }

        public override void OnClick(Tile given_tile)
        {

            if(given_tile.GetProperty<PlateProperty>().plate_id != m_plate.id)
            {
                Mode.AddToQueue(new StateAction(new DefaultState()));
            }
        }

        public void OnClick(EditPlateMenu given_menu)
        {
            InputFieldLayer selected = given_menu.GetSelectedTextField();
            if (selected != null)
            {
                Mode.AddToQueue(new TextFieldAction(selected));
                //Change textfields to InputFieldLayer to take the action against the layer
            }

        }

        public override void OnHover(Tile given_tile)
        {
        }

        public override void UnSelect()
        {
            if(!Mode.IsFocused())
            {
                Mode.AddToQueue(new StateAction(new DefaultState()));
            }
            else
            {
                m_menu.GetSelectedTextField().Unselect();
            }
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

