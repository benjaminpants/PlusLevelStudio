using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class TextButtonBuilder : TextBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject b = base.Build(parent, handler, data);
            GameObject collision = new ImageBuilder().Build(parent, handler, data);
            collision.name += "_Collision";
            collision.GetComponent<Image>().color = Color.clear;
            StandardMenuButton button = collision.ConvertToButton<StandardMenuButton>(false);
            button.text = b.GetComponent<TextMeshProUGUI>();
            button.underlineOnHigh = true;
            button.swapOnHigh = false;
            if (data.ContainsKey("onPressed"))
            {
                button.OnPress.AddListener(() => handler.SendInteractionMessage(data["onPressed"].Value<string>()));
            }
            return b;
        }
    }
}
