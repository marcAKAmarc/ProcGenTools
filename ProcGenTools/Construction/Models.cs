using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ProcGenTools.DataProcessing;
using Construction.Models;
using System.Configuration;
using System.IO;
using PuzzleBuilder;
namespace Construction.Models
{

    public enum TileType
    {
        Ground = 0,
        Solid = 1,
        Empty = 2,
        LedgeRight = 3,
        LedgeLeft = 4,
        ShooterRight = 5,
        ShooterLeft = 6,
        LadderTop = 7,
        LadderMid = 8,
        LadderBottom = 9,
        LadderBottomWithConveyor = 10,
        Conveyor = 11,
        Button = 12,
        Door = 13,
        Box = 14,
        HangingRope = 15,
        HangingRopeBottom = 16
    }

    public class GameElement {
        public List<Point> Points;

        public GameElement()
        {
            Points = new List<Point>();
        }
    }

    public class Shooter : GameElement { }
    public class Ladder : GameElement { }
    public class Conveyor : GameElement { }
    public class Button : GameElement {
        public bool Momentary;
        public GameElement ControlledElement;
    }
    public class Door : GameElement { }
    public class Box : GameElement { }
    public class Rope : GameElement { }
    public class Ground : GameElement {
        
    }

}

namespace ConstructionProcessing {

    public class Configuration
    {
        public string GroundTile;
        public string SolidTile;
        public string EmptyTile;
        public string LedgeRightTile;
        public string LedgeLeftTile;
        public string ShooterRightTile;
        public string ShooterLeftTile;
        public string LadderTopTile;
        public string LadderMidTile;
        public string LadderBottomTile;
        public string LadderBottomWithConveyorTile;
        public string ConveyorTile;
        public string ButtonTile;
        public string DoorTile;
        public string BoxTile;
        public string HangingRopeTile;
        public string HangingRopeBottomTile;

        public string DebugTypeGridFolder;
    }

    public class Processor
    {
        private Configuration Config;
        private Point TileDim;
        private Bitmap Input;
        private List<List<Bitmap>> TileGrid;
        private IntentionGrid IntentionGrid;
        private TileType[,] TypeGrid;
        private List<Bitmap> TileLibrary;
        public List<GameElement> GameElements;

        public Processor(int TileX, int TileY, Bitmap InputBitmap, IntentionGrid Intentions)
        {
            initializer(TileX, TileY, InputBitmap, Intentions);
        }

        public Processor(int TileX, int TileY, Bitmap InputBitmap, IntentionGrid Intentions, Configuration config)
        {
            Config = config;
            initializer(TileX, TileY, InputBitmap, Intentions);
        }

        private void initializer(int TileX, int TileY, Bitmap InputBitmap, IntentionGrid Intentions)
        {
            //file cleanup
            string[] filePaths = Directory.GetFiles(Config.DebugTypeGridFolder);
            foreach (string filePath in filePaths)
                File.Delete(filePath);

            TileDim = new Point(TileX, TileY);
            Input = InputBitmap;
            TileLibrary = new List<Bitmap>();
            IntentionGrid = Intentions;

            GetBitmapGrid();
            TypeGrid = new TileType[TileGrid.Count, TileGrid[0].Count];

            LoadTilesToRam();
            GetTypeGrid();

            GameElements = new List<GameElement>();
            GetGameElements();
        }

        private void GetGameElements()
        {
            GetGroundElement();

            GetShooterLeftElements();
            GetShooterRightElements();

            GetLadderElements();

            GetDoorElements();

            GetBoxElements();

            GetRopeElements();

            GetButtonElements();
        }

        private void GetButtonElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if (TypeGrid[x, y] == TileType.Button)
                    {
                        var buttonIntention = IntentionGrid.Positions[x, y].Intentions.Where(i => i.Meaning == Meaning.Button).First();
                        GameElements.Add(
                            new Button()
                            {
                                Points = new List<Point>()
                                {
                                    new Point(x,y)
                                },
                                Momentary = buttonIntention.Info == "momentary",
                                ControlledElement = GameElements.Where(ge => 
                                    (
                                        (buttonIntention.RelatedTileMeaning.Meaning == Meaning.ToggleDoor && ge is Door)
                                        ||
                                        (buttonIntention.RelatedTileMeaning.Meaning == Meaning.Conveyor && ge is Conveyor)
                                        ||
                                        (buttonIntention.RelatedTileMeaning.Meaning == Meaning.Shooter && ge is Shooter)
                                    )
                                    && ge.Points.First().X == buttonIntention.RelatedTilePosition.X && 
                                    ge.Points.First().Y == buttonIntention.RelatedTilePosition.Y
                                ).First()
                            }
                        );
                    }
                }
            }
        }

        private void GetRopeElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if (TypeGrid[x, y] == TileType.HangingRope)
                    {
                        var yAdd = 0;
                        while (
                            y+yAdd < TypeGrid.GetLength(1) && 
                            TypeGrid[x, y + yAdd] != TileType.HangingRopeBottom
                        )
                            yAdd += 1;

                        GameElements.Add(
                            new Rope()
                            {
                                Points = new List<Point>()
                                {
                                    new Point(x,y),
                                    new Point(x,y+yAdd)
                                }
                            }
                        );
                    }
                }
            }
        }

        private void GetBoxElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if (TypeGrid[x, y] == TileType.Box)
                    {
                        GameElements.Add(
                            new Box()
                            {
                                Points = new List<Point>()
                                {
                                    new Point(x,y)
                                }
                            }
                        );
                    }
                }
            }
        }

        private void GetDoorElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if (TypeGrid[x, y] == TileType.Door)
                    {
                        GameElements.Add(
                            new Door()
                            {
                                Points = new List<Point>()
                                {
                                    new Point(x,y)
                                }
                            }
                        );
                    }
                }
            }
        }

        private void GetLadderElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if (TypeGrid[x, y] == TileType.LadderTop)
                    {
                        var yAdd = 0;
                        while (TypeGrid[x, y + yAdd] != TileType.LadderBottom && TypeGrid[x, y + yAdd] != TileType.LadderBottomWithConveyor)
                            yAdd += 1;

                        GameElements.Add(
                            new Ladder()
                            {
                                Points = new List<Point>()
                                {
                                    new Point(x,y),
                                    new Point(x,y+yAdd)
                                }
                            }
                        );
                    }
                }
            }
        }

        private void GetGroundElement()
        {
            var element = new Ground();
            for (var x = 0; x < Input.Width; x++)
            {
                for (var y = 0; y < Input.Height; y++)
                {
                    if (
                        Input.GetPixel(x, y).Equals(
                            GetBitmapByTileTypeFromRam(TileType.Solid).GetPixel(0, 0)
                        )
                    )
                        element.Points.Add(new Point(x, y));
                }
            }
            GameElements.Add(element);
        }

        private void GetShooterRightElements()
        {
            for(var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for(var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if(TypeGrid[x,y] == TileType.ShooterRight)
                    {
                        GameElements.Add(
                            new Shooter()
                            {
                                Points = new List<Point>()
                                {
                                    new Point(x,y),
                                    new Point(1,0)
                                }
                            }
                        );
                    }
                }
            }
        }



        private void GetShooterLeftElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if (TypeGrid[x, y] == TileType.ShooterLeft)
                    {
                        GameElements.Add(
                            new Shooter()
                            {
                                Points = new List<Point>()
                                {
                                    new Point(x,y),
                                    new Point(-1,0)
                                }
                            }
                        );
                    }
                }
            }
        }

        private void GetTypeGrid()
        {
            for (var x = 0; x < TileGrid.Count; x++)
            {
                for (var y = 0; y < TileGrid[x].Count; y++)
                {
                    var tileType = GetTileType(TileGrid[x][y]);
                    try
                    {
                        TypeGrid[x, y] = tileType.Value;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could Not Determine tile type.  Logged to file system.");
                        BitmapOperations.SaveBitmapToFile(Config.DebugTypeGridFolder + "//unknownTile-" + x.ToString() + "-" + y.ToString() + ".bmp", TileGrid[x][y]);
                    }
                }
            }
        }

        private void GetBitmapGrid()
        {
            TileGrid = BitmapOperations.GetBitmapTiles(Input, TileDim.X, TileDim.Y, false);
        }

        private void LoadTilesToRam()
        {
            if (Config == null)
            {
                for (var i = 0; i < ((TileType[])Enum.GetValues(typeof(TileType))).Count(); i++)// each(var type in (TileType[])Enum.GetValues(typeof(TileType)))
                {
                    var type = ((TileType[])Enum.GetValues(typeof(TileType))).First(ty => (int)ty == i);
                    var bitmap = GetBitmapByTileTypeFromSystem(type);
                    TileLibrary.Add(bitmap);
                }
            }
            else
            {
                LoadTilesToRam(Config);
            }
        }

        private void LoadTilesToRam(Configuration config)
        {
            for (var i = 0; i < ((TileType[])Enum.GetValues(typeof(TileType))).Count(); i++)// each(var type in (TileType[])Enum.GetValues(typeof(TileType)))
            {
                Bitmap tilesetImg = null;
                switch ((TileType)i){
                    case TileType.Box:
                        tilesetImg = Image.FromFile(config.BoxTile) as Bitmap;
                        break;
                    case TileType.Button:
                        tilesetImg = Image.FromFile(config.ButtonTile) as Bitmap;
                        break;
                    case TileType.Conveyor:
                        tilesetImg = Image.FromFile(config.ConveyorTile) as Bitmap;
                        break;
                    case TileType.Door:
                        tilesetImg = Image.FromFile(config.DoorTile) as Bitmap;
                        break;
                    case TileType.Empty:
                        tilesetImg = Image.FromFile(config.EmptyTile) as Bitmap;
                        break;
                    case TileType.Ground:
                        tilesetImg = Image.FromFile(config.GroundTile) as Bitmap;
                        break;
                    case TileType.HangingRope:
                        tilesetImg = Image.FromFile(config.HangingRopeTile) as Bitmap;
                        break;
                    case TileType.HangingRopeBottom:
                        tilesetImg = Image.FromFile(config.HangingRopeBottomTile) as Bitmap;
                        break;
                    case TileType.LadderBottom:
                        tilesetImg = Image.FromFile(config.LadderBottomTile) as Bitmap;
                        break;
                    case TileType.LadderBottomWithConveyor:
                        tilesetImg = Image.FromFile(config.LadderBottomWithConveyorTile) as Bitmap;
                        break;
                    case TileType.LadderMid:
                        tilesetImg = Image.FromFile(config.LadderMidTile) as Bitmap;
                        break;
                    case TileType.LadderTop:
                        tilesetImg = Image.FromFile(config.LadderTopTile) as Bitmap;
                        break;
                    case TileType.LedgeLeft:
                        tilesetImg = Image.FromFile(config.LedgeLeftTile) as Bitmap;
                        break;
                    case TileType.LedgeRight:
                        tilesetImg = Image.FromFile(config.LedgeRightTile) as Bitmap;
                        break;
                    case TileType.ShooterLeft:
                        tilesetImg = Image.FromFile(config.ShooterLeftTile) as Bitmap;
                        break;
                    case TileType.ShooterRight:
                        tilesetImg = Image.FromFile(config.ShooterRightTile) as Bitmap;
                        break;
                    case TileType.Solid:
                        tilesetImg = Image.FromFile(config.SolidTile) as Bitmap;
                        break;
                }
                
                TileLibrary.Add(tilesetImg);
            }
        }


        private Bitmap GetBitmapByTileTypeFromSystem(TileType tileType)
        {
            var path = ConfigurationManager.AppSettings[tileType.ToString() + "Tile"].ToString();
            Bitmap tilesetImg = Image.FromFile(path) as Bitmap;
            return tilesetImg;
        }

        private Bitmap GetBitmapByTileTypeFromRam(TileType tileType)
        {
            var index = (int)tileType;//Array.IndexOf(Enum.GetValues(typeof(TileType)), tileType);
            return TileLibrary[index];
        }

        private TileType? GetTileType(Bitmap bitmap)
        {
            foreach(var tileType in (TileType[])Enum.GetValues(typeof(TileType)))
            {
                if (BitmapIsTile(bitmap, tileType))
                    return tileType;
            }
            return null;
        }

        private bool BitmapIsTile(Bitmap Bitmap, TileType tileType)
        {
            var refTile = GetBitmapByTileTypeFromRam(tileType);
            return BitmapOperations.Compare(Bitmap, refTile);
        }


    }

}
