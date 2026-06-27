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
            if (data.ContainsKey("onReleased"))
            {
                button.OnRelease.AddListener(() => handler.SendInteractionMessage(data["onReleased"].Value<string>()));
            }
            if (data.ContainsKey("transitionType"))
            {
                button.transitionOnPress = true;
                button.transitionTime = 0.0167f;
                button.transitionType = (UiTransition)Enum.Parse(typeof(UiTransition), data["transitionType"].Value<string>());
            }
            if (data.ContainsKey("onHighlight"))
            {
                button.eventOnHigh = true;
                button.OnHighlight.AddListener(() => handler.SendInteractionMessage(data["onHighlight"].Value<string>()));
            }
            return b;
        }
    }
}
