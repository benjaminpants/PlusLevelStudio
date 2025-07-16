using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MTM101BaldAPI.UI;

namespace PlusLevelStudio.UI
{
    public class TextBuilder : UIElementBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            TextMeshProUGUI baseText = UIHelpers.CreateText<TextMeshProUGUI>((BaldiFonts)Enum.Parse(typeof(BaldiFonts), data["font"].Value<string>()), data["text"].Value<string>(), parent.transform, Vector3.zero, false);
            baseText.transform.localPosition = Vector3.zero;
            baseText.transform.localEulerAngles = Vector3.zero;
            baseText.alignment = (TextAlignmentOptions)Enum.Parse(typeof(TextAlignmentOptions), data["alignment"].Value<string>());
            baseText.rectTransform.anchorMin = ConvertToVector2(data["anchorMin"]);
            baseText.rectTransform.anchorMax = ConvertToVector2(data["anchorMax"]);
            baseText.rectTransform.sizeDelta = ConvertToVector2(data["size"]);
            baseText.rectTransform.pivot = ConvertToVector2(data["pivot"]);
            baseText.rectTransform.anchoredPosition = ConvertToVector2(data["anchoredPosition"]);
            baseText.color = ConvertToColor(data["color"]);
            baseText.name = data["name"].Value<string>();
            if (data.ContainsKey("localized"))
            {
                if (data["localized"].Value<bool>() == true)
                {
                    TextLocalizer localizer = baseText.gameObject.AddComponent<TextLocalizer>();
                    localizer.key = baseText.text;
                    localizer.GetLocalizedText(localizer.key);
                }
            }
            return baseText.gameObject;
        }
    }
}
