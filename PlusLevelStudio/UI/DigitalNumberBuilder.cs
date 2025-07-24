using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class DigitalNumberBuilder : UIElementBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject segmentDisplay = new GameObject(data["name"].Value<string>());
            segmentDisplay.transform.SetParent(parent.transform, false);
            Image img = segmentDisplay.AddComponent<Image>();
            img.rectTransform.anchorMin = ConvertToVector2(data["anchor"]);
            img.rectTransform.anchorMax = ConvertToVector2(data["anchor"]);
            img.rectTransform.sizeDelta = new Vector2(32f,32f);
            img.rectTransform.pivot = ConvertToVector2(data["pivot"]);
            if (data.ContainsKey("color"))
            {
                img.color = ConvertToColor(data["color"]);
            }
            else
            {
                img.color = Color.red;
            }
            DigitalNumberDisplay display = segmentDisplay.AddComponent<DigitalNumberDisplay>();
            display.image = img;
            display.sprites = new Sprite[] {
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment0"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment1"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment2"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment3"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment4"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment5"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment6"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment7"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment8"),
                LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Segment9")
            };
            if (data.ContainsKey("default"))
            {
                display.currentValue = data["default"].Value<int>();
            }
            else
            {
                display.currentValue = 0;
            }
            img.rectTransform.anchoredPosition = ConvertToVector2(data["anchoredPosition"]);
            return segmentDisplay;
        }
    }

    public class DigitalNumberDisplay : MonoBehaviour
    {
        protected int _value = 0;
        public int currentValue {
            get
            {
                return _value;
            }
            set
            {
                if (value < 0)
                {
                    _value = sprites.Length - 1;
                }
                else
                {
                    _value = value % sprites.Length;
                }
                image.sprite = sprites[_value];
            }
        }
        public Sprite[] sprites;
        public Image image;
    }
}
