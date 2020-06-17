using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PuzzleBuilder.Process
{
    public abstract class Process
    {
        public Process() { }
        public abstract void Init(Random random, IntentionGrid grid, TilesetConfiguration tilesConfig);
        public abstract PuzzleInfo CreateIt(List<Point> Entrances, List<Point> Exits);
        public abstract void ClearForReuse(Random random);
    }
}
