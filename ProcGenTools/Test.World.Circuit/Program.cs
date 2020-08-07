using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
using RoomEditor;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuzzleBuilder.Creators;
using PuzzleBuilder;
using PuzzleBuilder.Process;

namespace Test.World.Circuit
{
    class Program
    {

        static void Main(string[] args)
        {
            //setup hierarchical map
            var seed = 1;

            var Random = new Random(seed);
            HierarchicalMap.RelativeScales = new double[] {4};
            var map = new HierarchicalMap(16,16, Random);
            map.SpawnZoneAtClusterPosition(4, 3, null, false);
            map.SpawnZoneAtClusterPosition(4, 3, null, true);
            map.SpawnZoneAtClusterPosition(4, 3, null, true);
            map.SpawnZoneAtClusterPosition(4, 3, null, true);
            map.MarkExitOnlyPortals();
            map.CreateSubMaps();

            ZoneSequentialId.Reset();
            map.ReassignSequentialIds();
            map.AssignAbsPositions();
            map.SetClutchRoomRelations();

            //map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"]);
            //for (var i = 0; i < map.flatZones.Count(); i++)
            //{
            //    var submap = map.flatZones[i].SubMap;
            //    submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            //}
            var mastermap = map.GetMasterMap();
            var terminalmap = map.GetMapTerminals();
            var masterPortals = map.GetMasterPortal();
            var thevar = terminalmap.Select(tm => new { absX = tm._AbsX, absY = tm._AbsY }).ToList();

            map.PrintMasterOrderingToBitmap(ConfigurationManager.AppSettings["Output"].Replace(".bmp", "main_map.bmp"));
            map.PrintMasterToBitmap(ConfigurationManager.AppSettings["Output"].Replace(".bmp", "main__map.bmp"));

            var tileSize = 6;

            


            //must be divisible by 3 for paths?!
            var scale = 3;

            var finalBitmap = new Bitmap(mastermap.GetLength(0) * scale * tileSize, mastermap.GetLength(1) * scale * tileSize);


            ProcessFactory<AdvancedCircuitProcess.PuzzleProcess> factory = null;
            int roomSeed = seed;
            PuzzleInfo roomResult;
            for (var y = 0; y < mastermap.GetLength(1); y++)
            {
                for (var x = 0; x < mastermap.GetLength(0); x++)
                {
                    roomSeed = roomSeed + 10;
                    var roomLevelMap = mastermap[x, y].FirstOrDefault(mm => mm.flatZones.Count == 0 && mm.contents == null);
                    if (roomLevelMap != null)
                    {

                        var room = roomLevelMap.ToRoom(scale);

                        if (factory == null)
                            factory = new ProcessFactory<AdvancedCircuitProcess.PuzzleProcess>(room.Width, room.Height, "..//..//WfcDebug//Current//", "..//..//TilesetsDebug//Current//");
                        else
                            factory.Reset(room.Width, room.Height);

                        try
                        {
                            roomResult = factory.GetPuzzle(
                                seed,
                                room.portals.Where(p => p.IsEntrance()).Select(p => p.point).ToList(),
                                room.portals.Where(p => !p.IsEntrance()).Select(p => p.point).ToList()
                            );
                        }
                        catch(Exception ex)
                        {
                            //do this so we don't end up trying again....
                            roomLevelMap.contents = new Bitmap(roomLevelMap._MapWidth * scale * tileSize, roomLevelMap._MapHeight * scale * tileSize);
                            continue;
                        }
                        if (roomResult.Success)
                        {
                            roomLevelMap.contents = roomResult.TileMap;
                        }
                            //factory.SaveResult(
                            //    ConfigurationManager.AppSettings["Output"].ToString()
                            //            .Replace("output.bmp", "")
                            //);
                        

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

            //draw line from clutch rooms
            foreach(var relation in map.ClutchRelations.Where(x=>x.ItemRoom._AllSubChildren.Count == 0))
            {
                using (Graphics g = Graphics.FromImage(finalBitmap))
                {
                    g.DrawLine(Pens.Red,
                        new Point((relation.ItemRoom._AbsX * scale * tileSize), relation.ItemRoom._AbsY * scale * tileSize),
                        new Point(relation.LockedRoom._AbsX * scale * tileSize, relation.LockedRoom._AbsY * scale * tileSize)
                    );
                    g.DrawString("I", SystemFonts.DefaultFont, Brushes.Red, (relation.ItemRoom._AbsX * scale * tileSize), relation.ItemRoom._AbsY * scale * tileSize);

                    g.DrawString(" L", SystemFonts.DefaultFont, Brushes.Red, (relation.LockedRoom._AbsX * scale * tileSize), relation.LockedRoom._AbsY * scale * tileSize);
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
