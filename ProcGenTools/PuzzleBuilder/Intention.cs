using ProcGenTools.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace PuzzleBuilder
{
    public class IntentionDependencies
    {
        public IntentionGrid grid;
        public List<Point> entrances;
        public List<Point> exits;
        Random random;
    }
    public delegate void Intend(IntentionDependencies parameters);

    public static class Intentions
    {
        public static int[] CreateGroundLevels(IntentionDependencies p)
        {
            var floorHeight = 2.0;
            var groundLevels = new int[(int)Math.Floor((p.grid.Height - 2) / floorHeight)];
            for (var i = 0; i < groundLevels.Count(); i++)
            {
                groundLevels[i] = (int)Math.Floor((i) * (p.grid.Height - 2) / (float)groundLevels.Count());
                groundLevels[i] += (int)floorHeight;
            }

            for (var x = 1; x < p.grid.Width - 1; x++)
            {
                for (var y = 0; y < p.grid.Height; y++)
                {
                    if (groundLevels.Any(gl => gl == y))
                    {
                        p.grid.Positions[x, y].Intentions.Add(Intention.GroundLevelIntention());
                    }
                }
            }

            DebugPrintMeaning(p.grid, Meaning.GroundLevel);

            return groundLevels;
        }

        public static void CreateCircuit(IntentionDependencies p)
        {
            var verticals = new int[2];
            verticals[0] = (int)Math.Floor(p.grid.Width / 4.0f);
            verticals[1] = (int)Math.Floor(3.0f * p.grid.Width / 4.0f);

            var groundLevels = p.grid.GetByMeaning(Meaning.GroundLevel);
            var groundLevelsMin = groundLevels.Min(x => x.Y);
            var groundLevelsMax = groundLevels.Max(x => x.Y);

            for (var x = 0; x < p.grid.Width; x++)
            {
                for (var y = 0; y < p.grid.Height; y++)
                {
                    if (
                        !(
                            y >= groundLevelsMin
                            && y <= groundLevelsMax
                            && x >= verticals[0]
                            && x <= verticals[1]
                        )
                    )
                        continue;

                    if (
                        (y == groundLevelsMin || y == groundLevelsMax)
                        || (x == verticals[0] || x == verticals[1])
                    )
                    {
                        p.grid.Positions[x, y].Intentions.Add(Intention.CircuitIntention());
                    }

                    if (x == verticals[0] || x == verticals[1])
                    {
                        p.grid.Positions[x, y].Intentions.Add(Intention.LadderIntention());
                    }
                }
            }


            //Debug
            DebugPrintMeaning(p.grid, Meaning.Circuit);
        }

        public static void CreateHorizontalEntrancePath(IntentionDependencies p)
        {
            var circuits = p.grid.GetByMeaning(Meaning.Circuit);
            var leftMost = circuits.Select(x => x.X).Min();
            var rightMost = circuits.Select(x => x.X).Max();

            foreach(var entrance in p.entrances.Where(e=>e.X == 0 || e.X == p.grid.Width)) { }
            var xDir = 0;
            var xStart = 0;
            var xStop = leftMost;
            if (entranceX <= 0)
                xDir = 1;
            else
            {
                xDir = -1;
                xStart = grid.Width - 1;
                xStop = rightMost;
            }

            for (var x = xStart; x != xStop; x += xDir)
            {
                grid.Positions[x, entranceY].Intentions.Add(Intention.EntrancePathIntention());
            }

            //Debug
            DebugPrintMeaning(grid, Meaning.EntrancePath);
        }


        public static void CreateHorizontalExitPath(IntentionGrid grid, int exitX, int exitY)
        {
            var circuits = grid.GetByMeaning(Meaning.Circuit);
            var leftMost = circuits.Select(x => x.X).Min();
            var rightMost = circuits.Select(x => x.X).Max();

            var xDir = 0;
            var xStart = 0;
            var xStop = leftMost;
            if (exitX <= 0)
                xDir = 1;
            else
            {
                xDir = -1;
                xStart = grid.Width - 1;
                xStop = rightMost;
            }

            for (var x = xStart; x != xStop; x += xDir)
            {
                grid.Positions[x, exitY].Intentions.Add(Intention.ExitPathIntention());
            }

            DebugPrintMeaning(grid, Meaning.ExitPath);
        }

        public static void CreateToggleExitDoor(IntentionGrid grid)
        {
            var tile = grid.GetByMeaning(Meaning.ExitPath).Where(t => t.X == grid.Width - 1 || t.X == 0).FirstOrDefault();
            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ToggleDoorIntention());

            DebugPrintMeaning(grid, Meaning.ToggleDoor);
        }

        public static void CreateToggleExitDoorButton(IntentionGrid grid, Random random)
        {
            //get a humble GroundLevel Tile
            var tiles = grid.GetByMeaning(Meaning.GroundLevel).Where(t => grid.Positions[t.X, t.Y].Intentions.Count == 1).ToList();
            var tile = tiles[random.Next(0, tiles.Count())];
            var buttonIntention = Intention.ButtonIntention();
            var exit = grid.GetByMeaning(Meaning.ExitPath).Where(t => grid.Positions[t.X, t.Y].Intentions.Any(i => i.Meaning == Meaning.ToggleDoor)).First().Intentions.Where(i => i.Meaning == Meaning.ToggleDoor).First();
            buttonIntention.RelatedTileMeaning = exit;
            exit.RelatedTileMeaning = buttonIntention;
            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ButtonIntention());

            DebugPrintMeaning(grid, Meaning.Button);
        }

        public static void CreateRopeSection(IntentionGrid grid, Random random)
        {
            //TODO - RANDOMIZE THIS

            //get a humble groundlevel tile
            var tiles = grid.GetByMeaning(Meaning.GroundLevel).Where(t => grid.Positions[t.X, t.Y].Intentions.Count == 1).ToList();

            //find 3 in a row
            var middle = tiles.Where(t => tiles.Any(tleft => tleft.Y == t.Y && tleft.X == t.X - 1) && tiles.Any(tright => tright.Y == t.Y && tright.X == t.X + 1)).FirstOrDefault();
            if (middle != null)
            {
                /*for(var x = middle.X -1; x <= middle.X +1; x++)
                {
                    grid.Positions[x, middle.Y].Intentions.Add(Intention.RopeIntention());
                }*/
                grid.Positions[middle.X, middle.Y].Intentions.Add(Intention.RopeIntention());
            }

            DebugPrintMeaning(grid, Meaning.Rope);
        }

        public static void CreateBorder(IntentionGrid grid, Random random)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                var str = "";
                for (var x = 0; x < grid.Width; x++)
                {
                    if (
                        (x == 0 || y == 0 || x == grid.Width - 1 || y == grid.Height - 1)
                        &&
                        grid.Positions[x, y].Intentions.Count() == 0
                    )
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.SolidIntention());
                    }
                }
            }

            DebugPrintMeaning(grid, Meaning.Solid);
        }

        private static void DebugPrintMeaning(IntentionGrid grid, Meaning meaning)
        {
            //Debug
            Console.WriteLine(meaning.ToString() + ":  ");
            for (var y = 0; y < grid.Height; y++)
            {
                var str = "";
                for (var x = 0; x < grid.Width; x++)
                {

                    if (grid.Positions[x, y].Intentions.Any(i => i.Meaning == meaning))
                        str = str + "X";
                    else
                        str = str + "-";
                }
                Console.WriteLine(str);
            }
            Console.WriteLine("");
        }
    }
}
