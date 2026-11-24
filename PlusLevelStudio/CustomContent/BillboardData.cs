using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.CustomContent
{
    [Serializable]
    public class BillboardData
    {
        public string sprite;
        public string offSprite;
        public float[] pivot = new float[] { 0.5f, 0.5f };
        public float pixelsPerUnit = 50f;
        public bool billboarded = false;
    }
}
