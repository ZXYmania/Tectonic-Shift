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
        public static event Action<Guid> EndFindPath = delegate { };
        protected static bool focused;
        Dictionary<string,Menu> m_menu;
        protected static PlateState current_state;

        protected T GetMenu<T>() where T : Menu
        {
            return (T)m_menu[typeof(T).Name];
        }

        public static void ChangeState(PlateState next_state)
        {
            current_state.OnActionLeave();
            current_state.Clear();
            next_state.OnActionEnter();
            PlateMode.current_state = next_state;
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
            if (current_mode == null)
            {
                SetUp();
            }
            Initialise();
        }

        public void Initialise()
        {
            Mode.current_mode = this;
            selected = new List<Clickable>();
            m_menu = new Dictionary<string, Menu>();
            TextField.Selected += Focus;
            TextField.UnSelected += Unfocus;
            Tile.draw_property = typeof(PlateProperty).Name;
            SaveMenu save = Menu.CreateMenu<SaveMenu>();
            current_state = new DefaultState();
            current_state.OnActionEnter();
        }

        void Update()
        {
            if (!focused)
            {
                MoveCamera();
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    current_state.Unselect();
                }
                else
                {
                    current_state.CommitAction();
                }
            }
        }
        public struct PlateDrawController : DrawController
        {
            public void Initialise()
            {

            }
            public int GetDistance(Position from, Position to)
            {
                if (Map.map[to].GetProperty<PlateProperty>().plate_id != Guid.Empty)
                {
                    return -1;
                }
                Position result = new Position(to.x - from.x, to.y - from.y);
                return result.x * result.x + result.y * result.y;
            }
        }

        public static void KillPath(Guid given_id)
        {
            EndFindPath(given_id);
        }

        /*public void OnClick(Tile given_tile)
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



        public void UnHover(Tile given_tile)
        {
            given_tile.UnHover();
        }

        /*public void UnSelect(Clickable unselected)
        {
             if (selected[0] is Plate)
            {
                GetMenu<EditPlateMenu>().UnSelect();
                Clear();
            }
            
        }*/

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
