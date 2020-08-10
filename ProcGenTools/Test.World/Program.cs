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
            var Random = new Random(0);
            HierarchicalMap.RelativeScales = new double[] { 4 };
            var map = new HierarchicalMap(12, 12, Random, 4, 3);
            map.DefaultSetup();
            //map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"]);
            //for (var i = 0; i < map.flatZones.Count(); i++)
            //{
            //    var submap = map.flatZones[i].SubMap;
            //    submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            //}
            var mastermap = map.GetMasterMap();
            var masterPortals = map.GetMasterPortal();


            map.PrintMasterOrderingToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", "main_map.bmp"));
            map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", "main__map.bmp"));

            var tileSize = 5;

            var editor = new LevelEditor(tileSize, tileSize, 12, 12, ConfigurationManager.AppSettings["errorTile"], ConfigurationManager.AppSettings["cautionTile"]);
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


            //must be divisible by 3 for paths?!
            var scale = 12;
            
            var finalBitmap = new Bitmap(mastermap.GetLength(0) * scale * tileSize, mastermap.GetLength(1) * scale * tileSize);

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
                                //BitmapOperations.SaveBitmapToFile("../../Output/MostRecentError.bmp", editor.GetBitmap());
                                Console.WriteLine("Failed Manual Changes: " + attempt.ToString());
                                continue;
                            }
                            
                            result = result && editor.AddPaths(room);
                            if (!result)
                            {
                                Console.WriteLine("Failed Add Paths");
                                //BitmapOperations.SaveBitmapToFile("../../Output/MostRecentError.bmp", editor.GetBitmap());
                                Console.WriteLine("Failed Manual Changes: " + attempt.ToString());
                                continue;
                            }

                            if (Random.NextDouble() < .25)
                            {
                                result = result && editor.MakeSpacesIntraversable();
                                if (!result)
                                {
                                    Console.WriteLine("Failed Intraversable");
                                    //BitmapOperations.SaveBitmapToFile("../../Output/MostRecentError.bmp", editor.GetBitmap());
                                    Console.WriteLine("Failed Manual Changes: " + attempt.ToString());
                                    continue;
                                }
                            }

                            result = result && editor.CollapseWcf();
                            if (!result)
                            {
                                Console.WriteLine("Failed Collapse");
                                //BitmapOperations.SaveBitmapToFile("../../Output/MostRecentError.bmp", editor.GetBitmap());
                                Console.WriteLine("Failed Manual Changes: " + attempt.ToString());
                                continue;
                            }
                            
                            //if we made it here, we have succeeded
                            break;
                            
                        }

                        roomLevelMap.contents = editor.GetBitmap();

                        //draw to larger bitmap
                        Graphics g = Graphics.FromImage(finalBitmap);
                        g.DrawImage(roomLevelMap.contents, new Point(x * scale * tileSize, y * scale * tileSize));

                        //draw portals
                        var portals = roomLevelMap._Portals;//.Where(p=>/*p.DestinationPortal!= null &&*/ p.id.CompareTo(((HierarchicalMapPortal)p.DestinationPortal).id) == -1).ToList();
                        foreach (var portal in portals)
                        {
                            if(portal.direction == new Point())
                            {
                                var breaka = "here";
                            }
                            var startPos = new Point(
                                    (x * scale * tileSize) + (portal.portalOffsetFromRoom.X * tileSize),
                                    (y * scale * tileSize) + (portal.portalOffsetFromRoom.Y * tileSize )
                                );
                            var endPos = new Point(
                                    startPos.X + portal.direction.X * tileSize/2,
                                    startPos.Y + portal.direction.Y * tileSize/2
                                );
                            var xDir = Math.Sign(endPos.X - startPos.X);
                            var yDir = Math.Sign(endPos.Y - startPos.Y);
                            var drawPos = startPos;
                            if(drawPos == endPos)
                            {
                                var breka = "here";
                            }
                            while (true)
                            {
                                var color = finalBitmap.GetPixel(drawPos.X, drawPos.Y);
                                color = Color.FromArgb(255 - (int)color.R, 255 - (int)color.G, 255 - (int)color.B);

                                if (portal.directionOfPassage == ZonePortalDirection.In)
                                    color = Color.Red;
                                else if (portal.directionOfPassage == ZonePortalDirection.Out)
                                    color = Color.Green;

                                finalBitmap.SetPixel(drawPos.X, drawPos.Y, color);

                                if (drawPos == endPos)
                                    break;

                               

                                //getNewPos
                                drawPos.X += xDir;
                                drawPos.Y += yDir;
                            }
                        }

                        //BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", "_wcf_" + bmpIndex.ToString() + ".bmp"), roomLevelMap.contents);
                        
                    }
                }
            }

            //draw portals
            //for (var y = 0; y < masterPortals.GetLength(1); y++)
            //{
            //    for (var x = 0; x < masterPortals.GetLength(0); x++)
            //    {
            //        var portals = masterPortals[x, y].Where(p => p.SubPortal == null && p.DestinationPortal != null && ((HierarchicalMapPortal)p.DestinationPortal).id.CompareTo(p.id) == -1);
            //        foreach(var portal in portals)
            //        {
            //            var startPos = new Point(
            //                    (x * scale * tileSize) + (tileSize*2),
            //                    (y * scale * tileSize) + (tileSize*2)
            //                );
            //            var endPos = new Point(
            //                    startPos.X + portal.direction.X * tileSize,
            //                    startPos.Y + portal.direction.Y * tileSize
            //                );
            //            var xDir = Math.Sign(endPos.X - startPos.X);
            //            var yDir = Math.Sign(endPos.Y - startPos.Y);
            //            var drawPos = startPos;
            //            while (true)
            //            {
            //                var color = finalBitmap.GetPixel(drawPos.X, drawPos.Y);
            //                color = Color.FromArgb(255 - (int)color.R, 255 - (int)color.G, 255 - (int)color.B);
            //                finalBitmap.SetPixel(drawPos.X, drawPos.Y, color);

            //                if (drawPos == endPos)
            //                    break;

            //                //getNewPos
            //                drawPos.X += xDir;
            //                drawPos.Y += yDir;
            //            }
            //        }
            //    }
            //}

            BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["BitmapOutput"], finalBitmap);


            //map.PrintMasterOrderingToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", "main_map.bmp"));
            //for (var i = 0; i < map.flatZones.Count(); i++)
            //{
            //    var submap = map.flatZones[i].SubMap;
            //    submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            //}
        }
    }
}
