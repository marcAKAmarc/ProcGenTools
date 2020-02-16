using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Configuration;
using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
using Test.LevelGeneration.Models;
namespace Test.LevelGeneration
{
    class Program
    {
        public delegate void UpdateFrontEnd(WcfGrid grid);

        static void Main(string[] args)
        {
            Execute();
        }

        private static void Execute()
        {
            var editor = new LevelEditor(5, 5, 12, 12, ConfigurationManager.AppSettings["errorTile"], ConfigurationManager.AppSettings["cautionTile"]);
            editor.LoadTileset(ConfigurationManager.AppSettings["TilesetInput"]);
            editor.SetupBorderTiles(ConfigurationManager.AppSettings["borderInput"]);
            editor.SetupHEntranceTiles(ConfigurationManager.AppSettings["hEntranceInput"]);
            editor.SetupVEntranceTiles(ConfigurationManager.AppSettings["vEntranceInput"]);
            editor.SetupCornersAndIntersectionTiles(ConfigurationManager.AppSettings["cornerTraverseInput"]);
            editor.SetupTraversableTiles(
                ConfigurationManager.AppSettings["hTraverseInput"],
                ConfigurationManager.AppSettings["vTraverseInput"]
            );

            var Rooms = Room.GetTestRooms();

            var seed = 20;
            for (var i = 0; i < Rooms.Count; i++)
            {
                var room = Rooms[i];
                var result = true;
                seed += 1;
                editor.InitWcfGrid(seed);
                foreach(var portal in room.portals)
                {
                    result = result && editor.AddPortal(portal);
                }
                result = result && editor.AddBorder(room);
                result = result && editor.AddPaths(room);

                result = result &&  editor.CollapseWcf();
                if (!result)
                {
                    i -= 1;
                    continue;
                }
                editor.OutputWfc(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp",i.ToString()+".bmp"));   
            }
            Console.ReadKey();
        }
    }
}
