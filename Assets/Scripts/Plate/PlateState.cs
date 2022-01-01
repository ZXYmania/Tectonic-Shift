using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateSpace
{
    public abstract class PlateState
    {
        public virtual void OnActionEnter()
        {
            Tile.Clicked += OnClick;
            Tile.Hovered += OnHover;
            //EditPlateMenu.Clicked += OnClick;
            SaveMenu.Save += EditFile;
        }
        public virtual void OnActionLeave()
        {
            Tile.Clicked -= OnClick;
            Tile.Hovered -= OnHover;
            //EditPlateMenu.Clicked += OnClick;
            SaveMenu.Save -= EditFile;
        }

        public abstract void Unselect();
        public abstract void Clear();
        public abstract void CommitAction();
        public abstract void OnHover(Tile given_tile);
        public abstract void OnClick(Tile given_tile);

        protected virtual void EditFile(bool edit_type)
        {
            Clear();
            if (edit_type)
            {
                PlateMode.SaveFile<Plate>(Plate.plate.Values.ToList());
            }
            else
            {
                List<Plate> load = PlateMode.LoadFile<Plate>();
                Plate.plate.Clear();
                for (int i = 0; i < load.Count; i++)
                {
                    Plate.CreatePlate((load[i]));
                }
                List<CreatePlateErrorData> error = PlateMode.LoadFile<CreatePlateErrorData>("error.dat");
            }
        }

    }

    public class DefaultState : PlateState
    {
        Tile hover;

        public override void Clear()
        {
        }

        public override void CommitAction()
        {

        }

        public override void OnClick(Tile given_tile)
        {
            Guid plate_id = given_tile.GetProperty<PlateProperty>().plate_id;
            if ( plate_id != new Guid())
            {
                PlateMode.ChangeState(new EditPlateState(Plate.plate[plate_id]));
            }
            else
            {
                PlateMode.ChangeState(new DrawPlateState(given_tile));
            }
        }

        public override void OnHover(Tile given_tile)
        {
            if (hover != null)
            {
                hover.UnHover();
            }
            hover = given_tile;
            hover.OnHover();
        }

        public override void Unselect()
        {
        }
    }
}
