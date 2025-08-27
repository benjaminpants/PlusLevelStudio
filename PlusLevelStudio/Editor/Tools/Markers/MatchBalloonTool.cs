using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor.Tools
{
    public class MatchBalloonTool : PostionMarkerPlaceTool
    {
        public MatchBalloonTool() : base("matchballoon")
        {
            verticalOffset = 5f;
        }

        protected override PositionMarker MakeMarker()
        {
            return new MatchBalloonMarker() { type = "matchballoon" };
        }
    }
}
