using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class BlockerBuilder : UIElementBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject baseObject = new GameObject("Blocker");
            baseObject.transform.SetParent(parent.transform, false);
            Image img = baseObject.AddComponent<Image>();
            img.rectTransform.anchorMin = new Vector2(0f, 1f);
            img.rectTransform.anchorMax = new Vector2(0f, 1f);
            img.rectTransform.sizeDelta = new Vector2(float.MaxValue, float.MaxValue);
            img.rectTransform.pivot = new Vector2(0f, 1f);
            img.color = Color.clear;
            img.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            return baseObject;
        }
    }
}
