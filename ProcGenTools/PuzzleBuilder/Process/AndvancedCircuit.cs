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
using ProcGenTools.DataProcessing;
using PuzzleBuilder.Core;
using RoomEditor.Models;

namespace PuzzleBuilder.Process
{
    public static class AdvancedCircuitProcess
    {

        public class PuzzleProcess:Process, WcfGridEventHandler
        {
            public Random Random;
            public IntentionGrid Grid;
            public int[] GroundLevels;
            public TilesetConfiguration TilesConfig;
            public WcfGrid WcfGrid;
            public iDisplayer Displayer;
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
                WcfGrid.eventHandler = this; 
            }

            public override void SetDisplayer(iDisplayer displayer)
            {
                Displayer = displayer;
            }

            public override void ClearForReuse(Random random)
            {
                Random = random;
                Grid.Clear();
                if (WcfGrid.Width != Grid.Width || WcfGrid.Height != Grid.Height)
                    WcfGrid = Helpers.InitWcfGrid(random, Grid, TilesConfig);
                else
                {
                    WcfGrid.ClearForReuse();
                    WcfGrid.SetRandom(random);

                }
                WcfGrid.eventHandler = this;
            }

            public override PuzzleInfo CreateIt(List<Portal> portals, int? keyLockId = null, bool isSkippable = false)
            {
                int[] GroundLevels;
                if(Random.NextDouble() >= .5d)
                    GroundLevels = BasicCircuitCreators.CreateGroundLevels(Grid);
                else
                    GroundLevels = BasicCircuitCreators.CreateMinimalGroundLevels(Grid);

                
                if (PortalsOnGroundLevels(GetHorizontalPortals(portals, Grid), Grid))
                {
                    BasicCircuitCreators.CreateCircuitFractured(Grid, Random);
                }
                else
                {
                    BasicCircuitCreators.CreateCircuit(Grid, Random);
                }

                var hEntrances = GetHorizontalPortals(portals.Where(p=>p.IsEntrance()).ToList(), Grid);
                var vEntrances = GetVerticalPortals(portals.Where(p=>p.IsEntrance()).ToList(), Grid);
                var vExits = GetVerticalPortals(portals.Where(p=>!p.IsEntrance()).ToList(), Grid);
                var hExits = GetHorizontalPortals(portals.Where(p=>!p.IsEntrance()).ToList(), Grid);

                foreach (var entrance in hEntrances)
                {
                    BasicCircuitCreators.CreateHorizontalEntrancePath(Grid, entrance.point.X, entrance.point.Y);
                }
                //Grid.Commit();
                foreach (var entrance in hExits)
                {
                    BasicCircuitCreators.CreateHorizontalExitPath(Grid, entrance.point.X, entrance.point.Y);
                }
                //Creators.CreateHorizontalExitPath(Grid, Exits.X, Exits.Y);
                foreach (var entrance in vEntrances)
                {
                    BasicCircuitCreators.CreateVerticalEntrancePath(Grid, entrance.point.X, entrance.point.Y);
                }
                foreach (var exit in vExits)
                {
                    BasicCircuitCreators.CreateVerticalExitPath(Grid, exit.point.X, exit.point.Y);
                }

                BasicCircuitCreators.CreateToggleExitDoor(Grid, portals.Where(x=>!x.IsEntrance() && !x.IsDestinationSkippable()).ToList());

                BasicCircuitCreators.CreateToggleExitDoorButton(Grid, Random);

                //conveyor jump
                if (Random.NextDouble() < .666f)
                {
                    BasicCircuitCreators.AddConveyorJumpToHiddenPlace(Grid, Random);
                    if (Random.NextDouble() >= .5d)
                        BasicCircuitCreators.LengthenExistingConveyor(Grid, Random);
                }

                //rope cube
                if (Random.NextDouble() < .666f)
                    BasicCircuitCreators.CreateRopeCube(Grid, Random);

                //conveyor jump to nothing maybe
                if (Random.NextDouble() < .666f)
                {
                    BasicCircuitCreators.AddConveyorJump(Grid, Random);
                    if (Random.NextDouble() >= .5d)
                        BasicCircuitCreators.LengthenExistingConveyor(Grid, Random);
                }

                //box push to button
                if (Random.NextDouble() < .666f)
                    BasicCircuitCreators.CreateBoxButtonSection(Grid, Random);

                //shooters
                if (Random.NextDouble() < .333f)
                    BasicCircuitCreators.CreateShooterSection(Grid, Random);
                if(Random.NextDouble() < .333f)
                    BasicCircuitCreators.CreateShooterSection(Grid, Random);

                //add random conveyor
                if (Random.NextDouble() < .666f)
                {
                    BasicCircuitCreators.AddConveyor(Grid, Random);
                    if (Random.NextDouble() >= .5d)
                        BasicCircuitCreators.LengthenExistingConveyor(Grid, Random);
                }

                
                //enemy
                if (Random.NextDouble() < .5f)
                    BasicCircuitCreators.CreateEnemy(Grid, Random);

                //impede box door
                if (Random.NextDouble() < .666f)
                {
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
                }

                //random extra door
                if (Random.NextDouble() < .666f)
                {
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
                }

                if(keyLockId != null)
                {
                    BasicCircuitCreators.PlaceKey(Grid, Random, keyLockId.Value);
                }

                if (isSkippable)
                {
                    BasicCircuitCreators.PlaceLoot(Grid, Random);
                }

                BasicCircuitCreators.CreateBorder(Grid, Random);
                BasicCircuitCreators.AddSolidToSidesOfCircuit(Grid, Random);
                BasicCircuitCreators.BlanketNonDynamic(Grid, Random);

                var result = Helpers.ApplyIntentionToGrid(Grid, WcfGrid, TilesConfig, Converter);

                return new PuzzleInfo()
                {
                    TileMap = Helpers.ToBitmap(WcfGrid, TilesConfig),
                    Grid = Grid,
                    Success = result
                };

            }
            private List<Portal> GetHorizontalPortals(List<Portal> Portals, IntentionGrid grid)
            {
                return Portals.Where(p => p.point.X == 0 || p.point.X == grid.Width - 1).ToList();
            }
            private List<Portal> GetVerticalPortals(List<Portal> Portals, IntentionGrid grid)
            {
                return Portals.Where(p => p.point.Y == 0 || p.point.Y == grid.Height - 1).ToList();
            }
            private bool PortalsOnGroundLevels(List<Portal> Portals, IntentionGrid grid)
            {
                var groundlevels = grid.GetByMeaning(Meaning.GroundLevel);
                return !Portals.Any(p => !groundlevels.Any(gl => gl.Y == p.point.Y));
            }

            void WcfGridEventHandler.OnPropagation()
            {
                if (Displayer != null)
                    Displayer.Display(Helpers.ToBitmap(WcfGrid, TilesConfig));
                //BitmapOperations.SaveBitmapToFile("D:\\_Projects\\ProcGenTools\\ProcGenTools\\Test.World.Circuit\\Output\\liveUpdate.bmp", Helpers.ToBitmap(WcfGrid, TilesConfig));
            }

            void WcfGridEventHandler.OnCollapse()
            {
                if (Displayer != null)
                    Displayer.Display(Helpers.ToBitmap(WcfGrid, TilesConfig));
                //BitmapOperations.SaveBitmapToFile("D:\\_Projects\\ProcGenTools\\ProcGenTools\\Test.World.Circuit\\Output\\liveUpdate.bmp", Helpers.ToBitmap(WcfGrid, TilesConfig));

            }
        }

    }
       
}
