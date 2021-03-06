﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Configuration;
using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
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
            var editor = new LevelEditor(5, 5, 12, 6, ConfigurationManager.AppSettings["errorTile"], ConfigurationManager.AppSettings["cautionTile"]);
            editor.LoadTileset(ConfigurationManager.AppSettings["TilesetInput"]);
            editor.SetupBorderTiles(ConfigurationManager.AppSettings["borderInput"]);
            editor.SetupDoorTiles(ConfigurationManager.AppSettings["entranceInput"]);
            editor.SetupCornersAndIntersectionTiles(ConfigurationManager.AppSettings["cornerTraverseInput"]);
            editor.SetupTraversableTiles(
                ConfigurationManager.AppSettings["hTraverseInput"],
                ConfigurationManager.AppSettings["vTraverseInput"]
            );
            for (var i = 0; i < 1; i++)
            {
                editor.InitWcfGrid(0);
                editor.ManualChanges(4, 3);
                var result = editor.GetWcfResult(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp",i.ToString()+".bmp"));
                if (result == false)
                    i -= 1;
            }
        }
    }
}
