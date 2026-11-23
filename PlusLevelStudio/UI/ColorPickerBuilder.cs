using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.UI;
using Newtonsoft.Json.Linq;
using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class ColorPickerBuilder : ImageBuilder
    {
        public override GameObject Build(GameObject parent, UIExchangeHandler handler, Dictionary<string, JToken> data)
        {
            float size = data["size"].Value<float>();
            if (!data.ContainsKey("graphic"))
            {
                data["graphic"] = size < 256 ? "HSVWheelSmall" : "HSVWheel";
            }
            data["size"] = new JArray(size, size);
            GameObject builtImage = base.Build(parent, handler, data);
            GameObject pickerObject = new GameObject();
            pickerObject.transform.SetParent(builtImage.transform, false);
            pickerObject.transform.localScale = Vector3.one;
            pickerObject.transform.localPosition = Vector3.zero;
            Image pickerImage = pickerObject.AddComponent<Image>();
            pickerImage.name = "Picker";
            pickerImage.sprite = GetSprite("ColorPickerSymbol");
            pickerImage.rectTransform.pivot = Vector2.one * (2f/5f);
            pickerImage.rectTransform.sizeDelta = new Vector2(5f, 5f);
            pickerImage.raycastTarget = false;

            builtImage.gameObject.tag = "Button";
            ColorPicker colorPicker = builtImage.AddComponent<ColorPicker>();
            colorPicker.picker = pickerImage;
            colorPicker.baseImage = builtImage.GetComponent<Image>();
            colorPicker.detector = builtImage.AddComponent<MouseDragDetector>();

            GameObject brightnessSliderObject = new GameObject("BrightnessSlider");
            brightnessSliderObject.gameObject.tag = "Button";
            brightnessSliderObject.transform.SetParent(builtImage.transform, false);
            brightnessSliderObject.transform.localScale = Vector3.one;
            brightnessSliderObject.transform.localPosition = Vector3.zero;
            Image brightnessSliderImage = brightnessSliderObject.AddComponent<Image>();
            brightnessSliderImage.rectTransform.pivot = new Vector2(0.5f,0.5f);
            brightnessSliderImage.sprite = GetSprite("BrightnessSlider");
            brightnessSliderImage.rectTransform.sizeDelta = new Vector2(16f,128f);
            brightnessSliderImage.rectTransform.anchoredPosition = new Vector2(colorPicker.baseImage.rectTransform.sizeDelta.x/2 + 16, 0f);
            colorPicker.brightnessDetector = brightnessSliderObject.AddComponent<MouseDragDetector>();

            GameObject brightnessPickerObject = new GameObject("Picker");
            brightnessPickerObject.transform.SetParent(brightnessSliderObject.transform, false);
            brightnessPickerObject.transform.localScale = Vector3.one;
            brightnessPickerObject.transform.localPosition = Vector3.zero;
            Image brightnessPickerImage = brightnessPickerObject.AddComponent<Image>();
            brightnessPickerImage.sprite = GetSprite("TinySliderTick");
            brightnessPickerImage.rectTransform.pivot = new Vector2(0f,1f/3f);
            brightnessPickerImage.rectTransform.sizeDelta = new Vector2(5f, 3f);
            brightnessPickerImage.raycastTarget = false;
            colorPicker.brightnessPicker = brightnessPickerImage;

            GameObject colorPreviewObject = new GameObject("ColorPreview");
            colorPreviewObject.transform.SetParent(builtImage.transform, false);
            colorPreviewObject.transform.localScale = Vector3.one;
            colorPreviewObject.transform.localPosition = Vector3.zero;
            Image colorPreviewImage = colorPreviewObject.AddComponent<Image>();
            colorPreviewImage.sprite = GetSprite("ColorDisplaySmall");
            colorPreviewImage.rectTransform.pivot = new Vector2(0.5f,0.5f);
            colorPreviewImage.rectTransform.anchoredPosition = new Vector2(colorPicker.baseImage.rectTransform.sizeDelta.x / 2 + 40, colorPicker.baseImage.rectTransform.sizeDelta.y / 2f - 12f);
            colorPreviewImage.rectTransform.sizeDelta = new Vector2(24f,24f);
            colorPicker.colorPreview = colorPreviewImage;

            bool canExpand = (size < 256f) && EditorController.Instance != null;
            if (data.ContainsKey("expandable"))
            {
                canExpand &= data["expandable"].Value<bool>();
            }

            if (canExpand)
            {
                // the color picker expand button
                bool inverted = ConvertToColor(data["textColor"]).r < 0.5f;
                GameObject expandObject = new GameObject("ExpandButton");
                expandObject.transform.SetParent(builtImage.transform, false);
                expandObject.transform.localScale = Vector3.one;
                expandObject.transform.localPosition = Vector3.zero;
                Image expandImage = expandObject.AddComponent<Image>();
                expandImage.sprite = GetSprite(inverted ? "ThreeDotsInvert" : "ThreeDots");
                expandImage.rectTransform.pivot = new Vector2(0f, 1f);
                expandImage.rectTransform.sizeDelta = new Vector2(16f, 16f);
                expandImage.rectTransform.anchoredPosition = new Vector2(-colorPicker.baseImage.rectTransform.sizeDelta.x / 2, colorPicker.baseImage.rectTransform.sizeDelta.y / 2);
                StandardMenuButton expandButton = expandImage.gameObject.ConvertToButton<StandardMenuButton>();
                expandButton.swapOnHigh = true;
                expandButton.highlightedSprite = GetSprite(!inverted ? "ThreeDotsInvert" : "ThreeDots");
                expandButton.unhighlightedSprite = GetSprite(inverted ? "ThreeDotsInvert" : "ThreeDots");
                string expandedTitle = "Ed_ColorPicker_Generic";
                if (data.ContainsKey("expandedTitle"))
                {
                    expandedTitle = data["expandedTitle"].Value<string>();
                }
                expandButton.OnPress.AddListener(() =>
                {
                    ExpandedColorPickerMenuExchangeHandler expandedPickerMenu = EditorController.Instance.CreateUI<ExpandedColorPickerMenuExchangeHandler>("ColorPickerExpanded", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Elements", "ColorPickerExpanded.json"));
                    expandedPickerMenu.SetColor(colorPicker.outputColor);
                    expandedPickerMenu.targetColorPicker = colorPicker;
                    expandedPickerMenu.SetTitle(expandedTitle);
                });
            }


            colorPicker.Initialize();

            GameObject typableObjects = new GameObject("Typables");
            typableObjects.transform.SetParent(builtImage.transform, false);
            RectTransform tf = typableObjects.AddComponent<RectTransform>();
            tf.pivot = new Vector2(0f, 1f);
            tf.anchoredPosition = new Vector2(colorPicker.baseImage.rectTransform.sizeDelta.x / 2 + 28, colorPicker.baseImage.rectTransform.sizeDelta.y / 2f - 24f);
            ColorPickerExchangeHandler hand = UIBuilder.BuildUIFromFile<ColorPickerExchangeHandler>(tf, "ActualElements", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Elements", "ColorPickerClickables.json"), new Dictionary<string, JToken>()
            {
                { "localColor", data["textColor"] }
            });
            hand.colorPicker = colorPicker;
            colorPicker.pickerHandler = hand;
            colorPicker.parentHandler = handler;
            if (data.ContainsKey("onChange"))
            {
                colorPicker.parentMessage = data["onChange"].Value<string>();
            }
            if (data.ContainsKey("onChangeConfirm"))
            {
                colorPicker.parentMessageConfirm = data["onChangeConfirm"].Value<string>();
            }
            return builtImage;
        }
    }

    public class ExpandedColorPickerMenuExchangeHandler : EditorOverlayUIExchangeHandler
    {
        TextMeshProUGUI title;
        ColorPicker ourColorPicker;
        public ColorPicker targetColorPicker;
        public Action<Color> onExit;
        public override void OnElementsCreated()
        {
            ourColorPicker = transform.Find("ColorPicker").GetComponent<ColorPicker>();
            title = transform.Find("TitleText").GetComponent<TextMeshProUGUI>();
        }

        public void SetTitle(string text)
        {
            title.text = LocalizationManager.Instance.GetLocalizedText(text);
        }

        public void SetColor(Color color)
        {
            ourColorPicker.outputColor = color;
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "changeBase":
                case "changeBaseDone":
                    if (targetColorPicker != null)
                    {
                        targetColorPicker.outputColor = (Color)data;
                        targetColorPicker.SendUpdateMessage(message == "changeBaseDone");
                    }
                    return;
            }
            base.SendInteractionMessage(message, data);
        }

        public override bool OnExit()
        {
            if (onExit != null)
            {
                onExit(ourColorPicker.outputColor);
            }
            return true;
        }
    }

    public class ColorPickerExchangeHandler : UIExchangeHandler
    {
        public ColorPicker colorPicker;
        TextMeshProUGUI hexText;
        TextMeshProUGUI redText;
        TextMeshProUGUI greenText;
        TextMeshProUGUI blueText;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            hexText = transform.Find("HexCodeDisplay").GetComponent<TextMeshProUGUI>();
            redText = transform.Find("RedDisplay").GetComponent<TextMeshProUGUI>();
            greenText = transform.Find("GreenDisplay").GetComponent<TextMeshProUGUI>();
            blueText = transform.Find("BlueDisplay").GetComponent<TextMeshProUGUI>();
        }

        public void Refresh(bool refreshHex, bool refreshR = true, bool refreshG = true, bool refreshB = true)
        {
            if (refreshHex)
            {
                hexText.text = ColorUtility.ToHtmlStringRGB(colorPicker.outputColor);
            }
            if (refreshR)
            {
                redText.text = Mathf.RoundToInt(colorPicker.outputColor.r * 255f).ToString();
            }
            if (refreshG)
            {
                greenText.text = Mathf.RoundToInt(colorPicker.outputColor.g * 255f).ToString();
            }
            if (refreshB)
            {
                blueText.text = Mathf.RoundToInt(colorPicker.outputColor.b * 255f).ToString();
            }
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "red":
                case "redDone":
                    int r = 0;
                    int.TryParse((string)data, out r);
                    colorPicker.outputColor = new Color(Mathf.Clamp01(r / 255f), colorPicker.outputColor.g, colorPicker.outputColor.b);
                    Refresh(true, refreshR: message == "redDone");
                    colorPicker.SendUpdateMessage(message == "redDone");
                    break;
                case "green":
                case "greenDone":
                    int g = 0;
                    int.TryParse((string)data, out g);
                    colorPicker.outputColor = new Color(colorPicker.outputColor.r, Mathf.Clamp01(g / 255f), colorPicker.outputColor.b);
                    Refresh(true, refreshG: message == "greenDone");
                    colorPicker.SendUpdateMessage(message == "greenDone");
                    break;
                case "blue":
                case "blueDone":
                    int b = 0;
                    int.TryParse((string)data, out b);
                    colorPicker.outputColor = new Color(colorPicker.outputColor.r, colorPicker.outputColor.g, Mathf.Clamp01(b / 255f));
                    Refresh(true, refreshB: message == "blueDone");
                    colorPicker.SendUpdateMessage(message == "blueDone");
                    break;
                case "hexDone":
                case "hex":
                    if (ColorUtility.TryParseHtmlString("#" + (hexText.text.PadRight(6, '0')), out Color c))
                    {
                        colorPicker.RefreshColor(c, false);
                        Refresh(message == "hexDone");
                        colorPicker.SendUpdateMessage(message == "hexDone");
                    }
                    break;
                case "randomColor":
                    colorPicker.outputColor = new Color(UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                    colorPicker.SendUpdateMessage(true);
                    break;
            }
        }
    }

    public class ColorPicker : MonoBehaviour
    {
        public Image baseImage;
        public MouseDragDetector detector;
        public MouseDragDetector brightnessDetector;
        public Image colorPreview;
        public Image picker;
        public Image brightnessPicker;
        public ColorPickerExchangeHandler pickerHandler;
        public UIExchangeHandler parentHandler;
        public string parentMessage;
        public string parentMessageConfirm;
        bool initialized = false;
        Color[] pixels;
        int size = 0;
        Color chosenColor = Color.white;

        public Color outputColor
        {
            get
            {
                return chosenColor * baseImage.color;
            }
            set
            {
                RefreshColor(value, true);
            }
        }

        public void Initialize()
        {
            initialized = true;
            size = baseImage.sprite.texture.width;
            pixels = baseImage.sprite.texture.GetPixels(0, 0, size, size);
            detector.onDrag = OnDrag;
            brightnessDetector.onDrag = OnBrightnessDrag;
            UpdateBrightnessSelector();
        }

        void UpdateBrightnessSelector()
        {
            brightnessPicker.rectTransform.anchoredPosition = new Vector2(-5f, Mathf.Floor((baseImage.color.r * 122f) + 3f) - 64f);
        }

        void OnBrightnessDrag(Vector3 pos)
        {
            float brightness = (Mathf.Clamp((pos.y + 64f), 3f, 125f) - 3f) / 122f;
            baseImage.color = new Color(brightness,brightness,brightness,1f);
            UpdateBrightnessSelector();
            UpdateColorPreview();
            pickerHandler.Refresh(true);
            SendUpdateMessage(true);
        }

        public void RefreshColor(Color color, bool sendRefresh)
        {
            Color.RGBToHSV(color, out float h, out float s, out float v);
            baseImage.color = new Color(v, v, v);
            UpdateBrightnessSelector();

            Color targetColor = Color.HSVToRGB(h, s, 1f);
            chosenColor = targetColor;

            int closestIndex = 0;
            float closestScore = float.MaxValue;

            for (int i = 1; i < pixels.Length; i++)
            {
                if (pixels[i].a < 1f) continue;
                float score = CompareColors(pixels[i], targetColor);
                if (score < closestScore)
                {
                    closestScore = score;
                    closestIndex = i;
                }    
            }
            UpdatePicker(closestIndex);
            UpdateColorPreview();
            if (sendRefresh)
            {
                pickerHandler.Refresh(true);
            }
        }

        // Compares two colors, closer to 0 = closer.
        float CompareColors(Color a, Color b)
        {
            float rC = Mathf.Abs(a.r - b.r);
            float gC = Mathf.Abs(a.g - b.g);
            float bC = Mathf.Abs(a.b - b.b);
            return (rC * 10f) + (gC * 100f) + (bC * 1000f);
        }

        void OnDrag(Vector3 pos)
        {
            Vector2 off = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y)) + (baseImage.rectTransform.sizeDelta / 2); // todo: rewrite to handle other pivots?
            off.x = Mathf.Clamp(off.x, 0f, size - 1);
            off.y = Mathf.Clamp(off.y, 0f, size - 1);
            int attemptedIndex = ((int)off.x + ((int)off.y * size));
            while (pixels[attemptedIndex].a < 1f)
            {
                if (pos.x > 0)
                {
                    attemptedIndex--;
                }
                else
                {
                    attemptedIndex++;
                }
            }
            UpdatePicker(attemptedIndex);
            chosenColor = pixels[attemptedIndex];
            UpdateColorPreview();
            pickerHandler.Refresh(true);
            SendUpdateMessage(true);
            //picker.color = pixels[attemptedIndex];
        }

        void UpdatePicker(int index)
        {
            Vector2 chosenPixel = new Vector2(index % size, Mathf.FloorToInt(index / size));
            picker.rectTransform.anchoredPosition = chosenPixel - (baseImage.rectTransform.sizeDelta / 2);
        }

        void UpdateColorPreview()
        {
            colorPreview.color = outputColor;
        }

        public void SendUpdateMessage(bool confirm)
        {
            if (confirm && !string.IsNullOrEmpty(parentMessageConfirm))
            {
                parentHandler.SendInteractionMessage(parentMessageConfirm, outputColor);
            }
            else if (!confirm && !string.IsNullOrEmpty(parentMessage))
            {
                parentHandler.SendInteractionMessage(parentMessage, outputColor);
            }
        }
    }
}
