using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomEditor.Models;
using ProcGenTools.DataStructures;
using System.Drawing;

namespace Test.World
{
    public static class Helpers
    {
        //public static Room ToRoom(this Zone zone, int scale = 1)
        //{
        //    var room = new Room();
        //    room.Height = zone.Height * scale;
        //    room.Width = zone.Width * scale;
        //    room.BorderWidth = scale - 1;
        //    room.portals = new List<Portal>();

        //    foreach(var portal in zone.ZonePortals)
        //    {
        //        if (portal.ZoneRelativePosition.X != 0 && portal.ZoneRelativePosition.X != zone.Width - 1 && portal.ZoneRelativePosition.Y != 0 && portal.ZoneRelativePosition.Y != zone.Height - 1)
        //            continue;
        //        var x = portal.ZoneRelativePosition.X;
        //        var y = portal.ZoneRelativePosition.Y;
        //        if (y == zone.Height - 1)
        //            y += scale - 1;
        //        if (x == zone.Width - 1)
        //            x += scale - 1;
        //        var p = new Portal()
        //        {
        //            point = new Point(x, y),
        //            direction = new Point(1, 0)
        //        };
        //        room.portals.Add(p);
        //    }

        //    return room;
        //}

        public static Room ToRoom(this HierarchicalMap hMap, int scale = 1)
        {
            ///MAYBE THIS IS MESSED UP //confirming other stuff first.
            var room = new Room();
            room.Height = hMap._MapHeight * scale;
            room.Width = hMap._MapWidth * scale;
            room.DistanceBetweenPathAndEdge = 2;//scale - 1;
            room.portals = new List<Portal>();

            foreach (var portal in hMap._Portals)
            {
                if (portal.point.X != 0 && portal.point.X != hMap._MapWidth - 1 && portal.point.Y != 0 && portal.point.Y != hMap._MapHeight - 1)
                    continue;
                var x = portal.point.X * scale;
                var y = portal.point.Y * scale;
                if(portal.direction.X == 0)
                    x += (int)Math.Floor((double)scale / 2);
                else
                    y += (int)Math.Floor((double)scale / 2);

                if (portal.direction.X == 0 && y != 0)
                    y = (hMap._MapHeight * scale) - 1;
                else if (portal.direction.Y == 0 && x != 0)
                    x = (hMap._MapWidth * scale) - 1;
                var p = new Portal()
                {
                    point = new Point(x, y),
                    direction = portal.direction
                };
                room.portals.Add(p);
            }

            return room;
        }
    }
}
