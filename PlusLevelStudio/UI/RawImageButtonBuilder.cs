using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class RawImageButtonBuilder : RawImageBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject b = base.Build(parent, handler, data);
            GameObject collision = new ButtonBuilder().Build(parent, handler, data);
            collision.name += "_Collision";
            collision.GetComponent<Image>().color = Color.clear;
            return b;
        }
    }
}
