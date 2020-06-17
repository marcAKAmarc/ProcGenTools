using Construction.Models;
using PuzzleBuilder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Construction
{
    public class Factory
    {
        private ConstructionProcessing.Processor myProcessor = null;
        private ConstructionProcessing.Configuration myConfiguration = null;
        public Factory(string getDebugTypeGridPath, int TileWidth, int TileHeight)
        {
            myConfiguration = new ConstructionProcessing.Configuration(getDebugTypeGridPath);
            myProcessor = new ConstructionProcessing.Processor(TileWidth, TileHeight, myConfiguration);
        }

        public List<GameElement> GetGameElements(Bitmap input, IntentionGrid grid)
        {
            myProcessor.ClearForReuse();
            myProcessor.DoIt(input, grid);
            return myProcessor.GameElements;
        } 

    }
}
