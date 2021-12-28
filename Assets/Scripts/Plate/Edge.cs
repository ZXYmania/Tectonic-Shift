using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlateSpace
{
    public class Edge
    {
        public List<Position> tile;
        public Position facing;
        public Guid plate_id;
        public Edge(List<Position> tile, Guid plate_id)
        {
            this.plate_id = plate_id;
            this.tile = new List<Position>(tile);
        }

        public Position GetDirection()
        {
            return Map.GetDirection(tile[1], tile[0]);
        }
        public void SetOutside()
        {
            int halfway = Mathf.FloorToInt(tile.Count / 2);
            Position outside = GetDirection();
            outside = new Position(outside.y, outside.x * -1);
            if (Plate.plate[plate_id].IsInside(tile[halfway] + outside))
            {
                outside = new Position(outside.x * -1, outside.y * -1);
                if (Plate.plate[plate_id].IsInside(tile[halfway] + outside))
                {
                    facing = new Position(0, 0);
                }
            }
            facing = outside;
        }

        public bool Validate()
        {
            Plate parent = Plate.plate[plate_id];
            int i = 1;
            for (; i < tile.Count - 1; i++)
            {
                if (parent.IsInside(tile[i] + facing))
                {
                    Debug.Log(tile[i] + " " + facing + " edge is not valid");
                    Map.map[tile[i] + facing].SetProperty<PlateProperty>(new PlateProperty(2, plate_id));
                    return false;
                }
            }
            return i > 1 || !parent.IsInside(tile[0] + facing);
        }
    }
}
