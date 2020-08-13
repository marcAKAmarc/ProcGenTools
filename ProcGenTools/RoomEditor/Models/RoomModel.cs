﻿using ProcGenTools.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RoomEditor.Models
{
    public class Room
    {
        public List<Portal> portals;
        public int Width;
        public int Height;
        public int DistanceBetweenPathAndEdge;
        public HierarchicalMap FromMap;
        public static List<Room> GetTestRooms()
        {
            return new List<Room>
            {
                new Room{
                    portals = new List<Portal>
                    {
                       new Portal
                       {
                           point = new Point(0, 4),
                           direction = new Point(1, 0)
                       },
                       new Portal
                       {
                           point = new Point(11, 4),
                           direction = new Point(1, 0)
                       },
                    }, Width = 12, Height = 12, DistanceBetweenPathAndEdge = 1
                },
                new Room{
                    portals = new List<Portal>
                    {
                       new Portal
                       {
                           point = new Point(4, 0),
                           direction = new Point(0, 1)
                       },
                       new Portal
                       {
                           point = new Point(4, 11),
                           direction = new Point(0, 1)
                       },
                    }, Width = 12, Height = 12, DistanceBetweenPathAndEdge = 1
                },
                new Room{
                    portals = new List<Portal>
                    {
                       new Portal
                       {
                           point = new Point(4, 0),
                           direction = new Point(0, 1)
                       },
                       new Portal
                       {
                           point = new Point(11, 4),
                           direction = new Point(1, 0)
                       },
                    }, Width = 12, Height = 12, DistanceBetweenPathAndEdge = 1
                },
                new Room{
                    portals = new List<Portal>
                    {
                       new Portal
                       {
                           point = new Point(4, 0),
                           direction = new Point(0, 1)
                       },
                       new Portal
                       {
                           point = new Point(0, 5),
                           direction = new Point(-1, 0)
                       },
                    }, Width = 12, Height = 12, DistanceBetweenPathAndEdge = 1
                },
                new Room{
                    portals = new List<Portal>
                    {
                       new Portal
                       {
                           point = new Point(4, 11),
                           direction = new Point(0, -1)
                       },
                       new Portal
                       {
                           point = new Point(11, 5),
                           direction = new Point(1, 0)
                       },
                    }, Width = 12, Height = 12, DistanceBetweenPathAndEdge = 1
                },
                new Room{
                    portals = new List<Portal>
                    {
                       new Portal
                       {
                           point = new Point(4, 11),
                           direction = new Point(0, -1)
                       },
                       new Portal
                       {
                           point = new Point(0, 5),
                           direction = new Point(-1, 0)
                       },
                    }, Width = 12, Height = 12, DistanceBetweenPathAndEdge = 1
                }
            };
        }
    }

    public class Portal
    {
        public Point point;
        public Point direction;  //this might be useless
        public HierarchicalMapPortal fromPortal;
        public bool IsEntrance()
        {
            return fromPortal.Map.SequentialId > fromPortal.destination.SequentialId;
        }
    }
}
