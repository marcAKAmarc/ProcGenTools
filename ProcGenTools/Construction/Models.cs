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
using Construction.Properties;

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
        HangingRopeBottom = 16,
        Enemy = 17,
        DoorH = 18
    }

    public class GameElement {
        public List<Point> Points;
        public Guid Id;
        public GameElement()
        {
            Points = new List<Point>();
            Id = Guid.NewGuid();
        }
    }

    public class Shooter : GameElement { }
    public class Ladder : GameElement { }
    public class Conveyor : GameElement {
        public string direction;
    }
    public class Button : GameElement {
        public bool Momentary;
        public GameElement ControlledElement;
    }
    public class Door : GameElement {
        public int? LockId;
        public bool Horizontal; 
    }
    public class Box : GameElement { }
    public class Enemy : GameElement { }
    public class Rope : GameElement { }
    public class Ground : GameElement {
        
    }
    public class Loot : GameElement { }
    public class Key : GameElement
    {
        public int LockId;
    }

}

namespace ConstructionProcessing {

    public class Configuration
    {
        public Bitmap GroundTile;
        public Bitmap SolidTile;
        public Bitmap EmptyTile;
        public Bitmap EnemyTile;
        public Bitmap LedgeRightTile;
        public Bitmap LedgeLeftTile;
        public Bitmap ShooterRightTile;
        public Bitmap ShooterLeftTile;
        public Bitmap LadderTopTile;
        public Bitmap LadderMidTile;
        public Bitmap LadderBottomTile;
        public Bitmap LadderBottomWithConveyorTile;
        public Bitmap ConveyorTile;
        public Bitmap ButtonTile;
        public Bitmap DoorTile;
        public Bitmap DoorHTile;
        public Bitmap BoxTile;
        public Bitmap HangingRopeTile;
        public Bitmap HangingRopeBottomTile;

        public string DebugTypeGridFolder;

        public Configuration( string debugTypeGridFolder )
        {
            GroundTile = Resources.ground;
            SolidTile = Resources.solid;
            EmptyTile = Resources.empty;
            EnemyTile = Resources.enemy;
            LedgeRightTile = Resources.ledgeRight;
            LedgeLeftTile = Resources.ledgeLeft;
            ShooterRightTile = Resources.shooterRight;
            ShooterLeftTile = Resources.shooterLeft;
            LadderTopTile = Resources.ladderTop;
            LadderMidTile = Resources.ladderMid;
            LadderBottomTile = Resources.ladderBottom;
            LadderBottomWithConveyorTile = Resources.ladderBottomConveyor;
            ConveyorTile = Resources.conveyor;
            ButtonTile = Resources.button;
            DoorTile = Resources.door;
            DoorHTile = Resources.doorH;
            BoxTile = Resources.box;
            HangingRopeTile = Resources.rope;
            HangingRopeBottomTile = Resources.ropeBottom;

            DebugTypeGridFolder = debugTypeGridFolder;
        }
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

        public Processor(int TileX, int TileY)
        {
            initializer(TileX, TileY);
        }

        public Processor(int TileX, int TileY, Configuration config)
        {
            Config = config;
            initializer(TileX, TileY);
        }
        public void ClearForReuse()
        {
           //foreach(var list in TileGrid)
           // {
           //     list.Clear();
           // }

            //for(var x = 0; x < IntentionGrid.Width; x++)
            //{
            //    for(var y = 0; y < IntentionGrid.Height; y++)
            //    {
            //        IntentionGrid.Positions[x, y].Intentions.Clear();
            //    }
            //}

            //typegrid has to just be overwritten pretty much
            //have to put that in here i guess I dunno.
            GameElements.Clear();
        }

        private void initializer(int TileX, int TileY)
        {
            //file cleanup
            if (Config != null && Config.DebugTypeGridFolder != null)
            {
                if (!Directory.Exists(Config.DebugTypeGridFolder))
                    Directory.CreateDirectory(Config.DebugTypeGridFolder);
                string[] filePaths = Directory.GetFiles(Config.DebugTypeGridFolder);
                foreach (string filePath in filePaths)
                    File.Delete(filePath);
            }

            TileDim = new Point(TileX, TileY);
            
            TileLibrary = new List<Bitmap>();
            

            
            

            LoadTilesToRam();
            GameElements = new List<GameElement>();
        }

        public void DoIt(Bitmap InputBitmap, IntentionGrid Intentions)
        {
            Input = InputBitmap;
            IntentionGrid = Intentions;
            GetBitmapGrid();

            TypeGrid = new TileType[TileGrid.Count, TileGrid[0].Count];
            GetTypeGrid();
            GetGameElements();
        }

        private void GetGameElements()
        {
            GetGroundElement();

            GetShooterLeftElements();
            GetShooterRightElements();

            GetOnscreenLadderElements();

            GetDoorElements();

            GetBoxElements();

            GetRopeElements();

            GetConveyorElements();

            GetButtonElements();

            GetEnemyElements();

            GetKeyGameElements();

            GetLootGameElements();
        }

        private void GetButtonElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if (TypeGrid[x, y] == TileType.Button)
                    {
                        var buttonIntention = IntentionGrid.Positions[x, y].Intentions.Where(i => i.Meaning == Meaning.Button).FirstOrDefault();
                        if (buttonIntention == null)
                            return;
                        if (buttonIntention.RelatedTileMeaning == null)
                            throw new Exception("Button Construction failed - button is not hooked up to anything!");

                        try
                        {
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
                        catch(Exception ex)
                        {
                            var breaka = "here";
                        }

                        if(((Button)GameElements.Last()).ControlledElement is Door && ((Door)((Button)GameElements.Last()).ControlledElement).LockId != null)
                        {
                            var breaka = "here";
                        }
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

        private void GetEnemyElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    if (TypeGrid[x, y] == TileType.Enemy)
                    {
                        GameElements.Add(
                            new Enemy()
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
                    if (TypeGrid[x, y] == TileType.Door || TypeGrid[x, y] == TileType.DoorH)
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
                        Intention intention = IntentionGrid.Positions[x, y].Intentions.Where(i => i.Meaning == Meaning.ToggleDoor).FirstOrDefault();
                        if(intention == null)
                        {
                            var breaka = "here";
                        }

                        if (intention.Info != null && intention.Info.Contains("LockId:"))
                        {
                            ((Door)GameElements.Last()).LockId = int.Parse(intention.Info.Replace("LockId:", ""));
                        }

                        if (TypeGrid[x, y] == TileType.DoorH)
                            ((Door)GameElements.Last()).Horizontal = true;
                    }
                }
            }
        }

        private void GetOnscreenLadderElements()
        {
            for (var x = 0; x < TypeGrid.GetLength(0); x++)
            {
                for (var y = 0; y < TypeGrid.GetLength(1); y++)
                {
                    //if at ladder top or any ladder type on top row
                    if (
                        TypeGrid[x, y] == TileType.LadderTop || (
                            y == 0 && (
                                TypeGrid[x, y] == TileType.DoorH
                                ||
                                TypeGrid[x, y] == TileType.LadderMid
                                ||
                                TypeGrid[x, y] == TileType.LadderBottom
                                ||
                                TypeGrid[x, y] == TileType.LadderBottomWithConveyor
                            )
                        )
                    )
                    {
                        var yAdd = 0;
                        while (TypeGrid[x, y + yAdd] != TileType.LadderBottom 
                            && TypeGrid[x, y + yAdd] != TileType.LadderBottomWithConveyor
                            && y + yAdd != TypeGrid.GetLength(1) - 1
                        )
                            yAdd += 1;
                        if(y + yAdd >= TypeGrid.GetLength(1))
                        {
                            //we ran off the bottom of the screen, so this is a transitional ladder
                            continue;
                        }


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


        private void GetConveyorElements()
        {
            string dir = "";
            for(var y = 0; y < TypeGrid.GetLength(1); y++)
            {
                for (var x = 0; x < TypeGrid.GetLength(0); x++)
                {
                    if(TypeGrid[x,y] == TileType.Conveyor || TypeGrid[x,y] == TileType.LadderBottomWithConveyor)
                    {
                        var xStart = x;
                        dir = IntentionGrid.Positions[xStart, y].Intentions.Where(i => i.Meaning == Meaning.Conveyor).First().Info;
                        while (
                            x < TypeGrid.GetLength(0)
                            && (TypeGrid[x,y] == TileType.Conveyor || TypeGrid[x, y] == TileType.LadderBottomWithConveyor)
                            && IntentionGrid.Positions[x, y].Intentions.Where(i => i.Meaning == Meaning.Conveyor).First().Info == dir
                        )
                        {
                            x++;
                        }
                        GameElements.Add(
                            new Conveyor()
                            {
                                direction = IntentionGrid.Positions[xStart, y].Intentions.Where(i=>i.Meaning == Meaning.Conveyor).First().Info,
                                Points = new List<Point>()
                                {
                                    new Point(xStart, y),
                                    new Point(x, y)
                                }
                            }
                        );
                    }
                }
            }
        }

        private void GetKeyGameElements()
        {
            foreach(IntentionTiles i in IntentionGrid.GetByMeaning(Meaning.Key))
            {
                GameElements.Add(
                    new Key()
                    {
                        LockId = int.Parse(i.Intentions.First(f => f.Meaning == Meaning.Key).Info.Replace("LockId:", "")),
                        Points = new List<Point>()
                        {
                            new Point(i.X, i.Y)
                        }
                    }
                );
            }
        }

        private void GetLootGameElements()
        {
            foreach(IntentionTiles i in IntentionGrid.GetByMeaning(Meaning.Loot))
            {
                GameElements.Add(
                    new Loot()
                    {
                        Points = new List<Point>()
                        {
                            new Point(i.X, i.Y)
                        }
                    }
                );
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
                        if (Config.DebugTypeGridFolder != null)
                        {
                            Console.WriteLine("Could Not Determine tile type.  Logged to file system.");
                            BitmapOperations.SaveBitmapToFile(Config.DebugTypeGridFolder + "//unknownTile-" + x.ToString() + "-" + y.ToString() + ".bmp", TileGrid[x][y]);
                        }
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
                        tilesetImg = config.BoxTile as Bitmap;
                        break;
                    case TileType.Button:
                        tilesetImg = config.ButtonTile as Bitmap;
                        break;
                    case TileType.Conveyor:
                        tilesetImg = config.ConveyorTile as Bitmap;
                        break;
                    case TileType.Door:
                        tilesetImg = config.DoorTile as Bitmap;
                        break;
                    case TileType.DoorH:
                        tilesetImg = config.DoorHTile as Bitmap;
                        break;
                    case TileType.Empty:
                        tilesetImg = config.EmptyTile as Bitmap;
                        break;
                    case TileType.Ground:
                        tilesetImg = config.GroundTile as Bitmap;
                        break;
                    case TileType.HangingRope:
                        tilesetImg = config.HangingRopeTile as Bitmap;
                        break;
                    case TileType.HangingRopeBottom:
                        tilesetImg = config.HangingRopeBottomTile as Bitmap;
                        break;
                    case TileType.LadderBottom:
                        tilesetImg = config.LadderBottomTile as Bitmap;
                        break;
                    case TileType.LadderBottomWithConveyor:
                        tilesetImg = config.LadderBottomWithConveyorTile as Bitmap;
                        break;
                    case TileType.LadderMid:
                        tilesetImg = config.LadderMidTile as Bitmap;
                        break;
                    case TileType.LadderTop:
                        tilesetImg = config.LadderTopTile as Bitmap;
                        break;
                    case TileType.LedgeLeft:
                        tilesetImg = config.LedgeLeftTile as Bitmap;
                        break;
                    case TileType.LedgeRight:
                        tilesetImg = config.LedgeRightTile as Bitmap;
                        break;
                    case TileType.ShooterLeft:
                        tilesetImg = config.ShooterLeftTile as Bitmap;
                        break;
                    case TileType.ShooterRight:
                        tilesetImg = config.ShooterRightTile as Bitmap;
                        break;
                    case TileType.Solid:
                        tilesetImg = config.SolidTile as Bitmap;
                        break;
                    case TileType.Enemy:
                        tilesetImg = config.EnemyTile as Bitmap;
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
