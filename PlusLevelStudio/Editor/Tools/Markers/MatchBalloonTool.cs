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

        protected override bool TryPlace(IntVector2 position)
        {
            bool ret = base.TryPlace(position);
            if (ret)
            {
                SoundPlayOneshot("MatchBalloon_Revealed");
            }
            return ret;
        }
    }
}
