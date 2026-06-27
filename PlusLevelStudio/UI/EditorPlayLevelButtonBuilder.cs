using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using PlusLevelStudio.Menus;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class EditorPlayLevelButtonBuilder : UIElementBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            Vector2 center = Vector2.one / 2f;
            GameObject playBase = new GameObject(data["name"].Value<string>());
            playBase.transform.SetParent(parent.transform, false);
            RectTransform rectTransform = playBase.AddComponent<RectTransform>();
            rectTransform.anchorMin = center;
            rectTransform.anchorMax = center;
            rectTransform.pivot = center;
            rectTransform.sizeDelta = new Vector2(284f, 68f);
            rectTransform.anchoredPosition = ConvertToVector2(data["anchoredPosition"]); ;
            Image backImage = playBase.AddComponent<Image>();
            //backImage.color = new Color(0f,0f,0f, 0.5f);
            backImage.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("PlayLevelBorder");
            TextMeshProUGUI titleText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.BoldComicSans12, "My Awesome Level!", playBase.transform, Vector3.zero, false);
            titleText.name = "Title";
            titleText.rectTransform.anchorMin = center;
            titleText.rectTransform.anchorMax = center;
            titleText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            titleText.rectTransform.sizeDelta = new Vector2(164f, 18f);
            titleText.rectTransform.anchoredPosition = new Vector2(14f, 24f);
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.richText = false;
            titleText.overflowMode = TextOverflowModes.Ellipsis;

            TextMeshProUGUI authorText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans12, "By MissingTextureMan101", playBase.transform, Vector3.zero, false);
            authorText.name = "Author";
            authorText.rectTransform.anchorMin = center;
            authorText.rectTransform.anchorMax = center;
            authorText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            authorText.rectTransform.sizeDelta = new Vector2(136f, 16f);
            authorText.rectTransform.anchoredPosition = new Vector2(0f, 6f);
            authorText.alignment = TextAlignmentOptions.Left;
            authorText.overflowMode = TextOverflowModes.Ellipsis;
            authorText.richText = false;

            TextMeshProUGUI modeText = UIHelpers.CreateText<TextMeshProUGUI>(BaldiFonts.ComicSans12, "Hide & Seek", playBase.transform, Vector3.zero, false);
            modeText.name = "ModeTitle";
            modeText.rectTransform.anchorMin = center;
            modeText.rectTransform.anchorMax = center;
            modeText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            modeText.rectTransform.sizeDelta = new Vector2(136f, 16f);
            modeText.rectTransform.anchoredPosition = new Vector2(0f, -24f);
            modeText.alignment = TextAlignmentOptions.Left;


            GameObject playButtonBase = new GameObject("PlayButton");
            playButtonBase.transform.SetParent(playBase.transform, false);
            Image playButtonImage = playButtonBase.AddComponent<Image>();
            playButtonImage.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MPlayButton");
            playButtonImage.rectTransform.anchorMin = Vector2.one;
            playButtonImage.rectTransform.anchorMax = Vector2.one;
            playButtonImage.rectTransform.pivot = Vector2.one;
            playButtonImage.rectTransform.sizeDelta = Vector2.one * 32;
            playButtonImage.rectTransform.anchoredPosition = new Vector2(-2f, -2f);
            StandardMenuButton playButton = playButtonImage.gameObject.ConvertToButton<StandardMenuButton>();
            playButton.unhighlightedSprite = playButtonImage.sprite;
            playButton.swapOnHigh = true;
            playButton.highlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MPlayButtonHover");
            GameObject discardButtonBase = new GameObject("DiscardButton");
            discardButtonBase.transform.SetParent(playBase.transform, false);
            Image discardButtonImage = discardButtonBase.AddComponent<Image>();
            discardButtonImage.sprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MDiscardButton");
            discardButtonImage.rectTransform.anchorMin = Vector2.right;
            discardButtonImage.rectTransform.anchorMax = Vector2.right;
            discardButtonImage.rectTransform.pivot = Vector2.right;
            discardButtonImage.rectTransform.sizeDelta = Vector2.one * 32;
            discardButtonImage.rectTransform.anchoredPosition = new Vector2(-2f, -2f);
            StandardMenuButton discardButton = discardButtonImage.gameObject.ConvertToButton<StandardMenuButton>();
            discardButton.unhighlightedSprite = discardButtonImage.sprite;
            discardButton.swapOnHigh = true;
            discardButton.highlightedSprite = LevelStudioPlugin.Instance.assetMan.Get<Sprite>("MDiscardButtonHover");

            GameObject thumbBase = new GameObject("Thumbnail");
            thumbBase.transform.SetParent(playBase.transform, false);
            RawImage thumbImage = thumbBase.AddComponent<RawImage>();
            thumbImage.rectTransform.anchorMin = Vector2.zero;
            thumbImage.rectTransform.anchorMax = Vector2.zero;
            thumbImage.rectTransform.pivot = Vector2.zero;
            thumbImage.rectTransform.sizeDelta = Vector2.one * 64;
            thumbImage.rectTransform.anchoredPosition = new Vector2(2f, 2f);
            EditorPlayLevelButton button = playBase.AddComponent<EditorPlayLevelButton>();
            button.titleText = titleText;
            button.authorText = authorText;
            button.modeText = modeText;
            button.thumbnail = thumbImage;
            button.playButton = playButton;
            button.discardButton = discardButton;
            return button.gameObject;
        }
    }
}
