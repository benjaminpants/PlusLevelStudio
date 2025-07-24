using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelLoader
{
    [Serializable]
    public class ExtendedExtraLevelData : ExtraLevelData
    {
        public float timeOutTime = 0f;
        public RandomEvent timeOutEvent;
    }

    public class ExtendedExtraLevelDataAsset : ExtraLevelDataAsset
    {
        public float timeOutTime = 0f;
        public RandomEvent timeOutEvent;
    }
}
