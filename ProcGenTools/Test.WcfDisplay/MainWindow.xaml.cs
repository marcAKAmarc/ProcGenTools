using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
using PuzzleBuilder;
using PuzzleBuilder.Core;
using PuzzleBuilder.Process;
using RoomEditor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test.WcfDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    
    public partial class MainWindow : Window, iDisplayer
    {
        public MainWindow()
        {
            InitializeComponent();
            Console.SetOut(new ControlWriter(FakeConsole));
            var back = new BackgroundWorker();
            back.DoWork += new DoWorkEventHandler(back_DoWork);
            back.RunWorkerAsync();
            mainWindow.UseLayoutRounding = true;
            //CreateItAll();

        }
        void back_DoWork(object sender, DoWorkEventArgs e)
        {
            CreateItAll();
        }

        void iDisplayer.Display(Bitmap bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                Action action = delegate { wcfImageControl.Source = bitmapImage; };
                wcfImageControl.Dispatcher.Invoke(action);
            }
        }

        private void CreateItAll()
        {
            //setup hierarchical map
            var seed = 3;

            var Random = new marcRandom(seed);
            HierarchicalMap.RelativeScales = new double[] { 4 };
            var map = new HierarchicalMap(16, 16, Random, 4, 3);
            map.DefaultSetup();

            //map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"]);
            //for (var i = 0; i < map.flatZones.Count(); i++)
            //{
            //    var submap = map.flatZones[i].SubMap;
            //    submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            //}
            var mastermap = map.GetMasterMap();
            var masterPortals = map.GetMasterPortal();
            var terminalMap = map.GetMapTerminals();

            //map.PrintMasterOrderingToBitmap(ConfigurationManager.AppSettings["Output"].Replace(".bmp", "main_map.bmp"));
            //map.PrintMasterToBitmap(ConfigurationManager.AppSettings["Output"].Replace(".bmp", "main__map.bmp"));

            var tileSize = 6;




            //must be divisible by 3 for paths?!
            var scale = 3;

            var finalBitmap = new Bitmap(mastermap.GetLength(0) * scale * tileSize, mastermap.GetLength(1) * scale * tileSize);


            ProcessFactory<AdvancedCircuitProcess.PuzzleProcess> factory = null;
            int roomSeed = seed;
            PuzzleInfo roomResult;
            foreach (var roomLevelMap in terminalMap.OrderBy(x => x.SequentialId))
            {
                /*for (var y = 0; y < mastermap.GetLength(1); y++)
                {
                    for (var x = 0; x < mastermap.GetLength(0); x++)
                    {*/
                roomSeed = Random.Next();
                //var roomLevelMap = mastermap[x, y].FirstOrDefault(mm => mm.flatZones.Count == 0 && mm.contents == null);
                if (roomLevelMap != null)
                {

                    var room = roomLevelMap.ToRoom(scale);

                    if (factory == null)
                        factory = new ProcessFactory<AdvancedCircuitProcess.PuzzleProcess>(room.Width, room.Height, "..//..//WfcDebug//Current//", "..//..//TilesetsDebug//Current//");
                    else
                        factory.Reset(room.Width, room.Height);
                    factory.SetProcessDisplayer(this);

                    int? KeyId = null;
                    if (roomLevelMap.IsItemRoom)
                    {
                        KeyId = roomLevelMap.RelationsAsItemRoom.First().LockInfo.LockId;
                    }
                    try
                    {
                        roomResult = factory.GetPuzzle(
                            roomSeed,
                            room.portals,
                            KeyId,
                            roomLevelMap.IsSkippable
                        );
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    if (roomResult.Success)
                    {
                        roomLevelMap.contents = roomResult.TileMap;
                    }
                    else
                    {
                        var breaka = "here";
                    }
                    //factory.SaveResult(
                    //    ConfigurationManager.AppSettings["Output"].ToString()
                    //            .Replace("output.bmp", "")
                    //);


                    //draw to larger bitmap
                    Graphics g = Graphics.FromImage(finalBitmap);
                    g.DrawImage(roomLevelMap.contents, new System.Drawing.Point(roomLevelMap._AbsX * scale * tileSize, roomLevelMap._AbsY * scale * tileSize));

                    //draw portals
                    var portals = roomLevelMap._Portals;//.Where(p=>/*p.DestinationPortal!= null &&*/ p.id.CompareTo(((HierarchicalMapPortal)p.DestinationPortal).id) == -1).ToList();
                    foreach (var portal in portals)
                    {
                        if (portal.direction == new System.Drawing.Point())
                        {
                            var breaka = "here";
                        }
                        var startPos = new System.Drawing.Point(
                                (roomLevelMap._AbsX * scale * tileSize) + (portal.portalOffsetFromRoom.X * tileSize),
                                (roomLevelMap._AbsY * scale * tileSize) + (portal.portalOffsetFromRoom.Y * tileSize)
                            );
                        var endPos = new System.Drawing.Point(
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
                            color = System.Drawing.Color.FromArgb(255 - (int)color.R, 255 - (int)color.G, 255 - (int)color.B);

                            if (portal.directionOfPassage == ZonePortalDirection.In)
                                color = System.Drawing.Color.Red;
                            else if (portal.directionOfPassage == ZonePortalDirection.Out)
                                color = System.Drawing.Color.Green;

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

            //BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["Output"], finalBitmap);


            //map.PrintMasterOrderingToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", "main_map.bmp"));
            //for (var i = 0; i < map.flatZones.Count(); i++)
            //{
            //    var submap = map.flatZones[i].SubMap;
            //    submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            //}
        }
    }


    public static class Helpers
    {

        public static Room ToRoom(this HierarchicalMap hMap, int scale = 1)
        {
            ///MAYBE THIS IS MESSED UP //confirming other stuff first.
            var room = new Room();
            room.Height = hMap._MapHeight * scale;
            room.Width = hMap._MapWidth * scale;
            if (room.Height < room.Width && hMap._Portals.Any(x => x.direction.Y == 1))
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
                    point = new System.Drawing.Point(x, y),
                    direction = portal.direction
                };
                p.fromPortal = portal;
                portal.portalOffsetFromRoom = new System.Drawing.Point(x, y);
                room.portals.Add(p);
            }

            return room;
        }
    }


    public class ControlWriter : TextWriter
    {
        private TextBox textbox;
        public ControlWriter(TextBox textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            Action action = delegate { textbox.Text += value; };
            textbox.Dispatcher.Invoke(action);
        }

        public override void Write(string value)
        {
            Action action = delegate { textbox.Text += value; };
            textbox.Dispatcher.Invoke(action);
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
}
