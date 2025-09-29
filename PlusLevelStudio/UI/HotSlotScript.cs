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
                    return;
                }
                if (_currentTool.sprite == null)
                {
                    iconImage.sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/unknown");
                    return;
                }
                iconImage.sprite = _currentTool.sprite;
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
