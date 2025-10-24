using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelFormat
{
    public class RandomStructureInfo
    {
        public string type;
        public StructureParameterInfo info = new StructureParameterInfo();
    }

    public class StructureParameterInfo
    {
        public List<float> chance = new List<float>();

        public List<MystIntVector2> minMax = new List<MystIntVector2>();

        public List<WeightedPrefab> prefab = new List<WeightedPrefab>();
    }

    public class WeightedPrefab
    {
        public string prefab;
        public int weight;
    }
}
