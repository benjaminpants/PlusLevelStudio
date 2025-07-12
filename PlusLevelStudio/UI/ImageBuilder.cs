using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class ImageBuilder : UIElementBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject baseObject = new GameObject(data["name"].Value<string>());
            baseObject.transform.SetParent(parent.transform, false);
            Image img = baseObject.AddComponent<Image>();
            img.rectTransform.anchorMin = ConvertToVector2(data["anchorMin"]);
            img.rectTransform.anchorMax = ConvertToVector2(data["anchorMax"]);
            img.rectTransform.sizeDelta = ConvertToVector2(data["size"]);
            img.rectTransform.pivot = ConvertToVector2(data["pivot"]);
            if (data.ContainsKey("graphic"))
            {
                img.sprite = GetSprite(data["graphic"].Value<string>());
            }
            img.rectTransform.anchoredPosition = ConvertToVector2(data["anchoredPosition"]);
            return baseObject;
        }
    }
}
