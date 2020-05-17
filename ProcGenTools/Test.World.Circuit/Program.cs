using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
using RoomEditor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleBuilder.Creators;
using PuzzleBuilder;

namespace Test.World.Circuit
{
    class Program
    {

        static void Main(string[] args)
        {
            //setup hierarchical map
            var Random = new Random(3);
            HierarchicalMap.RelativeScales = new int[] { 2,2 };
            var map = new HierarchicalMap(8, 8, Random);
            map.SpawnZoneAtClusterPosition(4, 3);
            map.SpawnZoneAtClusterPosition(4, 3, null, true);
            map.SpawnZoneAtClusterPosition(4, 3, null, true);
            map.SpawnZoneAtClusterPosition(4, 3, null, true);
            map.MarkExitOnlyPortals();
            map.CreateSubMaps();
            //map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"]);
            //for (var i = 0; i < map.flatZones.Count(); i++)
            //{
            //    var submap = map.flatZones[i].SubMap;
            //    submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            //}
            var mastermap = map.GetMasterMap();
            var masterPortals = map.GetMasterPortal();


            map.PrintMasterOrderingToBitmap(ConfigurationManager.AppSettings["Output"].Replace(".bmp", "main_map.bmp"));
            map.PrintMasterToBitmap(ConfigurationManager.AppSettings["Output"].Replace(".bmp", "main__map.bmp"));

            var tileSize = 6;

            


            //must be divisible by 3 for paths?!
            var scale = 5;

            var finalBitmap = new Bitmap(mastermap.GetLength(0) * scale * tileSize, mastermap.GetLength(1) * scale * tileSize);

            int bmpIndex = 0;
            for (var y = 0; y < mastermap.GetLength(1); y++)
            {
                for (var x = 0; x < mastermap.GetLength(0); x++)
                {

                    var roomLevelMap = mastermap[x, y].FirstOrDefault(mm => mm.flatZones.Count == 0 && mm.contents == null);
                    if (roomLevelMap != null)
                    {
                        bmpIndex += 1;
                        for (var attempt = 0; attempt < 8; attempt++)
                        {
                            var room = roomLevelMap.ToRoom(scale);
                            var grid = new IntentionGrid(room.Width, room.Height);
                            var TilesConfig = new TilesetConfiguration(
                                ConfigurationManager.AppSettings["mainTileset"],
                                ConfigurationManager.AppSettings["hTileset"],
                                ConfigurationManager.AppSettings["hPlainTileset"],
                                ConfigurationManager.AppSettings["vTileset"],
                                ConfigurationManager.AppSettings["ladderTileset"],
                                ConfigurationManager.AppSettings["verticalExitTileset"],
                                ConfigurationManager.AppSettings["doorTileset"],
                                ConfigurationManager.AppSettings["buttonTileset"],
                                ConfigurationManager.AppSettings["ropeTileset"],
                                ConfigurationManager.AppSettings["conveyorTileset"],
                                ConfigurationManager.AppSettings["elevatorTileset"],
                                ConfigurationManager.AppSettings["shooterTileset"],
                                ConfigurationManager.AppSettings["solidTileset"],
                                ConfigurationManager.AppSettings["emptyTileset"],
                                ConfigurationManager.AppSettings["nondynamic"],
                                ConfigurationManager.AppSettings["nondynamicstrict"],
                                ConfigurationManager.AppSettings["cautionTileset"],
                                ConfigurationManager.AppSettings["errorTileset"]
                            );
                            var processor = new BasicCircuitProcess.PuzzleProcess(Random, grid, TilesConfig);
                            try
                            {
                                roomLevelMap.contents = processor.CreateIt(room.portals.Select(p => p.point).ToList(), new List<Point>()).TileMap;
                            }catch(Exception ex)
                            {
                                continue;
                            }
                            break;
                        }

                        //draw to larger bitmap
                        Graphics g = Graphics.FromImage(finalBitmap);
                        g.DrawImage(roomLevelMap.contents, new Point(x * scale * tileSize, y * scale * tileSize));

                        //draw portals
                        var portals = roomLevelMap._Portals;//.Where(p=>/*p.DestinationPortal!= null &&*/ p.id.CompareTo(((HierarchicalMapPortal)p.DestinationPortal).id) == -1).ToList();
                        foreach (var portal in portals)
                        {
                            if (portal.direction == new Point())
                            {
                                var breaka = "here";
                            }
                            var startPos = new Point(
                                    (x * scale * tileSize) + (portal.portalOffsetFromRoom.X * tileSize),
                                    (y * scale * tileSize) + (portal.portalOffsetFromRoom.Y * tileSize)
                                );
                            var endPos = new Point(
                                    startPos.X + portal.direction.X * tileSize / 2,
                                    startPos.Y + portal.direction.Y * tileSize / 2
                                );
                            var xDir = Math.Sign(endPos.X - startPos.X);
                            var yDir = Math.Sign(endPos.Y - startPos.Y);
                            var drawPos = startPos;
                            if (drawPos == endPos)
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

            BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["Output"], finalBitmap);


            //map.PrintMasterOrderingToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", "main_map.bmp"));
            //for (var i = 0; i < map.flatZones.Count(); i++)
            //{
            //    var submap = map.flatZones[i].SubMap;
            //    submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            //}
        }
        
    }
}
