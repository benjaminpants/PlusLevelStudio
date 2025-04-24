using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class HotSlotBuilder : UIElementBuilder
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
            img.sprite = GetSprite("HotslotBG");
            GameObject foregroundObject = new GameObject("FG");
            foregroundObject.transform.SetParent(baseObject.transform);
            foregroundObject.transform.localScale = Vector3.one;

            GameObject itemObject = new GameObject("Itm");
            itemObject.transform.SetParent(baseObject.transform);
            itemObject.transform.localScale = Vector3.one;
            Image itemImage = itemObject.AddComponent<Image>();
            itemImage.sprite = GetSprite("QuestionMark1");
            itemImage.rectTransform.sizeDelta = new Vector2(32f,32f);
            // ABSOLUTE NIGHTMARE
            itemImage.rectTransform.anchoredPosition3D = Vector3.zero;
            itemImage.transform.localPosition = Vector3.zero;
            itemImage.transform.localRotation = Quaternion.identity;
            itemImage.rectTransform.anchorMin = Vector2.zero;
            itemImage.rectTransform.anchorMax = Vector2.zero;
            itemImage.rectTransform.pivot = Vector2.zero;
            itemImage.rectTransform.anchoredPosition = new Vector2(4f,1f);
            // nightmare over

            Image foregroundImage = foregroundObject.AddComponent<Image>();
            foregroundImage.sprite = GetSprite(data["graphic"].Value<string>());
            foregroundImage.rectTransform.sizeDelta = img.rectTransform.sizeDelta;
            // ABSOLUTE NIGHTMARE
            foregroundImage.rectTransform.anchoredPosition3D = Vector3.zero;
            foregroundObject.transform.localPosition = Vector3.zero;
            foregroundObject.transform.localRotation = Quaternion.identity;
            foregroundImage.rectTransform.anchoredPosition = Vector2.zero;
            // nightmare over


            itemObject.transform.SetAsFirstSibling();
            img.rectTransform.anchoredPosition = ConvertToVector2(data["anchoredPosition"]);
            StandardMenuButton button = foregroundImage.gameObject.ConvertToButton<StandardMenuButton>();
            button.eventOnHigh = true;
            button.OnHighlight.AddListener(() =>
            {
                img.color = Color.red;
            });
            button.OffHighlight.AddListener(() =>
            {
                img.color = Color.white;
            });
            return baseObject;
        }
    }

    public class SpecialHotSlotBuilder : HotSlotBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject b = base.Build(parent, handler, data);

            Image itmImage = b.transform.Find("Itm").GetComponent<Image>();

            StandardMenuButton button = b.GetComponentInChildren<StandardMenuButton>();
            button.OnHighlight.AddListener(() =>
            {
                itmImage.sprite = GetSprite("ObjectBoxOpen");
            });
            button.OffHighlight.AddListener(() =>
            {
                itmImage.sprite = GetSprite("ObjectBoxClosed");
            });
            itmImage.sprite = GetSprite("ObjectBoxClosed");

            return b;
        }
    }
}
