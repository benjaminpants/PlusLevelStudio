using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using PlusLevelStudio.Editor;
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
            if (data.ContainsKey("tooltip"))
            {
                string key = data["tooltip"].Value<string>();
                button.eventOnHigh = true;
                button.OnHighlight.AddListener(() => EditorController.Instance.tooltipController.UpdateTooltip(key));
                button.OffHighlight.AddListener(() => EditorController.Instance.tooltipController.CloseTooltip());
            }
            return b;
        }
    }
}
