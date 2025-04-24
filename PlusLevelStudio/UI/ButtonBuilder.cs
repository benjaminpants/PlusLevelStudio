using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.UI
{
    public class ButtonBuilder : ImageBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject b = base.Build(parent, handler, data);
            StandardMenuButton button = b.ConvertToButton<StandardMenuButton>();
            button.unhighlightedSprite = button.image.sprite;
            if (data.ContainsKey("hoverGraphic"))
            {
                button.highlightedSprite = GetSprite(data["hoverGraphic"]);
                button.swapOnHigh = true;
            }
            if (data.ContainsKey("onPressed"))
            {
                button.OnPress.AddListener(() => handler.SendInteractionMessage(data["onPressed"].Value<string>()));
            }
            return b;
        }
    }
}
