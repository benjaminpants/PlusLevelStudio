using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.UI
{
    public class DragDetectorBuilder : ImageBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject b = base.Build(parent, handler, data);
            b.gameObject.tag = "Button";
            MouseDragDetector dragDetect = b.AddComponent<MouseDragDetector>();
            if (data.ContainsKey("onDrag"))
            {
                string dragPre = data["onDrag"].Value<string>();
                dragDetect.onDrag = (Vector3 off) =>
                {
                    handler.SendInteractionMessage(dragPre, off);
                };
            }
            return b;
        }
    }
}
