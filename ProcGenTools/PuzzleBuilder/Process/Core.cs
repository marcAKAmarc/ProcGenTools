using PuzzleBuilder.Core;
using RoomEditor.Models;
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
        public abstract PuzzleInfo CreateIt(List<Portal> portals, int? keyLockId = null, bool isSkippable = false);
        public abstract void ClearForReuse(Random random);
        public abstract void SetDisplayer(iDisplayer displayer);
    }
}
