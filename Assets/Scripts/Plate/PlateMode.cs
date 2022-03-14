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
        Dictionary<string,Menu> m_menu;
        protected static PlateState current_state;
        //public List<UserAction> temp_history;

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
            Tile.draw_property = typeof(PlateProperty).Name;
            SaveMenu save = Menu.CreateMenu<SaveMenu>();
            current_state = new DefaultState();
            current_state.OnActionEnter();
        }
        protected T GetMenu<T>() where T : Menu
        {
            return (T)m_menu[typeof(T).Name];
        }

        public static void ChangeState(PlateState next_state)
        {
            current_state.OnActionLeave();
            next_state.OnActionEnter();
            PlateMode.current_state = next_state;
            FlushHistory();
        }

        void Update()
        {
            if (!focused)
            {
                MoveCamera();
                if (current_actions.Count > 0)
                {
                    current_actions.Sort((UserAction item1, UserAction item2) => item1.GetPriority().CompareTo(item2.GetPriority()));
                    UserAction current_action = current_actions.First();
                    current_action.Execute();
                    try
                    {
                        if(reverted_history.Count> 0)
                        {
                            reverted_history.Clear();
                        }
                        history.Add(current_action);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    current_actions.Clear();
                }
                else if(Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if(reverted_history.Count > 0)
                    {
                        UserAction redo_action = reverted_history[reverted_history.Count-1];
                        reverted_history.RemoveAt(reverted_history.Count - 1);
                        history.Add(redo_action);
                        redo_action.Execute();
                    }
                }
                else if(Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    try
                    {
                        current_actions.Clear();
                        if (history.Count > 0)
                        {
                            UserAction reverted_action = history[history.Count - 1];
                            history.RemoveAt(history.Count - 1);
                            reverted_history.Add(reverted_action);
                            reverted_action.Undo();
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    current_actions.Clear();
                }
                else if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    try
                    {
                        current_actions.Clear();
                        if (history.Count > 0)
                        {
                            UserAction reverted_action = history[history.Count - 1];
                            if (!reverted_action.IsPermanent())
                            {
                                history.RemoveAt(history.Count - 1);
                                reverted_history.Add(reverted_action);
                                reverted_action.Undo();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    current_actions.Clear();
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

        public override void OnModeEnter()
        {

        }

        public override void OnModeExit()
        {

        }
    }

}
