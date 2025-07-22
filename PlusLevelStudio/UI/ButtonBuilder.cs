using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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
            if (data.ContainsKey("onReleased"))
            {
                button.OnRelease.AddListener(() => handler.SendInteractionMessage(data["onReleased"].Value<string>()));
            }
            if (data.ContainsKey("tooltip"))
            {
                string key = data["tooltip"].Value<string>();
                button.eventOnHigh = true;
                button.OnHighlight.AddListener(() => EditorController.Instance.tooltipController.UpdateTooltip(key));
                button.OffHighlight.AddListener(() => EditorController.Instance.tooltipController.CloseTooltip());
            }
            if (data.ContainsKey("useAltOn"))
            {
                ChangeUnhighlightSpriteIfTrue hl = button.gameObject.AddComponent<ChangeUnhighlightSpriteIfTrue>();
                hl.handler = handler;
                hl.toCheck = data["useAltOn"].Value<string>();
                hl.specialSprite = GetSprite(data["altGraphic"]);
            }
            return b;
        }
    }

    public class ChangeUnhighlightSpriteIfTrue : MonoBehaviour
    {
        StandardMenuButton button;
        Sprite defaultSprite;
        Image renderer;
        public Sprite specialSprite;
        public UIExchangeHandler handler;
        public string toCheck;
        void Start()
        {
            button = GetComponent<StandardMenuButton>();
            renderer = GetComponent<Image>();
            defaultSprite = button.unhighlightedSprite;
        }
        void Update()
        {
            if (!button) return;
            if (handler.GetStateBoolean(toCheck))
            {
                button.unhighlightedSprite = specialSprite;
                if (renderer.sprite == defaultSprite)
                {
                    renderer.sprite = button.unhighlightedSprite;
                }
            }
            else
            {
                button.unhighlightedSprite = defaultSprite;
                if (renderer.sprite == specialSprite)
                {
                    renderer.sprite = button.unhighlightedSprite;
                }
            }
        }
    }
}
