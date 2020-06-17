using ProcGenTools.DataProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PuzzleBuilder
{
    public class ProcessFactory<T> where T : Process.Process, new()
    {
        private T myProcess;
        private IntentionGrid myGrid;
        private TilesetConfiguration myTilesetConfiguration;
        private string _wfcDebugPath;
        private string _tilesetDebugPath;
        private Random random;

        private int effectiveSeed = 0;
        private int maxAttempts = 10;
        public int currentAttempt = 0;
        private int attemptOffset = 1000;
        private PuzzleInfo LastResult;
        private int LastSeed = 0;
        private int LastEffectiveSeed = 0;

        public ProcessFactory(int gridWidth, int gridHeight, string wfcDebugPath, string tilesetDebugPath){
            myGrid = new IntentionGrid(gridWidth, gridHeight);
            random = new marcRandom(0);
            myTilesetConfiguration = new TilesetConfiguration(wfcDebugPath, tilesetDebugPath);
            myProcess = new T();
            myProcess.Init(random, myGrid, myTilesetConfiguration);
            _wfcDebugPath = wfcDebugPath;
            _tilesetDebugPath = tilesetDebugPath;

        }

        public PuzzleInfo GetPuzzle(int seed, List<Point> Entrances, List<Point> Exits)
        {
            PuzzleInfo result = null;

            effectiveSeed = seed;
            currentAttempt = 0;

            while( currentAttempt < maxAttempts && 
                (result == null || result.Success == false)
            )
            {
                random = new marcRandom(effectiveSeed);
                myProcess.ClearForReuse(random);
                result = myProcess.CreateIt(Entrances, Exits);
                if(result == null || result.Success == false)
                {
                    effectiveSeed += attemptOffset;
                    currentAttempt += 1;
                }
            }

            LastResult = result;
            LastSeed = seed;
            LastEffectiveSeed = effectiveSeed;

            if (result == null || result.Success == false)
            {
                throw new Exception("Processor failed to create a puzzle.");
            }

            

            return result;
        }

        public void SaveResult(string folderpath)
        {
                BitmapOperations.SaveBitmapToFile(
                    folderpath + "Seed" + LastSeed.ToString() + "Effective" + LastEffectiveSeed.ToString() + ".bmp"
                    , LastResult.TileMap
                );
        }
        
    }
}
