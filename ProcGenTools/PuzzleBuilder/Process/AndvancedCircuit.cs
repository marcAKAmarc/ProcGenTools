using PuzzleBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProcGenTools.Helper;
using ProcGenTools.DataStructures;
using PuzzleBuilder.Creators;
using System.Drawing;
using PuzzleBuilder.Process;

namespace PuzzleBuilder.Process
{
    public static class AdvancedCircuitProcess
    {
        public class PuzzleProcess:Process
        {
            public Random Random;
            public IntentionGrid Grid;
            public int[] GroundLevels;
            public TilesetConfiguration TilesConfig;
            public WcfGrid WcfGrid;
            public iMeaningConverter Converter;

            public PuzzleProcess() { }

            public PuzzleProcess(Random random, IntentionGrid grid, TilesetConfiguration tilesConfig)
            {
                Init(random, grid, tilesConfig);
            }

            public override void Init(Random random, IntentionGrid grid, TilesetConfiguration tilesConfig)
            {
                Grid = grid;
                Random = random;
                TilesConfig = tilesConfig;

                Converter = new BasicCircuitProcess.MyMeaningConverter();

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

                BasicCircuitCreators.CreateRopeCube(Grid, Random);

                BasicCircuitCreators.CreateShooterSection(Grid, Random);

                BasicCircuitCreators.CreateEnemy(Grid, Random);

                //box Impede door
                var boxImpedePoint = BasicCircuitCreators.CreateBoxPathImpedingExtraDoor(Grid, Random);
                if (boxImpedePoint != null)
                {
                    var buttonResult = BasicCircuitCreators.CreateExtraDoorButton(Grid, Random, boxImpedePoint.Value);
                    if (buttonResult == false)
                    {
                        //undo last door creation if needed
                        Grid.Positions[boxImpedePoint.Value.X, boxImpedePoint.Value.Y].Intentions = Grid.Positions[boxImpedePoint.Value.X, boxImpedePoint.Value.Y].Intentions.Where(x => x.Meaning != Meaning.ToggleDoor).ToList();
                    }
                }

                //create extra door
                var doorloopsuccess = true;
                var maxAmount = 1;
                while (doorloopsuccess && maxAmount > 0)
                {
                    var resultingPoint = BasicCircuitCreators.CreateExtraDoor(Grid, Random);
                    if (resultingPoint != null)
                    {
                        var buttonResult = BasicCircuitCreators.CreateExtraDoorButton(Grid, Random, resultingPoint.Value);
                        if (buttonResult == false)
                        {
                            //undo last door creation if needed
                            Grid.Positions[resultingPoint.Value.X, resultingPoint.Value.Y].Intentions = Grid.Positions[resultingPoint.Value.X, resultingPoint.Value.Y].Intentions.Where(x => x.Meaning != Meaning.ToggleDoor).ToList();
                            doorloopsuccess = false;
                        }
                    }
                    else
                    {
                        doorloopsuccess = false;
                    }
                    maxAmount -= 1;
                }

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
        }

    }
       
}
