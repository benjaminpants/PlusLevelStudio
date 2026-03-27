using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.UI
{
    public class HotSlotScript : MonoBehaviour
    {
        public int slotIndex = 0;
        private EditorTool _currentTool;
        public Image iconImage;
        public Image slotImage;
        public Sprite defaultSlotSprite;
        public bool usesToolOverride = false;
        public EditorTool currentTool {
            get
            {
                return _currentTool;
            }
            set
            {
                _currentTool = value;
                iconImage.color = Color.white;
                if (_currentTool == null)
                {
                    iconImage.sprite = null;
                    iconImage.color = Color.clear;
                    if (usesToolOverride)
                    {
                        slotImage.sprite = defaultSlotSprite;
                    }
                    return;
                }
                if (_currentTool.sprite == null)
                {
                    iconImage.sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/unknown");
                    if (usesToolOverride)
                    {
                        slotImage.sprite = defaultSlotSprite;
                    }
                    return;
                }
                iconImage.sprite = _currentTool.sprite;
                if (usesToolOverride)
                {
                    slotImage.sprite = (_currentTool == null || _currentTool.frameOverride == null) ? defaultSlotSprite : _currentTool.frameOverride;
                }
            }
        }
        public StandardMenuButton button;

        void Start()
        {
            button.OnPress.AddListener(ButtonPressed);
        }

        void Update()
        {
            if (currentTool == null) return;
            if (EditorController.Instance.currentTool == currentTool)
            {
                iconImage.color = Color.clear;
            }
            else
            {
                iconImage.color = Color.white;
            }
        }

        void ButtonPressed()
        {
            if (currentTool == null) return;
            EditorController.Instance.SwitchToTool(currentTool);
        }
    }
}
