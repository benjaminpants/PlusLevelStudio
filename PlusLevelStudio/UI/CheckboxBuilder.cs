using HarmonyLib;
using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class CheckboxBuilder : UIElementBuilder
    {
        static FieldInfo _val = AccessTools.Field(typeof(MenuToggle), "val");
        static FieldInfo _checkmark = AccessTools.Field(typeof(MenuToggle), "checkmark");
        static FieldInfo _disableCover = AccessTools.Field(typeof(MenuToggle), "disableCover");
        static FieldInfo _hotspot = AccessTools.Field(typeof(MenuToggle), "hotspot");
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            GameObject baseObject = new GameObject(data["name"].Value<string>() + "_Visual");
            baseObject.transform.SetParent(parent.transform, false);
            Image boxImage = baseObject.AddComponent<Image>();
            boxImage.rectTransform.anchorMin = ConvertToVector2(data["anchorMin"]);
            boxImage.rectTransform.anchorMax = ConvertToVector2(data["anchorMax"]);
            boxImage.rectTransform.sizeDelta = Vector2.one * 32f;
            boxImage.rectTransform.pivot = ConvertToVector2(data["pivot"]);
            boxImage.sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("CheckBox");
            GameObject checkObject = new GameObject("Checkmark");
            checkObject.transform.SetParent(baseObject.transform, false);
            checkObject.transform.localScale = Vector3.one; // i still dont know why unity does this shit
            Image checkImage = checkObject.AddComponent<Image>();
            checkImage.rectTransform.anchorMin = ConvertToVector2(data["anchorMin"]);
            checkImage.rectTransform.anchorMax = ConvertToVector2(data["anchorMax"]);
            checkImage.rectTransform.sizeDelta = Vector2.one * 32f;
            checkImage.rectTransform.pivot = ConvertToVector2(data["pivot"]);
            checkImage.sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Check");

            boxImage.rectTransform.anchoredPosition = ConvertToVector2(data["anchoredPosition"]);
            checkImage.rectTransform.anchoredPosition = new Vector2(5f, 6f);
            checkImage.transform.localPosition = new Vector3(checkImage.transform.localPosition.x, checkImage.transform.localPosition.y, 0f); // what the fuck unity why

            GameObject buttonObject = new GameObject(data["name"].Value<string>());
            buttonObject.transform.SetParent(parent.transform, false);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.rectTransform.anchorMin = ConvertToVector2(data["anchorMin"]);
            buttonImage.rectTransform.anchorMax = ConvertToVector2(data["anchorMax"]);
            buttonImage.rectTransform.sizeDelta = Vector2.one * 32f;
            buttonImage.rectTransform.pivot = ConvertToVector2(data["pivot"]);
            buttonImage.rectTransform.anchoredPosition = ConvertToVector2(data["anchoredPosition"]);
            buttonImage.color = Color.clear;
            StandardMenuButton button = buttonImage.gameObject.ConvertToButton<StandardMenuButton>();
            MenuToggle menToggle = buttonImage.gameObject.AddComponent<MenuToggle>();
            _checkmark.SetValue(menToggle, checkImage.gameObject);
            _hotspot.SetValue(menToggle, buttonImage.gameObject);
            menToggle.Set(data["value"].Value<bool>());
            if (data.ContainsKey("onToggled"))
            {
                button.OnPress.AddListener(() =>
                {
                    menToggle.Set(!menToggle.Value);
                    handler.SendInteractionMessage(data["onToggled"].Value<string>(), menToggle.Value);
                });
            }

            if (data.ContainsKey("tooltip"))
            {
                string key = data["tooltip"].Value<string>();
                button.eventOnHigh = true;
                button.OnHighlight.AddListener(() => EditorController.Instance.tooltipController.UpdateTooltip(key));
                button.OffHighlight.AddListener(() => EditorController.Instance.tooltipController.CloseTooltip());
            }

            return buttonObject;
        }
    }
}
