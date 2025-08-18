using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.SettingsUI
{
    public class CustomPosterSettingsUI : EditorOverlayUIExchangeHandler
    {
        public TextMeshProUGUI typableText;
        Transform typableTextCollision;
        TextMeshProUGUI richText;
        public Func<string, bool> onSubmit;
        public override void OnElementsCreated()
        {
            typableText = transform.Find("Poster").Find("Text").GetComponent<TextMeshProUGUI>();
            typableText.richText = false;
            typableTextCollision = transform.Find("Poster").Find("Text_Collision");
            richText = transform.Find("Poster").Find("RichText").GetComponent<TextMeshProUGUI>();
            richText.richText = true;
            SetRich(false);
        }

        public void SetRich(bool enabled)
        {
            if (enabled)
            {
                richText.text = LocalizationManager.Instance.GetLocalizedText(typableText.text);
            }
            richText.gameObject.SetActive(enabled);
            typableText.gameObject.SetActive(!enabled);
            typableTextCollision.gameObject.SetActive(!enabled);
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "toggleRich":
                    SetRich((bool)data);
                    return;
                case "ok":
                    if (onSubmit(typableText.text))
                    {
                        base.SendInteractionMessage("exit", null);
                    }
                    return;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
