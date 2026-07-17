using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusLevelStudio.Campaigns
{
    public struct LifeModeData
    {
        public LifeMode mode;
        public string localizationKey;
        public int lifeCount;

        public LifeModeData(LifeMode mode, string key = null, int lives = 2)
        {
            this.mode = mode;
            localizationKey = key;
            if (localizationKey == null)
            {
                localizationKey = "Opt_LifeMode_" + mode.ToStringExtended();
            }
            lifeCount = lives;
        }
    }
}
