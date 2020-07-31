using ProcGenTools.DataStructures;
using PuzzleBuilder.Core;
using PuzzleBuilder.Creators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PuzzleBuilder.Process
{
    public static class BasicCircuitProcess
    {
        public class PuzzleProcess:Process
        {
            public Random Random;
            public IntentionGrid Grid;
            public int[] GroundLevels;
            public TilesetConfiguration TilesConfig;
            public WcfGrid WcfGrid;
            public iMeaningConverter Converter;

            public PuzzleProcess() : base() { }

            public PuzzleProcess(Random random, IntentionGrid grid, TilesetConfiguration tilesConfig)
            {
                Init(random, grid, tilesConfig);
            }

            public override void Init(Random random, IntentionGrid grid, TilesetConfiguration tilesConfig) 
            {
                Grid = grid;
                Random = random;
                TilesConfig = tilesConfig;

                Converter = new MyMeaningConverter();

                WcfGrid = Helpers.InitWcfGrid(random, grid, TilesConfig);
            }

            public override void ClearForReuse(Random random)
            {
                Random = random;
                Grid.Clear();
                WcfGrid.ClearForReuse();
                WcfGrid.SetRandom(random);
            }

            public override PuzzleInfo CreateIt(List<Point> Entrances, List<Point> Exits)
            {
                GroundLevels = BasicCircuitCreators.CreateGroundLevels(Grid);

                //if all entrances have ground level
                var allPortals = new List<Point>();
                allPortals.AddRange(Entrances);
                allPortals.AddRange(Exits);
                if (PointsOnGroundLevels(GetHorizontalPortals(allPortals, Grid), Grid))
                {
                    BasicCircuitCreators.CreateCircuitFractured(Grid, Random);
                }
                else
                {
                    BasicCircuitCreators.CreateCircuit(Grid, Random);
                }

                var hEntrances = GetHorizontalPortals(Entrances, Grid);
                var vEntrances = GetVerticalPortals(Entrances, Grid);
                var vExits = GetVerticalPortals(Exits, Grid);
                var hExits = GetHorizontalPortals(Exits, Grid);

                foreach (var entrance in hEntrances)
                {
                    BasicCircuitCreators.CreateHorizontalEntrancePath(Grid, entrance.X, entrance.Y);
                }
                //Grid.Commit();
                foreach (var entrance in hExits)
                {
                    BasicCircuitCreators.CreateHorizontalExitPath(Grid, entrance.X, entrance.Y);
                }
                //Creators.CreateHorizontalExitPath(Grid, Exits.X, Exits.Y);
                foreach (var entrance in vEntrances)
                {
                    BasicCircuitCreators.CreateVerticalEntrancePath(Grid, entrance.X, entrance.Y);
                }
                foreach (var exit in vExits)
                {
                    BasicCircuitCreators.CreateVerticalExitPath(Grid, exit.X, exit.Y);
                }
                //Grid.Commit();
                BasicCircuitCreators.CreateToggleExitDoor(Grid);
                //Grid.Commit();
                BasicCircuitCreators.CreateToggleExitDoorButton(Grid, Random);
                //Grid.Commit();
                BasicCircuitCreators.CreateBoxButtonSection(Grid, Random);

                BasicCircuitCreators.CreateRopeSection(Grid, Random);

                BasicCircuitCreators.CreateShooterSection(Grid, Random);

                BasicCircuitCreators.CreateEnemy(Grid, Random);

                BasicCircuitCreators.CreateBorder(Grid, Random);

                BasicCircuitCreators.BlanketNonDynamic(Grid, Random);

                var result = Helpers.ApplyIntentionToGrid(Grid, WcfGrid, TilesConfig, Converter);

                return new PuzzleInfo()
                {
                    TileMap = Helpers.ToBitmap(WcfGrid, TilesConfig),
                    Grid = Grid,
                    Success = result
                };

            }
            private List<Point> GetHorizontalPortals(List<Point> points, IntentionGrid grid)
            {
                return points.Where(p => p.X == 0 || p.X == grid.Width - 1).ToList();
            }
            private List<Point> GetVerticalPortals(List<Point> points, IntentionGrid grid)
            {
                return points.Where(p => p.Y == 0 || p.Y == grid.Height - 1).ToList();
            }
            private bool PointsOnGroundLevels(List<Point> points, IntentionGrid grid)
            {
                var groundlevels = grid.GetByMeaning(Meaning.GroundLevel);
                return !points.Any(p => !groundlevels.Any(gl => gl.Y == p.Y));
            }

            public override void SetDisplayer(iDisplayer displayer)
            {
                throw new NotImplementedException();
            }
        }

        public class MyMeaningConverter : iMeaningConverter
        {
            public List<OpinionatedItem<Bitmap>> MeaningToTiles(Meaning meaning, TilesetConfiguration config)
            {
                var result = new List<OpinionatedItem<Bitmap>>();
                switch (meaning)
                {
                    case Meaning.Box:
                        result.AddRange(config.BoxTiles);
                        break;
                    case Meaning.BoxPath:
                        result.AddRange(config.HorizontalTraversableTiles);
                        result.AddRange(config.VerticalTraversableTiles);
                        result.AddRange(config.EmptyTiles);
                        break;
                    case Meaning.BoxPathVertical:
                        result.AddRange(config.FallTiles);
                        //result.AddRange(config.VerticalTraversableTiles);
                        //result.AddRange(config.EmptyTiles);
                        break;
                    case Meaning.HTraversablePlain:
                        result.AddRange(config.HorizontalTraversablePlainTiles);
                        break;
                    case Meaning.Circuit:
                        result.AddRange(config.HorizontalTraversableTiles);
                        result.AddRange(config.VerticalTraversableTiles);
                        break;
                    case Meaning.HTraversable:
                    case Meaning.GroundLevel:
                        result.AddRange(config.HorizontalTraversableTiles);
                        break;
                    case Meaning.VTraversable:
                        result.AddRange(config.VerticalTraversableTiles);
                        break;
                    case Meaning.Ladder:
                        result.AddRange(config.LadderTiles);
                        break;
                    case Meaning.VerticalExit:
                        result.AddRange(config.VerticalExitTiles);
                        break;
                    case Meaning.ToggleDoor:
                        result.AddRange(config.DoorTiles);
                        break;
                    case Meaning.Button:
                        result.AddRange(config.ButtonTiles);
                        break;
                    case Meaning.Rope:
                        result.AddRange(config.RopeTiles);
                        break;
                    case Meaning.Elevator:
                        result.AddRange(config.ElevatorTiles);
                        break;
                    case Meaning.Shooter:
                        result.AddRange(config.ShooterTiles);
                        break;
                    case Meaning.Conveyor:
                        result.AddRange(config.ConveyorTiles);
                        break;
                    case Meaning.ExitPath:
                    case Meaning.EntrancePath:
                        result.AddRange(config.HorizontalTraversablePlainTiles);
                        result.AddRange(config.VerticalTraversableTiles);//was verticaltraversableTiles
                        result.AddRange(config.DoorTiles);
                        break;
                    case Meaning.Solid:
                        result.AddRange(config.SolidTiles);
                        break;
                    case Meaning.Empty:
                        result.AddRange(config.EmptyTiles);
                        break;
                    case Meaning.SolidOrEmpty:
                        result.AddRange(config.EmptyTiles);
                        result.AddRange(config.SolidTiles);
                        break;
                    case Meaning.Enemy:
                        result.AddRange(config.EnemyTiles);
                        break;
                    case Meaning.NonDynamnic:
                        result.AddRange(config.NonDynamic);
                        break;
                    case Meaning.NonDynamicStrict:
                        result.AddRange(config.NonDynamicStrict);
                        break;
                    case Meaning.Walkable:
                        result.AddRange(config.Walkable);
                        break;
                    case Meaning.Ledge:
                        result.AddRange(config.LedgeTiles);
                        break;
                }
                return result;
            }
        }
    }
}
