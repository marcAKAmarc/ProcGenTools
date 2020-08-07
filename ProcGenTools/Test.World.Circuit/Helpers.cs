using ProcGenTools.DataStructures;
using RoomEditor.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.World.Circuit
{
    public static class Helpers
    {

        public static Room ToRoom(this HierarchicalMap hMap, int scale = 1)
        {

            var room = new Room();
            room.Height = hMap._MapHeight * scale;
            room.Width = hMap._MapWidth * scale;
            if(room.Height < room.Width && hMap._Portals.Any(x=>x.direction.Y == 1))
            {
                var breaka = "here";
            }
            room.DistanceBetweenPathAndEdge = 2;//scale - 1;
            room.portals = new List<Portal>();
            room.FromMap = hMap;
            foreach (var portal in hMap._Portals)
            {
                if (portal.destination == null)
                    continue;
                if (portal.point.X != 0 && portal.point.X != hMap._MapWidth - 1 && portal.point.Y != 0 && portal.point.Y != hMap._MapHeight - 1)
                    continue;
                var x = portal.point.X * scale;
                var y = portal.point.Y * scale;
                if (portal.direction.X == 0)
                    x += (int)Math.Floor((double)scale / 2);
                else
                    y += (int)Math.Floor((double)scale / 2);
                //if (portal.direction.X == 0 && x == 0)
                //    x = room.DistanceBetweenPathAndEdge;
                //if (portal.direction.Y == 0 && y == 0)
                //    y = room.DistanceBetweenPathAndEdge;

                if (portal.direction.X == 0 && y != 0)
                    y = (hMap._MapHeight * scale) - 1;
                else if (portal.direction.Y == 0 && x != 0)
                    x = (hMap._MapWidth * scale) - 1;
                var p = new Portal()
                {
                    point = new Point(x, y),
                    direction = portal.direction,
                    fromPortal = portal
                };
                portal.portalOffsetFromRoom = new Point(x, y);
                room.portals.Add(p);
            }

            return room;
        }
    }
}
