using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcGenTools.DataStructures;
using ProcGenTools.DataProcessing;
using RoomEditor;
using System.Configuration;
using System.Drawing;

namespace Test.World
{
    class Program
    {
        static void Main(string[] args)
        {
            //setup hierarchical map
            var Random = new Random();
            HierarchicalMap.RelativeScales = new int[] { 2, 2 };
            var map = new HierarchicalMap(10, 10, Random);
            map.SpawnZoneAtSearchPosition(3, 2);
            map.SpawnZoneAtSearchPosition(3, 2, null, true);
            map.SpawnZoneAtSearchPosition(3, 2, null, true);
            map.SpawnZoneAtSearchPosition(3, 2, null, true);
            map.CreateSubMaps();
            //map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"]);
            //for (var i = 0; i < map.flatZones.Count(); i++)
            //{
            //    var submap = map.flatZones[i].SubMap;
            //    submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            //}
            var mastermap = map.GetMasterMap();

            var editor = new LevelEditor(5, 5, 12, 12, ConfigurationManager.AppSettings["errorTile"], ConfigurationManager.AppSettings["cautionTile"]);
            editor.LoadTileset(ConfigurationManager.AppSettings["TilesetInput"], true, "..//..//Output//mainTileset.bmp",true);
            editor.SetupBorderTiles(ConfigurationManager.AppSettings["borderInput"]);
            editor.SetupHEntranceTiles(ConfigurationManager.AppSettings["hEntranceInput"]);
            editor.SetupVEntranceTiles(ConfigurationManager.AppSettings["vEntranceInput"]);
            editor.SetupCornersAndIntersectionTiles(ConfigurationManager.AppSettings["cornerTraverseInput"], true);
            editor.SetupTraversableTiles(
                ConfigurationManager.AppSettings["hTraverseInput"],
                ConfigurationManager.AppSettings["vTraverseInput"],
                true
            );

            var scale = 5;
            var finalBitmap = new Bitmap(mastermap.GetLength(0) * scale * 5, mastermap.GetLength(1) * scale * 5);

            int bmpIndex = 0;
            for (var y = 0; y < mastermap.GetLength(1); y++)
            {
                for(var x = 0; x < mastermap.GetLength(0); x++)
                {
                    
                    var roomLevelMap = mastermap[x, y].FirstOrDefault(mm => mm.flatZones.Count == 0 && mm.contents == null);
                    if(roomLevelMap != null)
                    {
                        bmpIndex += 1;
                        for (var attempt = 0; attempt < 8; attempt++)
                        {
                            var room = roomLevelMap.ToRoom(scale);

                            editor.SetDimensions(room.Width, room.Height);
                            editor.InitWcfGrid(Random.Next());
                            
                            var result = true;
                            result = result && editor.AddBorder(room);
                            if (!result)
                            {
                                Console.WriteLine("Failed Add Border");
                            }
                            else
                            {
                                result = result && editor.AddPaths(room);
                            }
                            if (!result)
                            {
                                Console.WriteLine("Failed Add Paths");
                            }
                            else
                            {
                                result = result && editor.CollapseWcf();
                            }
                            if (!result)
                            {
                                Console.WriteLine("Failed Collapse");
                            }
                            if(!result)
                            {
                                BitmapOperations.SaveBitmapToFile("../../Output/MostRecentError.bmp", editor.GetBitmap());
                                Console.WriteLine("Failed Manual Changes: " + attempt.ToString());
                            }
                            else
                            {
                                break;
                            }
                        }

                        roomLevelMap.contents = editor.GetBitmap();

                        //draw to larger bitmap
                        Graphics g = Graphics.FromImage(finalBitmap);
                        g.DrawImage(roomLevelMap.contents, new Point(x * scale * 5, y * scale * 5));

                        BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", "_wcf_" + bmpIndex.ToString() + ".bmp"), roomLevelMap.contents);
                        
                    }
                }
            }

            BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["BitmapOutput"], finalBitmap);


            map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", "main_map.bmp"));
            for (var i = 0; i < map.flatZones.Count(); i++)
            {
                var submap = map.flatZones[i].SubMap;
                submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            }
        }
    }
}
