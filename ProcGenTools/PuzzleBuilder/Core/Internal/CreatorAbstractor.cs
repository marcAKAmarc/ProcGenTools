using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleBuilder.Core.Internal
{
    public abstract class CreatorAbstractor
    {
        public abstract Intention Execute(IntentionGrid grid, Random random, Intention inputIntention = null);
    }
}
