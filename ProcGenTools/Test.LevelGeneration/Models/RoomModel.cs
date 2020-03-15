using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// THIS HAS BEEN MIGRATED TO ROOM EDITOR PROJECT
/// </summary>
namespace Test.LevelGeneration.Models
{
    public class Room
    {
        public List<Portal> portals;
        public int Width;
        public int Height;
        public int BorderWidth;
        public static List<Room> GetTestRooms(){
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
                    }, Width = 12, Height = 12, BorderWidth = 1
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
                    }, Width = 12, Height = 12, BorderWidth = 1
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
                    }, Width = 12, Height = 12, BorderWidth = 1
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
                    }, Width = 12, Height = 12, BorderWidth = 1
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
                    }, Width = 12, Height = 12, BorderWidth = 1
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
                    }, Width = 12, Height = 12, BorderWidth = 1
                }
            };
        }
    }

    public class Portal
    {
        public Point point;
        public Point direction;

        public bool IsEntrance(Room room)
        {
            return (point.X + direction.X > 0 && point.X + direction.X < room.Width)
                &&
                (point.Y + direction.Y > 0 && point.Y + direction.Y < room.Height);
        }
    }
}
